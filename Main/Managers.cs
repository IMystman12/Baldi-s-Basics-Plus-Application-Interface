using System;
using System.Collections;
using System.Collections.Generic;
using BALDI_FULL_INTERFACE.Patches;
using UnityEngine;
using UnityInterface;
namespace BALDI_FULL_INTERFACE
{
    namespace Options
    {
        namespace Editor
        {
            public class InputFieldOption : Option
            {
            }
        }
        public class Category : Option
        {
            public Category parent;
            public string key;
            private Dictionary<string, Category> subCategories = new Dictionary<string, Category>();
            //tempories
            private Dictionary<string, Option> options = new Dictionary<string, Option>();
            public void InitializeWithMenu(OptionsMenu menu)
            {
                foreach (var item in subCategories.Values)
                {
                    item.InitializeWithMenu(menu);
                }
                if (parent == null)
                {
                    instance = OptionsManager.newCategory;
                    menu.AddToArray("categories", instance);
                    menu.AddToArray("categoryKeys", key);
                    instance.transform.name = key;
                }
                else if (false)
                {
                    instance = parent.instance.transform.GetChild(4).gameObject;
                    UnityEngine.Object.Instantiate(instance, instance.transform.parent);
                    instance.SetActive(true);
                    instance.name = key;
                }
                float h = height;
                float hpre = height;
                foreach (var item in options.Values)
                {
                    hpre = h - item.height;
                    if (hpre > 0)
                    {
                        h -= item.height;
                    }
                    if (item.GetType() == typeof(SliderOption))
                    {

                    }
                }
            }
            public Category CreateCategory(string categoryName)
            {
                subCategories.Add(categoryName, new Category() { parent = this });
                return subCategories[categoryName];
            }
            public Category GetCategory(string categoryName)
            {
                if (!subCategories.ContainsKey(categoryName))
                {
                    Debug.Log("Category: " + categoryName + " wasn't found!");
                    return default;
                }
                return subCategories[categoryName];
            }
            public Option AddOption(Option option, string name = "")
            {
                if (name != string.Empty)
                {
                    option.name = name;
                }
                options.Add(name, option);
                return options[name];
            }
            public object GetOption(string name)
            {
                if (!options.ContainsKey(name))
                {
                    Debug.Log("Option: " + name + " wasn't found!");
                    return default;
                }
                return options[name];
            }
        }
        public class Option
        {
            public string name;
            public int height;
            public GameObject instance;
            public object value;
        }
        public class DropDownOption : Option
        {
            public string[] selections = new string[0];
            public string Selection => selections[(int)value];
        }
        public class SliderOption : Option
        {
            public bool useInt;
            public int barCount;
            public float min = 0;
            public float max = 5;
        }
        [Obsolete("It's still in development,please!")]
        public static class OptionsManager
        {
            public static Dictionary<string, Category> categories = new Dictionary<string, Category>();
            public static OnSthOutputInSingle onInitialize;
            private static GameObject _category;
            public static GameObject newCategory
            {
                get
                {
                    GameObject v = UnityEngine.Object.Instantiate(_category.gameObject, _category.transform.parent);
                    v.transform.SetSiblingIndex(11);
                    return v;
                }
            }
            private static void DeactiveChild(GameObject g)
            {
                for (int i = 0; i < g.transform.childCount; i++)
                {
                    g.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            public static void Initialize(OptionsMenu menu)
            {
                GameObject v = menu.GetValue<GameObject[]>("categories")[0];
                _category = UnityEngine.Object.Instantiate(v.gameObject, v.transform.parent);
                _category.SetActive(false);
                _category.transform.SetSiblingIndex(11);
                _category.name = "Category_Prefab";
                DeactiveChild(_category);
                foreach (var item in categories.Values)
                {
                    item.InitializeWithMenu(menu);
                }
                onInitialize?.Invoke(menu);
            }
            public static Category CreateCategory(string categoryKey)
            {
                categories.Add(categoryKey, new Category() { key = categoryKey });
                return categories[categoryKey];
            }
            public static Category GetCategory(string categoryKey)
            {
                if (!categories.ContainsKey(categoryKey))
                {
                    Debug.Log("Category: " + categoryKey + " wasn't found!");
                    return default;
                }
                return categories[categoryKey];
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
        protected static void ForeachScenes(OnSthOutputInSingle<SceneObject> o, SpawnConfig cfg)
        {
            foreach (var scene in GetAllSceneObjectWithoutExclude(cfg))
            {
                o?.Invoke(scene);
            }
        }
        public static List<SceneObject> GetAllSceneObjectWithoutExclude(SpawnConfig cfg)
        {
            List<SceneObject> sol = new List<SceneObject>();
            foreach (var item in AssetManager.GetGameAssetsFromType<SceneObject>())
            {
                if (!cfg.excludeLevelTitles.Contains(item.levelTitle) && item.levelObject)
                {
                    sol.Add(item);
                }
            }
            return sol;
        }
        public static void AddItem(ItemObject item, ItemConfig cfg = default)
        {
            if (cfg == null)
            {
                cfg = new ItemConfig();
            }
            if (cfg.saveable)
            {
                PlayerFileManager.Instance.itemObjects.Add(item);
            }
            WeightedItemObject wio = new WeightedItemObject() { selection = item, weight = cfg.weight };
            OnSthOutputInSingle<SceneObject> o = null;
            if (cfg.spawnInShop)
            {
                o += (SceneObject scene) =>
                   {
                       scene.shopItems.Add(wio);
                   };
            }
            if (cfg.spawnInLevel)
            {
                if (cfg.add)
                {
                    o += (SceneObject scene) =>
                    {
                        scene.levelObject.items.Add(wio);
                    };
                }
                if (cfg.forcedSpawn)
                {
                    o += (SceneObject scene) =>
                    {
                        scene.levelObject.forcedItems.Add(item);
                    };
                }
                if (cfg.isPotential)
                {
                    o += (SceneObject scene) =>
                    {
                        scene.levelObject.potentialItems.Add(wio);
                    };
                }
                if (cfg.spawnInShop)
                {
                    o += (SceneObject scene) =>
                    {
                        scene.levelObject.shopItems.Add(wio);
                    };
                }
                ForeachScenes(o, cfg);
            }
        }
        public class ItemConfig : SpawnConfig
        {
            public bool saveable = true;
            public bool spawnInLevel = true;
            public bool add = true;
            public bool forcedSpawn = true;
            public bool isPotential = true;
            public bool spawnInShop = true;
        }
        public static void AddNPC(NPC nPC, NPCConfig cfg = default)
        {
            if (cfg == null)
            {
                cfg = new NPCConfig();
            }
            WeightedNPC win = new WeightedNPC() { selection = nPC, weight = cfg.weight };
            OnSthOutputInSingle<SceneObject> o = null;
            if (cfg.isPotential)
            {
                o += (SceneObject scene) =>
                {
                    scene.levelObject.potentialNPCs.Add(win);
                };
            }
            if (cfg.isPotential)
            {
                o += (SceneObject scene) =>
                {
                    scene.levelObject.forcedNpcs.Add(nPC);
                };
            }
            ForeachScenes(o, cfg);
        }
        public class NPCConfig : SpawnConfig
        {
            public bool isPotential = true;
            public bool forcedSpawn = true;
        }
        [Obsolete("It just a template,with Obsolete!", true)]
        public static void TemplateAdd(LevelBuilder nPC, TemplateConfig cfg = default)
        {
        }
        [Obsolete("It just a template,with Obsolete!", true)]
        public class TemplateConfig : SpawnConfig
        {
        }
        public static void AddBuilder(ObjectBuilder builder, BuilderConfig cfg = default)
        {
            if (cfg == null)
            {
                cfg = new BuilderConfig();
            }
            WeightedObjectBuilder w = new WeightedObjectBuilder() { selection = builder, weight = cfg.weight };
            OnSthOutputInSingle<SceneObject> o = null;
            if (cfg.forcedSpecialHall)
            {
                o += (SceneObject scene) =>
                {
                    scene.levelObject.forcedSpecialHallBuilders.Add(builder);
                };
            }
            if (cfg.specialHall)
            {
                o += (SceneObject scene) =>
                {
                    scene.levelObject.specialHallBuilders.Add(w);
                };
            }
            ForeachScenes(o, cfg);
        }
        public class BuilderConfig : SpawnConfig
        {
            public bool specialHall;
            public bool forcedSpecialHall;
            public static BuilderConfig AllOn => new BuilderConfig() { forcedSpecialHall = true, specialHall = true };
        }
        public static void AddRandomEvent(RandomEvent re, RandomEventConfig cfg = default)
        {
            if (cfg == null)
            {
                cfg = new RandomEventConfig();
            }
            WeightedRandomEvent w = new WeightedRandomEvent() { selection = re, weight = cfg.weight };
            OnSthOutputInSingle<SceneObject> o = (SceneObject scene) =>
                {
                    scene.levelObject.randomEvents.Add(w);
                };
            ForeachScenes(o, cfg);
        }
        public class RandomEventConfig : SpawnConfig
        {
        }
        public static void AddRoom(RoomAsset roomAsset, RoomAssetConfig cfg = default)
        {
            if (cfg == null)
            {
                cfg = new RoomAssetConfig();
            }
            WeightedRoomAsset w = new WeightedRoomAsset() { selection = roomAsset, weight = cfg.weight };
            OnSthOutputInSingle<SceneObject> o = null;
            o += (SceneObject scene) =>
            {
                foreach (var item in scene.levelObject.roomGroup)
                {
                    foreach (var item1 in cfg.categories)
                    {
                        if (item.name.ToLower() == item1.ToString().ToLower())
                        {
                            item.potentialRooms.Add(w);
                        }
                    }
                }
            };
            if (cfg.categories.Contains(RoomCategory.Hall))
            {
                o += (SceneObject scene) =>
                {
                    scene.levelObject.potentialPrePlotSpecialHalls.Add(w);
                    scene.levelObject.potentialPostPlotSpecialHalls.Add(w);
                };
            }
            if (cfg.categories.Contains(RoomCategory.Special))
            {
                o += (SceneObject scene) =>
                {
                    scene.levelObject.potentialSpecialRooms.Add(w);
                };
            }
            ForeachScenes(o, cfg);
        }
        public class RoomAssetConfig : SpawnConfig
        {
            public List<RoomCategory> categories = new List<RoomCategory>();
        }
        public static void AddRoomGroup(RoomGroup group, RoomGroupConfig cfg)
        {
            if (cfg == null)
            {
                cfg = new RoomGroupConfig();
            }
            OnSthOutputInSingle<SceneObject> o = (SceneObject scene) =>
            {
                scene.levelObject.roomGroup.Add(group);
            };
            ForeachScenes(o, cfg);
        }
        public class RoomGroupConfig : SpawnConfig
        {
        }
        public static void ReplaceRoomGroup(RoomGroup group, RoomGroupConfig cfg)
        {
            if (cfg == null)
            {
                cfg = new RoomGroupConfig();
            }
            OnSthOutputInSingle<SceneObject> o = null;
            o += (SceneObject scene) =>
            {
                foreach (var item in scene.levelObject.roomGroup)
                {
                    if (item.name == group.name)
                    {
                        group.Merge(item);
                    }
                }
            };
            ForeachScenes(o, cfg);
        }
        public static void AddRoomTexture(Texture2D texture, RoomTextureConfig cfg)
        {
            if (cfg == null)
            {
                cfg = new RoomTextureConfig();
            }
            WeightedTexture2D wt = new WeightedTexture2D() { selection = texture, weight = cfg.weight };
            OnSthOutputInSingle<SceneObject> o = null;
            o += (SceneObject scene) =>
            {
                foreach (var item in scene.levelObject.roomGroup)
                {
                    if (item.name == cfg.name)
                    {
                        if (cfg.ceiling)
                        {
                            item.ceilingTexture.Add(wt);
                        }
                        if (cfg.wall)
                        {
                            item.wallTexture.Add(wt);
                        }
                        if (cfg.floor)
                        {
                            item.floorTexture.Add(wt);
                        }
                    }
                }
            };
            ForeachScenes(o, cfg);
        }
        public class RoomTextureConfig : SpawnConfig
        {
            public string name;
            public bool ceiling;
            public bool wall;
            public bool floor;
        }
        public static IEnumerator AddPoster(PosterObject poster, PosterConfig cfg = default)
        {
            if (cfg == null)
            {
                cfg = new RoomGroupConfig();
            }
            OnSthOutputInSingle<SceneObject> o = null;
            o += (SceneObject scene) =>
            {
                foreach (var item in scene.levelObject.roomGroup)
                {
                    if (item.name == group.name)
                    {
                        group.Merge(item);
                    }
                }
            };
            yield return new WaitForMainMenu();
            ForeachScenes(o, cfg);
        }
        public class PosterConfig : SpawnConfig
        {
        }
    }
    public static class AssetConverter
    {
        public static RoomAsset ConvertToRoomAsset(this LevelAsset asset, int index)
        {
            RoomAsset r = ScriptableObject.CreateInstance<RoomAsset>();
            asset.rooms[index].ConvertToAsset(r, new IntVector2());
            return r;
        }
    }
    [AttributeUsage(AttributeTargets.All)]
    public class zg : Attribute
    {
        public zg(float h, int k, string ghi)
        {

        }
    }
}