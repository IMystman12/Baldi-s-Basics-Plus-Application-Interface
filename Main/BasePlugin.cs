
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BALDI_FULL_INTERFACE;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityInterface;
using static Register;

namespace BALDI_FULL_INTERFACE
{
    [BepInDependency("unity.interface"), BepInPlugin("imystman12.baldifull.interface", "Baldi Full Application Interface", "1.0"), HarmonyPatch]
    public class PluginCore : BaseUnityPlugin
    {
        internal static bool debugMode;
        internal static PluginCore Instance => instance;
        private static PluginCore instance;
        internal static List<ItemObject> itemToStick = new List<ItemObject>();
        internal static Dictionary<Language, Dictionary<string, string>> subtitles = new Dictionary<Language, Dictionary<string, string>>();
        [HarmonyPatch(typeof(ItemManager), "Update"), HarmonyPostfix]
        public static void Postfix(ItemManager __instance)
        {
            if (!debugMode)
            {
                return;
            }
            for (int i = 0; i < Mathf.Min(itemToStick.Count, 6); i++)
            {
                __instance.SetItem(itemToStick[i], i);
                __instance.LockSlot(i, val: true);
            }
        }
        [HarmonyPatch(typeof(MenuInitializer), "Start"), HarmonyPrefix]
        public static void Prefix() => WaitForBuiltInResourceLoaded.done = true;
        [HarmonyPatch(typeof(LocalizationManager), "LoadLocalizedText", typeof(string), typeof(Language)), HarmonyPostfix]
        public static void Postfix(LocalizationManager __instance, string fileName, Language language) => RefreshSubtitles(language);
        [HarmonyPatch(typeof(StickerManager), "AwakeFunction"), HarmonyPrefix]
        public static bool Prefix0()
        {
            Resources.FindObjectsOfTypeAll<StickerLoadingData>().ToList().ForEach(a => a.LoadInstanced());
            return true;
        }

        void Awake()
        {
            instance = this;
            string fontName = this.QuickOption("Defualt option font", "!Unassigned");
            debugMode = this.QuickOption("Debug Mode", false);
            new Harmony("imystman12.baldifull.interface").PatchAll();
        }
        IEnumerator Start()
        {
            yield return new WaitForBuiltInResourceLoaded();

            yield return new WaitForSecondsRealtime(1);

            int i = 0;
            List<string> langNames = Enum.GetNames(typeof(Language)).ToList();
            foreach (var caption0 in Resources.FindObjectsOfTypeAll<SubtitleObject>().ToArray())
            {
                SubtitleObject caption = caption0;
                if (caption == null || caption.localization == null || caption.localization.items == null || caption.localization.items.Length == 0)
                {
                    continue;
                }
                LocalizationData localizationData = caption.localization;
                i = langNames.IndexOf(caption.name);
                if (i > -1)
                {
                    Language l = (Language)i;
                    if (!subtitles.ContainsKey(l))
                    {
                        subtitles.Add(l, new Dictionary<string, string>());
                    }
                    localizationData.items.ToList().ForEach(b =>
                    {
                        if (!subtitles[l].ContainsKey(b.key))
                        {
                            subtitles[l].Add(b.key, b.value);
                        }
                    }
             );
                }
                else
                {
                    Debug.Log("Localization file name must include one of them!");
                    foreach (var item in langNames)
                    {
                        Debug.Log($"{item}");
                    }
                }
            }
            RefreshSubtitles(LocalizationManager.Instance.GetValue<Language>("currentSubLang"));

            try
            {
                Resources.FindObjectsOfTypeAll<AssetLoadingData>().ToList().ForEach(a => a.Load());
            }
            catch (Exception e)
            {
                Debug.LogError("Asset Loading failed! " + e);
            }
            OptionsManager.Initialize();
        }

        internal static void RefreshSubtitles(Language language)
        {
            Dictionary<string, string> d = LocalizationManager.Instance.GetValue<Dictionary<string, string>>("localizedText");
            if (subtitles.ContainsKey(language))
            {
                foreach (var item in subtitles[language])
                {
                    if (!d.ContainsKey(item.Key))
                    {
                        d.Add(item.Key, item.Value);
                    }
                    else
                    {
                        d[item.Key] = item.Value;
                    }
                }
            }
            LocalizationManager.Instance.SetValue("localizedText", d);
        }
    }
}