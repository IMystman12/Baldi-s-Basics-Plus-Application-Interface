using System;
using System.Collections.Generic;
using System.Linq;
using BALDI_FULL_INTERFACE;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityInterface;
[HarmonyPatch]
public static class OptionsManager
{
    public static Category AddCategory(string key)
    {
        Category category = new Category()
        {
            key = key
        };
        categories.Add(category);
        return category;
    }
    static List<Category> categories = new List<Category>();
    [HarmonyPatch(typeof(OptionsMenu), "Awake"), HarmonyPostfix]
    internal static void Load(OptionsMenu __instance)
    {
        string[] categoryKeys = __instance.GetValue<string[]>("categoryKeys");
        GameObject[] categoriesGameObjects = __instance.GetValue<GameObject[]>("categories");
        GameObject gPre;
        foreach (var item in categories)
        {
            gPre = new GameObject(item.key);
            gPre.transform.SetParent(__instance.transform);
            gPre.transform.localPosition = Vector3.zero;
            gPre.transform.localRotation = Quaternion.identity;
            categoryKeys = categoryKeys.AddAs(item.key);
            categoriesGameObjects = categoriesGameObjects.AddAs(gPre);
        }
        __instance.SetValue("categoryKeys", categoryKeys);
        __instance.SetValue("categories", categoriesGameObjects);
        __instance.ChangeCategory(0);
    }

    public struct Category
    {
        public string key;
        public List<OptionElement> optionsElements;
    }
    public class OptionElement
    {
        public string nameKey;
        public string tooltipKey;
        public TMP_FontAsset fontToOverride;
        public UnityEvent onChanged;
        public virtual float height => 9f;
        public virtual OptionInstance instance => null;
        void Build(Vector2 headPosition) => instance?.Build(headPosition, this);
    }
    public class OptionInstance : MonoBehaviour
    {
        public void Build(Vector2 headPosition, OptionElement data)
        {

        }
    }
    public class StandardMenuButton : OptionElement
    {

    }
}
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
}
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
        public WeightedFilteredAssets<LevelObject> levelObject = new WeightedFilteredAssets<LevelObject>()
            , levelObjectForced = new WeightedFilteredAssets<LevelObject>()
            , levelObjectStore = new WeightedFilteredAssets<LevelObject>();
        public WeightedFilteredAssets<SceneObject> sceneStore = new WeightedFilteredAssets<SceneObject>();
        public WeightedFilteredAssets<FieldTripBaseRoomFunction> fieldTrip = new WeightedFilteredAssets<FieldTripBaseRoomFunction>();
        public WeightedFilteredAssets<ExtraLevelDataAsset> extraLevelDataAsset = new WeightedFilteredAssets<ExtraLevelDataAsset>();
        public WeightedFilteredAssets<LevelDataContainer> levelDataContainer = new WeightedFilteredAssets<LevelDataContainer>();
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

            WeightedItemObject weighted = new WeightedItemObject() { selection = itm, weight = levelObject.weight };
            levelObject.Load(l =>
            {
                l.potentialItems = l.potentialItems.AddAs(weighted);
                l.items = l.items.AddAs(weighted);
            });

            weighted = new WeightedItemObject() { selection = itm, weight = levelObjectStore.weight };
            levelObjectStore.Load(a => a.shopItems = a.shopItems.AddAs(weighted));
            levelObjectForced.Load(a => a.forcedItems.Add(itm));

            weighted = new WeightedItemObject() { selection = itm, weight = fieldTrip.weight };
            fieldTrip.Load(a => a.SetValue("potentialItems", a.GetValue<WeightedItemObject[]>("potentialItems").AddAs(weighted)));

            weighted = new WeightedItemObject() { selection = itm, weight = sceneStore.weight };
            sceneStore.Load(a => a.shopItems = a.shopItems.AddAs(weighted));

            weighted = new WeightedItemObject() { selection = itm, weight = extraLevelDataAsset.weight };
            extraLevelDataAsset.Load(a => a.potentialItems = a.potentialItems.AddAs(weighted));

            weighted = new WeightedItemObject() { selection = itm, weight = levelDataContainer.weight };
            levelDataContainer.Load(a => a.extraData.potentialItems = a.extraData.potentialItems.AddAs(weighted));
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
            WeightedSticker weighted = new WeightedSticker(sticker, scene.weight);
            scene.Load(scene => scene.potentialStickers = scene.potentialStickers.AddAs(weighted));
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
            WeightedRoomAsset weighted = new WeightedRoomAsset() { selection = roomAsset, weight = levelObject.weight };
            levelObject.Load(a => a.roomGroup.Where(b => groupNames.Contains(b.name)).ToList().ForEach(c => c.potentialRooms = c.potentialRooms.AddAs(weighted)));
        }
    }
    [Serializable]
    public class RoomGroupLoadingData : AssetLoadingData
    {
        public RoomGroup group;
        public WeightedFilteredAssets<LevelObject> levelObject = new WeightedFilteredAssets<LevelObject>();
        public override void Load() => levelObject.Load(a => a.roomGroup = a.roomGroup.AddAs(group));
    }
    #endregion
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
        public WeightedFilteredAssets<LevelObject> levelObject = new WeightedFilteredAssets<LevelObject>(), levelObjectForced = new WeightedFilteredAssets<LevelObject>();
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
            WeightedPosterObject weighted = new WeightedPosterObject() { selection = poster, weight = levelObject.weight };
            levelObject.Load(a => a.posters = a.posters.AddAs(weighted));
        }
    }
    [Serializable]
    public class NPCLoadingData : AssetLoadingData
    {
        public WeightedFilteredAssets<LevelObject> levelObject = new WeightedFilteredAssets<LevelObject>();
        public WeightedFilteredAssets<LevelDataContainer> levelDataContainer = new WeightedFilteredAssets<LevelDataContainer>();
        public WeightedFilteredAssets<ExtraLevelDataAsset> extraLevelDataAsset = new WeightedFilteredAssets<ExtraLevelDataAsset>();
        public WeightedFilteredAssets<SceneObject> scene = new WeightedFilteredAssets<SceneObject>(), sceneForced = new WeightedFilteredAssets<SceneObject>();
    }
    [Serializable]
    public class WeightedFilteredAssets<T> where T : UnityEngine.Object
    {
        public bool affect = true;
        public int weight = 100;
        public string[] excludeNames = new string[]
        {
            "F1"
        };
        public void Load(Action<T> action)
        {
            if (affect)
            {
                Resources.FindObjectsOfTypeAll<T>().Where(a => !excludeNames.Contains(a.name)).ToList().ForEach(action);
            }
        }
    }
    public static void Registe(this RandomEvent randomEvent, RandomEventLoadingData data)
    {
        WeightedRandomEvent weighted = new WeightedRandomEvent() { selection = randomEvent, weight = data.levelObject.weight };
        data.levelObject.Load(a => a.randomEvents.Add(weighted));

        weighted = new WeightedRandomEvent() { selection = randomEvent, weight = data.levelAsset.weight };
        data.levelAsset.Load(a => a.events.Add(randomEvent));

        weighted = new WeightedRandomEvent() { selection = randomEvent, weight = data.levelDataContainer.weight };
        data.levelDataContainer.Load(a => a.events.Add(randomEvent));
    }
    public static void Registe(this RandomEvent randomEvent, string dataName) => randomEvent.Registe(PluginCore.Instance.GetScriptableObjectOrCreate<RandomEventLoadingData>(dataName));
    public static void Registe(this RandomEvent randomEvent) => randomEvent.Registe(randomEvent.name);
    public static void Registe(this StructureBuilder structure, StructureLoadingData data)
    {
        data.structureWithParameters.prefab = structure;
        WeightedStructureWithParameters weighted = new WeightedStructureWithParameters() { selection = data.structureWithParameters, weight = data.levelObject.weight };
        data.levelObject.Load(a => a.potentialStructures = a.potentialStructures.AddAs(weighted));
        data.levelObjectForced.Load(a => a.forcedStructures = a.forcedStructures.AddAs(data.structureWithParameters));

        data.levelAsset.Load(a => a.randomGenStructures.Add(data.structureWithParameters));
        data.levelDataContainer.Load(a => a.randomGenStructures.Add(data.structureWithParameters));
    }
    public static void Registe(this StructureBuilder structure, string dataName) => structure.Registe(PluginCore.Instance.GetScriptableObjectOrCreate<StructureLoadingData>(dataName));
    public static void Registe(this StructureBuilder structure) => structure.Registe(structure.name);
    public static void Registe(this NPC nPC, NPCLoadingData data)
    {
        data.levelObject.Load(a => a.forcedNpcs = a.forcedNpcs.AddAs(nPC));

        WeightedNPC weighted = new WeightedNPC() { selection = nPC, weight = data.levelDataContainer.weight };
        data.levelDataContainer.Load(a => a.extraData.potentialNpcs.Add(weighted));

        weighted = new WeightedNPC() { selection = nPC, weight = data.levelDataContainer.weight };
        data.extraLevelDataAsset.Load(a => a.potentialNpcs.Add(weighted));

        weighted = new WeightedNPC() { selection = nPC, weight = data.scene.weight };
        data.scene.Load(a => a.potentialNPCs.Add(weighted));

        data.sceneForced.Load(a => a.forcedNpcs = a.forcedNpcs.AddAs(nPC));
    }
    public static void Registe(this NPC nPC, string dataName) => nPC.Registe(PluginCore.Instance.GetScriptableObjectOrCreate<NPCLoadingData>(dataName));
    public static void Registe(this NPC nPC) => nPC.Registe(nPC.name);
}