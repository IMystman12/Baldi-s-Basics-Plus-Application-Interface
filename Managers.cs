using System;
using System.Collections.Generic;
using UnityEngine;
using UnityInterface;
namespace BALDI_FULL_INTERFACE
{
    public static class OptionsManager
    {
        private static Dictionary<string, Dictionary<string, OptionType>> options = new Dictionary<string, Dictionary<string, OptionType>>();
        private static Dictionary<string, Dictionary<string, MonoBehaviour>> instances = new Dictionary<string, Dictionary<string, MonoBehaviour>>();
        private static Dictionary<string, Dictionary<string, object>> values = new Dictionary<string, Dictionary<string, object>>();
        public static OnSthOutputInSingle onInitialize;
        private static AdjustmentBars _bars;
        private static MenuToggle _toggle;
        public enum OptionType
        {
            Null,
            Silder,
            Toggle,
            Category
        }
        public static void Initialize(OptionsMenu menu)
        {
            _bars = menu.GetValue<AdjustmentBars>("sensitivityAdj");
            onInitialize?.Invoke(menu);
        }
        public static object GetInstance(string name = "", string category = "")
        {
            if (!options.ContainsKey(category) || !options[category].ContainsKey(name))
            {
                Debug.LogWarning("Name: " + name + " in Category: " + category + " wasn't added. Please add from OptionsManager.Add!");
                return 0;
            }
            return instances[category][name];
        }
        public static object GetValue(string name = "", string category = "")
        {
            if (!options.ContainsKey(category) || !options[category].ContainsKey(name))
            {
                Debug.LogWarning("Name: " + name + " in Category: " + category + " wasn't added. Please add from OptionsManager.Add!");
                return 0;
            }
            return values[category][name];
        }
        public static void Add(string name = "", string category = "", OptionType type = OptionType.Null)
        {
            if (!options.ContainsKey(category))
            {
                options.Add(category, new Dictionary<string, OptionType>());
                values.Add(category, new Dictionary<string, object>());
                instances.Add(category, new Dictionary<string, MonoBehaviour>());
            }
            if (name != string.Empty)
            {
                options[category].Add(name, type);
                values[category].Add(name, null);
                instances[category].Add(name, null);
            }
        }
    }
    public class AssetInjector
    {
        public class SpawnConfig
        {
            public int weight = 100;
            public List<string> excludeLevelTitles = new List<string>();
        }
        public static (List<SceneObject>, string[]) GetAllSceneObjectWithoutExcludeWithLevelObjectNamesInExclude(SpawnConfig cfg)
        {
            string[] strs = new string[0];
            List<SceneObject> sol = new List<SceneObject>();
            foreach (var item in AssetManager.GetGameAssetsFromType<SceneObject>())
            {
                if (!cfg.excludeLevelTitles.Contains(item.levelTitle))
                {
                    sol.Add(item);
                }
                else if (item.levelObject)
                {
                    strs.Add(item.levelObject.name);
                }
            }
            return (sol, strs);
        }
        public static void AddItem(ItemObject itm, ItemConfig cfg = default)
        {
            if (cfg == null)
            {
                cfg = new ItemConfig();
            }
            if (cfg.saveable)
            {
                PlayerFileManager.Instance.itemObjects.Add(itm);
            }
            if (cfg.spawnInLevel)
            {
                WeightedItemObject wio = new WeightedItemObject() { selection = itm, weight = cfg.weight };
                (List<SceneObject>, string[]) v = GetAllSceneObjectWithoutExcludeWithLevelObjectNamesInExclude(cfg);
                foreach (var item in v.Item1)
                {
                    if (cfg.spawnInStore)
                    {
                        item.shopItems.Add(wio);
                    }
                }
                object[][] inp = new object[][]
            {
                new object[4]{
          cfg.addItemsInLevel,
          cfg.forceItemsInLevel,
          cfg.potentialItemsInLevel,
cfg.shopItemsInLevel
                },
                new object[4]{
          "ary",
         "lit_Without_Weight",
         "ary",
"ary"
                },
                new object[4]{
          "items",
         "forcedItems",
         "potentialItems",
"shopItems"
                },
            };
                Collections.RepeatOperations(
                    (object[] args) =>
                    {
                        if (Convert.ToBoolean(args[0]))
                        {
                            switch (args[1])
                            {
                                case "ary":
                                    AssetManager.AddGlobalValueToArray<LevelObject, WeightedItemObject>(Convert.ToString(args[2]), wio, v.Item2);
                                    break;
                                case "lit_Without_Weight":
                                    AssetManager.AddGlobalValueToList<LevelObject, WeightedItemObject>(Convert.ToString(args[2]), wio, v.Item2);
                                    break;
                            }
                        }
                    },
                    inp);
            }
        }
        public class ItemConfig : SpawnConfig
        {
            public bool saveable = true;
            public bool spawnInStore = true;
            public bool spawnInLevel = true;
            public bool addItemsInLevel = true;
            public bool forceItemsInLevel = true;
            public bool potentialItemsInLevel = true;
            public bool shopItemsInLevel = true;
        }
        public static void AddNPC(NPC nPC, NPCConfig cfg = default)
        {
            WeightedNPC win = new WeightedNPC() { selection = nPC, weight = cfg.weight };
            (List<SceneObject>, string[]) v = GetAllSceneObjectWithoutExcludeWithLevelObjectNamesInExclude(cfg);
            object[][] inp = new object[][]
            {
             new object[]{cfg.potentialNPCs,cfg.forcedNpcs},
             new object[]{"lit_With_Weight","ary"},
             new object[]{"potentialNPCs","forcedNpcs"},
            };
            Collections.RepeatOperations(
                (object[] args) =>
                {
                    if ((bool)args[0])
                    {
                        switch ((string)args[1])
                        {
                            case "lit_With_Weight":
                                AssetManager.AddGlobalValueToList<LevelObject, WeightedNPC>((string)args[2], win, v.Item2);
                                break;
                            case "ary":
                                AssetManager.AddGlobalValueToArray<LevelObject, NPC>((string)args[2], nPC, v.Item2);
                                break;
                        }
                    }
                }
                , inp);
        }
        public class NPCConfig : SpawnConfig
        {
            public bool potentialNPCs = true;
            public bool forcedNpcs = true;
        }
    }
}