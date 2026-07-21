using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BALDI_FULL_INTERFACE;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityInterface;
using uobj = UnityEngine.Object;
[HarmonyPatch]
public static class OptionsManager
{
    internal static void Initialize()
    {
        try
        {
            var a = GameObject.FindObjectOfType<OptionsMenu>(true);
            tooltipBase = uobj.Instantiate(a.transform.Find("TooltipBase"), AssetManager.prefabParent).GetComponent<RectTransform>();
            tooltipBase.name = "TooltipBase";

            var g = new GameObject("TooltipController");
            g.transform.SetParent(AssetManager.prefabParent);

            tooltipPref = a.gameObject.AddComponent<TooltipController>();
            tooltipPref.name = "TooltipController";
            a.GetComponent<TooltipController>().Merge(tooltipPref);
        }
        catch (Exception e)
        {
            Debug.Log("Tooltip setup failed! " + e.ToString());
        }
        try
        {
            dropdownPref = uobj.Instantiate(GameObject.FindObjectOfType<HideSeekMenu>(true).transform.Find("PlayStyle"), AssetManager.prefabParent).GetComponent<RectTransform>();
            dropdownPref.name = "Dropdown";
        }
        catch (Exception e)
        {
            Debug.Log("Dropdown setup failed! " + e.ToString());
        }
        try
        {
            togglePref = uobj.Instantiate(GameObject.FindObjectOfType<HideSeekMenu>(true).GetComponentInChildren<MenuToggle>(true), AssetManager.prefabParent).GetComponent<MenuToggle>();
            togglePref.name = "Toggle";
        }
        catch (Exception e)
        {
            Debug.Log("Toggle setup failed! " + e.ToString());
        }
    }
    static TooltipController tooltipPref;
    static RectTransform tooltipBase, dropdownPref;
    static MenuToggle togglePref;
    public static TooltipController EnableTooltip(this Canvas canvas) => canvas.gameObject.AddComponent<TooltipController>().InitializeTooltip();
    public static TooltipController InitializeTooltip(this TooltipController emptyTooltip, int siblingIndex = -1)
    {
        tooltipPref.Merge(emptyTooltip);
        var a = uobj.Instantiate(tooltipBase.gameObject, emptyTooltip.transform);
        a.transform.SetSiblingIndex(siblingIndex > -1 ? siblingIndex : emptyTooltip.transform.childCount - 2);
        emptyTooltip.SetValue("tooltipTmp", a.GetComponentInChildren<TMP_Text>(true));
        emptyTooltip.SetValue("tooltipRect", a.transform.Find("Tooltip").GetComponent<RectTransform>());
        emptyTooltip.SetValue("tooltipBgRect", a.transform.Find("Tooltip/BG").GetComponent<RectTransform>());
        return emptyTooltip;
    }
    public static MenuToggle CreateToggle(Vector3 localPosition, Transform canvas, string title, bool defualtVal, UnityEvent<bool> onValueChanged, int siblingIndex = -1, string name = "InstancedToggle", string tooltip = "")
    {
        var e = uobj.Instantiate(togglePref, canvas.transform);
        e.Set(defualtVal);
        var r = e.GetComponent<RectTransform>();
        r.localPosition = localPosition;
        r.transform.SetSiblingIndex(siblingIndex > -1 ? siblingIndex : canvas.transform.childCount - 2);
        r.name = name;

        e.GetComponentInChildren<TMP_Text>(true).SetNewText(title);

        var b = e.GetComponentInChildren<StandardMenuButton>(true);

        b.OnPress = new UnityEvent();
        b.OnPress.AddListener(() => e.Toggle());
        b.OnPress.AddListener(() => onValueChanged.Invoke(e.Value));

        if (!string.IsNullOrEmpty(tooltip) && canvas.GetComponent<TooltipController>())
        {
            var c = canvas.GetComponent<TooltipController>();
            b.eventOnHigh = true;

            b.OnHighlight = new UnityEvent();
            b.OnHighlight.AddListener(() => c.UpdateTooltip(tooltip));

            b.OffHighlight = new UnityEvent();
            b.OffHighlight.AddListener(() => c.CloseTooltip());
        }

        return e;
    }
    public static TMP_Text CreateDropdown(Vector3 localPosition, Transform canvas, string title, UnityEvent<TMP_Text> leftButtonChanged, UnityEvent<TMP_Text> rightButtonChanged, int siblingIndex = -1, string name = "InstancedDropdown")
    {
        var e = uobj.Instantiate(dropdownPref, canvas.transform);
        e.localPosition = localPosition;
        e.SetSiblingIndex(siblingIndex > -1 ? siblingIndex : canvas.transform.childCount - 2);
        e.name = name;

        var t = e.Find("StyleDisplay").GetComponent<TMP_Text>();

        e.Find("StyleTitle").GetComponent<TMP_Text>().SetNewText(title);
        var s = e.Find("LeftCategoryButton").GetComponent<StandardMenuButton>();
        s.OnPress = new UnityEvent();
        s.OnPress.AddListener(() => leftButtonChanged.Invoke(t));
        s = e.Find("RightCategoryButton").GetComponent<StandardMenuButton>();
        s.OnPress = new UnityEvent();
        s.OnPress.AddListener(() => rightButtonChanged.Invoke(t));

        return t;
    }
    public static Category AddCategory(string key)
    {
        Category category = new Category()
        {
            key = key
        };
        categories.Add(category);
        return category;
    }
    [Serializable]
    public struct Category
    {
        public string key;
    }
    [HarmonyPatch(typeof(OptionsMenu), "Awake"), HarmonyPostfix]
    internal static void Load(OptionsMenu __instance)
    {
        string[] categoryKeys = __instance.GetValue<string[]>("categoryKeys");
        GameObject[] categoriesGameObjects = __instance.GetValue<GameObject[]>("categories");
        GameObject gPre;
        foreach (var a in categories)
        {
            gPre = new GameObject(a.key);
            gPre.transform.SetParent(__instance.transform);
            gPre.transform.localPosition = Vector3.zero;
            gPre.transform.localRotation = Quaternion.identity;
            categoryKeys = categoryKeys.AddAs(a.key);
            categoriesGameObjects = categoriesGameObjects.AddAs(gPre);
            onOptionCategoryLoaded.Invoke(a.key, __instance);
        }
        __instance.SetValue("categoryKeys", categoryKeys);
        __instance.SetValue("categories", categoriesGameObjects);
        __instance.ChangeCategory(0);
    }
    static List<Category> categories = new List<Category>();
    public delegate void OnOptionCategoryLoaded(string key, OptionsMenu menu);
    public static OnOptionCategoryLoaded onOptionCategoryLoaded;
}
/// <summary>
/// Use "using static GeneralActions;" for better performance!
/// </summary>
public static class GeneralActions
{
    public static Vector2 Converted(this Vector3 position) => new Vector2(position.x, position.z);
    public static void Open(this Door door) => door.Open(door.makesNoise);
    public static void Open(this Door door, bool makeNoise)
    {
        if (door is StandardDoor sd)
        {
            sd.OpenTimed(float.PositiveInfinity, makeNoise);
        }
        if (door is SwingDoor sd0)
        {
            sd0.OpenTimed(float.PositiveInfinity, makeNoise);
        }
        if (door is LockdownDoor ld)
        {
            ld.Open(false, makeNoise);
        }
        else
        {
            door.Open(false, makeNoise);
            if (!door.IsOpen)
            {
                door.OpenTimed(float.PositiveInfinity, makeNoise);
            }
        }
    }
    public static void OpenTimed(this Door door, float time, bool makeNoise) => door.OpenTimed(time, makeNoise);
    public static void UnlockGeneral(this Door door)
    {
        if (door is GameLock gl)
        {
            GameObject.FindObjectsOfType<LockedRoomFunction>(true).Where(a => a.Room.doors.Contains(door)).ToList().ForEach(a => a.RemoveLocks());
        }
        door.Unlock();
    }
    public static void OpenTimedWithKey(this Door door, float time = 2) => door.OpenTimedWithKey(time, door.makesNoise);
    public static void OpenTimedWithKey(this Door door, float time, bool makeNoise)
    {
        door.OpenTimed(time, makeNoise);
        door.UnlockGeneral();
    }
    public static void SetPower(this EnvironmentController ec, bool val)
    {
        foreach (RoomController room in ec.rooms)
        {
            room.SetPower(val);
        }

        ec.mainHall.SetPower(val);
    }
    public static void SetGlobalLight(LightMode lightMode, int standardLightStrength, Color minLightColor)
    {
        Resources.FindObjectsOfTypeAll<LevelObject>().ToList().ForEach(a =>
        {
            a.lightMode = lightMode;
            a.standardLightStrength = standardLightStrength;
        });
        Resources.FindObjectsOfTypeAll<ExtraLevelDataAsset>().ToList().ForEach(a =>
        {
            a.lightMode = lightMode;
            a.minLightColor = minLightColor;
        });
        Resources.FindObjectsOfTypeAll<LevelDataContainer>().ToList().ForEach(a =>
        {
            a.extraData.lightMode = lightMode;
            a.extraData.minLightColor = minLightColor;
        });
    }
    public static void PlaySingle(this AudioManager audioManager, SoundObject audSound, bool loop)
    {
        if (loop)
        {
            audioManager.FlushQueue(endCurrent: true);
            audioManager.QueueAudio(audSound);
            audioManager.SetLoop(val: true);
        }
        else
        {
            audioManager.PlaySingle(audSound);
        }
    }
    public static bool CellsConnected(this EnvironmentController ec, Cell cellA, Cell cellB)
    {
        Direction direction = Directions.FromPointAToB(cellA.position, cellB.position);
        return Directions.OpenDirectionsFromBin(cellA.ConstBin).Contains(direction) && Directions.OpenDirectionsFromBin(cellB.ConstBin).Contains(direction.GetOpposite());
    }
    public static List<Cell> GetConnectedNeighbors(this EnvironmentController ec, Cell cell, bool sameRoom)
    {
        List<Cell> result = ec.GetCellNeighbors(cell.position).Where(a => (!sameRoom || a.room == cell.room) && ec.CellsConnected(cell, a)).ToList();
        result.Add(cell);
        return result;
    }
    public static List<Cell> GetNeighborsAtSameRoom(this EnvironmentController ec, Cell cell) => ec.GetCellNeighbors(cell.position).Where(a => a.room == cell.room).ToList();
    public static T BuildInRoom<T>(this T buttonPref, System.Random cRng, RoomController room) where T : GameButtonBase
    {
        Cell cell = room.GetNewTileList().Where(a => a.HasSoftFreeWall).Random(cRng);

        if (cell != null)
        {
            return (T)GameButton.Build(buttonPref, room.ec, cell.position, cell.RandomUncoveredDirection(cRng));
        }
        return null;
    }
    public static bool FitToBuildButtonBase(this RoomController room) => room.GetNewTileList().Where(a => a.HasSoftFreeWall).FirstOrDefault() != null;
    public static int Distance(this IntVector2 a, IntVector2 b)
    {
        IntVector2 c = a - b;
        return Mathf.Abs(c.x) + Mathf.Abs(c.z);
    }
    public static void Transition(this GlobalCam globalCam, UiTransition uiTransition = UiTransition.Dither) => globalCam.Transition(uiTransition, WaitForTransition.DitherTransitionTime);
    /// <summary>
    /// From game to interface.
    /// </summary>
    /// <param name="globalCam"></param>
    /// <param name="uiTransition"></param>
    public static void FadeIn(this GlobalCam globalCam, UiTransition uiTransition = UiTransition.Dither) => globalCam.FadeIn(uiTransition, WaitForTransition.DitherTransitionTime);
    public static void StartGame(this GameLoader loader, SceneObject scene, ElevatorScreen elevatorScreen = null, int liveIndex = 0, Mode gameMode = Mode.Main)
    {
        loader.Initialize(liveIndex);
        loader.SetMode((int)gameMode);
        if (elevatorScreen)
        {
            elevatorScreen.gameObject.SetActive(true);
            loader.AssignElevatorScreen(elevatorScreen);
        }
        loader.LoadLevel(scene);
    }
    public static void StartGame(this GameLoader loader, SceneObject scene, bool useElevator, int liveIndex = 0, Mode gameMode = Mode.Main) => StartGame(loader, scene, useElevator ? loader.FindWithInactive<ElevatorScreen>("ElevatorScreen") : null, liveIndex, gameMode);
    public static Coroutine WaitForGameLoading() => GlobalCam.Instance.StartCoroutine(WaitForGameLoadingIE());
    public static IEnumerator WaitForGameLoadingIE()
    {
        yield return new WaitUntil(() => CoreGameManager.Instance && BaseGameManager.Instance);
        var lb = GameObject.FindObjectOfType<LevelBuilder>();
        yield return new WaitUntil(() => !(lb.levelInProgress && !lb.levelCreated));
    }
    public static void FixFonts(this GameObject gameObject) => gameObject.GetComponentsInChildren<TMP_Text>(true).ToList().ForEach(a => a.font = Resources.Load<TMP_FontAsset>($"Comic_{a.fontSize}_Pro"));
    public static T[] GetNPCs<T>(this EnvironmentController ec) where T : NPC => ec.Npcs.Where(a => a is T).Select(a => (T)a).ToArray();
    public static void SafeTeleport(this Entity entity, Vector3 position)
    {
        entity.SetInteractionState(false);
        entity.SetFrozen(true);
        entity.Teleport(position);
        entity.SetFrozen(false);
        entity.SetInteractionState(true);
    }
    public static void FixCursors(this GameObject gameObject) => gameObject.GetComponentsInChildren<CursorInitiator>(true).ToList().ForEach(a => a.cursorPre = a.cursorPre ?? Resources.Load<CursorController>("CursorOrigin"));
    public static void FixBacks(this GameObject gameObject) => gameObject.GetComponentsInChildren<StandardMenuButton>(true).Where(a => a.name == "Back").ToList().ForEach(a =>
    {
        Sprite su = Resources.Load<Sprite>("BackArrow_0"), sl = Resources.Load<Sprite>("BackArrow_1");
        a.image.sprite = su;
        a.highlightedSprite = sl;
        a.unhighlightedSprite = su;
        a.heldSprite = sl;
    });
    public static void FixButtons(this GameObject gameObject) => gameObject.GetComponentsInChildren<StandardMenuButton>(true).ToList().ForEach(a => a.tag = "Button");
    public static void FixTooltips(this GameObject gameObject) => gameObject.GetComponentsInChildren<TooltipController>(true).ToList().ForEach(a => a.InitializeTooltip());
    public static void SetNewText(this TMP_Text text, string key)
    {
        if (text == null)
        {
            Debug.LogWarning("No text component found! " + StackTraceUtility.ExtractStackTrace());
            return;
        }
        if (text.GetComponent<TextLocalizer>())
        {
            var t = text.GetComponent<TextLocalizer>();
            if (!t.GetValue<TMP_Text>("textBox"))
            {
                t.SetValue("textBox", text);
            }
            t.key = key;
            t.GetLocalizedText(key);
        }
        else
        {
            text.text = key;
        }
    }
}
public class WaitForTransition : CustomYieldInstruction
{
    public const float DitherTransitionTime = 0.01666667f;
    public static WaitForTransition Instance => instance;
    static WaitForTransition instance = new WaitForTransition();
    public override bool keepWaiting => GlobalCam.Instance.TransitionActive;
}
/// <summary>
/// Registe random generatables. If there aren't any Registe method fits your request,use LOADINGDATAs
/// </summary>
public static class Register
{
    #region "Loader"
    [Serializable]
    public class AssetLoadingData : ScriptableObject
    {
        public virtual void Load()
        {

        }
    }
    [Serializable]
    public class ItemLoadingData : AssetLoadingData
    {
        public ItemObject itm;
        public WeightedFilteredAssets<LevelObject> levelObject = new WeightedFilteredAssets<LevelObject>(true)
            , levelObjectForced = new WeightedFilteredAssets<LevelObject>(false)
            , levelObjectStore = new WeightedFilteredAssets<LevelObject>();
        public WeightedFilteredAssets<SceneObject> sceneStore = new WeightedFilteredAssets<SceneObject>();
        public WeightedFilteredAssets<FieldTripBaseRoomFunction> fieldTrip = new WeightedFilteredAssets<FieldTripBaseRoomFunction>();
        public WeightedFilteredAssets<ExtraLevelDataAsset> extraLevelDataAsset = new WeightedFilteredAssets<ExtraLevelDataAsset>();
        public WeightedFilteredAssets<LevelDataContainer> levelDataContainer = new WeightedFilteredAssets<LevelDataContainer>();
        public WeightedFilteredAssets<SodaMachine> vendingMachines = new WeightedFilteredAssets<SodaMachine>(false);
        public bool stickToSlot;
        public override void Load()
        {
            if (!itm)
            {
                Debug.LogWarning("No item found!");
                return;
            }
            PlayerFileManager.Instance.itemObjects.Add(itm);
            if (stickToSlot)
            {
                PluginCore.itemToStick.Add(itm);
            }

            levelObject.Load((l, w) =>
            {
                WeightedItemObject weighted = new WeightedItemObject() { selection = itm, weight = w };
                l.potentialItems = l.potentialItems.AddAs(weighted);
                l.items = l.items.AddAs(weighted);
            });

            levelObjectStore.Load((a, w) => a.shopItems = a.shopItems.AddAs(new WeightedItemObject() { selection = itm, weight = w }));
            levelObjectForced.Load((a, w) => a.forcedItems.Add(itm));
            fieldTrip.Load((a, w) => a.SetValue("potentialItems", a.GetValue<WeightedItemObject[]>("potentialItems").AddAs(new WeightedItemObject() { selection = itm, weight = w })));
            sceneStore.Load((a, w) => a.shopItems = a.shopItems.AddAs(new WeightedItemObject() { selection = itm, weight = w }));
            extraLevelDataAsset.Load((a, w) => a.potentialItems = a.potentialItems.AddAs(new WeightedItemObject() { selection = itm, weight = w }));
            levelDataContainer.Load((a, w) => a.extraData.potentialItems = a.extraData.potentialItems.AddAs(new WeightedItemObject() { selection = itm, weight = w }));
            vendingMachines.Load((a, w) => a.SetValue("potentialItems", a.GetValue<WeightedItemObject[]>("potentialItems").AddAs(new WeightedItemObject() { selection = itm, weight = w })));
        }
    }
    [Serializable]
    public class SubtitleObject : ScriptableObject
    {
        public LocalizationData localization = new LocalizationData();
    }
    [Serializable]
    public class StickerLoadingData : AssetLoadingData
    {
        public Sticker sticker;
        public Sprite sprite;
        public float duplicateOddsMultiplier = 1f;
        public bool affectsLevelGeneration;
        public WeightedFilteredAssets<SceneObject> scene = new WeightedFilteredAssets<SceneObject>();
        public void LoadInstanced()
        {
            List<StickerData> array = StickerManager.Instance.GetValue<StickerData[]>("stickerData").ToList();
            int i = (int)sticker;
            while (i > (array.Count - 1))
            {
                array.Add(new StickerData());
            }
            array[i].sprite = sprite;
            array[i].duplicateOddsMultiplier = duplicateOddsMultiplier;
            array[i].affectsLevelGeneration = affectsLevelGeneration;
            StickerManager.Instance.SetValue("stickerData", array.ToArray());
        }
        public override void Load()
        {
            scene.Load((scene, w) => scene.potentialStickers = scene.potentialStickers.AddAs(new WeightedSticker(sticker, w)));
        }
    }
    [Serializable]
    public class RoomAssetLoadingData : AssetLoadingData
    {
        public string[] groupNames;
        public RoomAsset roomAsset;
        public WeightedFilteredAssets<LevelObject> levelObject = new WeightedFilteredAssets<LevelObject>();
        public override void Load()
        {
            levelObject.Load((a, w) => a.roomGroup.Where(b => groupNames.Contains(b.name)).ToList().ForEach(c => c.potentialRooms = c.potentialRooms.AddAs(new WeightedRoomAsset() { selection = roomAsset, weight = w })));
        }
    }
    [Serializable]
    public class RoomGroupLoadingData : AssetLoadingData
    {
        public RoomGroup group;
        public WeightedFilteredAssets<LevelObject> levelObject = new WeightedFilteredAssets<LevelObject>();
        public override void Load() => levelObject.Load((a, w) => a.roomGroup = a.roomGroup.AddAs(group));
    }
    #endregion
    #region "Loading Datas"
    [Serializable]
    public class RandomEventLoadingData : AssetLoadingData
    {
        public WeightedFilteredAssets<LevelObject> levelObject = new WeightedFilteredAssets<LevelObject>();
        public WeightedFilteredAssets<LevelAsset> levelAsset = new WeightedFilteredAssets<LevelAsset>();
        public WeightedFilteredAssets<LevelDataContainer> levelDataContainer = new WeightedFilteredAssets<LevelDataContainer>();
    }
    [Serializable]
    public class StructureLoadingData : AssetLoadingData
    {
        public StructureWithParameters structureWithParameters = new StructureWithParameters();
        public WeightedFilteredAssets<LevelObject> levelObject = new WeightedFilteredAssets<LevelObject>(), levelObjectForced = new WeightedFilteredAssets<LevelObject>(false);
        public WeightedFilteredAssets<LevelAsset> levelAsset = new WeightedFilteredAssets<LevelAsset>();
        public WeightedFilteredAssets<LevelDataContainer> levelDataContainer = new WeightedFilteredAssets<LevelDataContainer>();
    }
    [Serializable]
    public class PosterLoadingData : AssetLoadingData
    {
        public PosterObject poster;
        public WeightedFilteredAssets<LevelObject> levelObject = new WeightedFilteredAssets<LevelObject>();
        public override void Load()
        {
            levelObject.Load((a, w) => a.posters = a.posters.AddAs(new WeightedPosterObject() { selection = poster, weight = w }));
        }
    }
    [Serializable]
    public class NPCLoadingData : AssetLoadingData
    {
        public WeightedFilteredAssets<LevelObject> levelObject = new WeightedFilteredAssets<LevelObject>();
        public WeightedFilteredAssets<LevelDataContainer> levelDataContainer = new WeightedFilteredAssets<LevelDataContainer>();
        public WeightedFilteredAssets<ExtraLevelDataAsset> extraLevelDataAsset = new WeightedFilteredAssets<ExtraLevelDataAsset>();
        public WeightedFilteredAssets<SceneObject> scene = new WeightedFilteredAssets<SceneObject>(), sceneForced = new WeightedFilteredAssets<SceneObject>(false);
    }
    [Serializable]
    public class WeightedFilteredAssets<T> where T : UnityEngine.Object
    {
        public WeightedFilteredAssets()
        {

        }
        public WeightedFilteredAssets(bool affectNew) => affect = affectNew;
        public bool affect = true;
        public int weight = 100;
        public string[] excludeNames = new string[]
        {
            "F1"
        };
        public WeightedSelection<string>[] specificedWeights = new WeightedSelection<string>[]
        {
       new WeightedSelection<string>()
       {
               selection= "F1",
            weight=99
       }
        };
        public void Load(Action<T, int> action)
        {
            if (affect)
            {
                WeightedSelection<string> weightedOverried;
                foreach (var item in Resources.FindObjectsOfTypeAll<T>().Where(a => !excludeNames.Contains(a.name)))
                {
                    weightedOverried = specificedWeights.Where(a => a.selection == item.name).FirstOrDefault();
                    action(item, weightedOverried != null ? weightedOverried.weight : weight);
                }
            }
        }
    }
    #endregion
    public static void Registe(this RandomEvent randomEvent, RandomEventLoadingData data)
    {
        data.levelObject.Load((a, w) => a.randomEvents.Add(new WeightedRandomEvent() { selection = randomEvent, weight = w }));
        data.levelAsset.Load((a, w) => a.events.Add(randomEvent));
        data.levelDataContainer.Load((a, w) => a.events.Add(randomEvent));
    }
    public static void Registe(this RandomEvent randomEvent, string dataName) => randomEvent.Registe(PluginCore.Instance.GetScriptableObjectOrCreate<RandomEventLoadingData>(dataName));
    public static void Registe(this RandomEvent randomEvent) => randomEvent.Registe(randomEvent.name);
    public static void Registe(this StructureBuilder structure, StructureLoadingData data)
    {
        data.structureWithParameters.prefab = structure;
        data.levelObject.Load((a, w) => a.potentialStructures = a.potentialStructures.AddAs(new WeightedStructureWithParameters() { selection = data.structureWithParameters, weight = w }));
        data.levelObjectForced.Load((a, w) => a.forcedStructures = a.forcedStructures.AddAs(data.structureWithParameters));
        data.levelAsset.Load((a, w) => a.randomGenStructures.Add(data.structureWithParameters));
        data.levelDataContainer.Load((a, w) => a.randomGenStructures.Add(data.structureWithParameters));
    }
    public static void Registe(this StructureBuilder structure, string dataName) => structure.Registe(PluginCore.Instance.GetScriptableObjectOrCreate<StructureLoadingData>(dataName));
    public static void Registe(this StructureBuilder structure) => structure.Registe(structure.name);
    public static void Registe(this NPC nPC, NPCLoadingData data)
    {
        data.levelObject.Load((a, w) => a.forcedNpcs = a.forcedNpcs.AddAs(nPC));
        data.levelDataContainer.Load((a, w) => a.extraData.potentialNpcs.Add(new WeightedNPC() { selection = nPC, weight = w }));
        data.extraLevelDataAsset.Load((a, w) => a.potentialNpcs.Add(new WeightedNPC() { selection = nPC, weight = w }));
        data.scene.Load((a, w) => a.potentialNPCs.Add(new WeightedNPC() { selection = nPC, weight = w }));
        data.sceneForced.Load((a, w) => a.forcedNpcs = a.forcedNpcs.AddAs(nPC));
    }
    public static void Registe(this NPC nPC, string dataName) => nPC.Registe(PluginCore.Instance.GetScriptableObjectOrCreate<NPCLoadingData>(dataName));
    public static void Registe(this NPC nPC) => nPC.Registe(nPC.name);
}