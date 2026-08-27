using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using MEC;
using MidiPlayerTK;
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
            ResourcesManager.Get<StickerDataObject>().ToList().ForEach(a => a.LoadInstanced());
            return true;
        }
        [HarmonyPatch(typeof(MidiFilePlayer), "MPTK_Play", new Type[] { }), HarmonyPostfix]
        public static void Postfix(MidiFilePlayer __instance)
        {
            try
            {
                if (Resources.Load<TextAsset>(Path.Combine("MidiDB", __instance.MPTK_MidiName)) != null)
                {
                    return;
                }
                var bytes = ResourcesManager.Get<Midi>(__instance.MPTK_MidiName).data;
                if (__instance.MPTK_CorePlayer)
                {
                    Routine.RunCoroutine(__instance.ThreadCorePlay(bytes).CancelWith(__instance.gameObject), Segment.RealtimeUpdate);
                }
                else
                {
                    Routine.RunCoroutine(__instance.ThreadLegacyPlay(bytes).CancelWith(__instance.gameObject), Segment.RealtimeUpdate);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Midi: {__instance.MPTK_MidiName} custom player failed! Exception: " + e);
            }
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
            foreach (var caption0 in ResourcesManager.Get<SubtitleObject>().ToArray())
            {
                SubtitleObject caption = caption0;
                if (caption == null || caption.localization == null || caption.localization.items == null || caption.localization.items.Length == 0)
                {
                    continue;
                }
                LocalizationData localizationData = caption.localization;
                i = -1;
                for (int a = 0; a < langNames.Count; a++)
                {
                    if (caption.name.Contains(langNames[a]))
                    {
                        i = a;
                        break;
                    }
                }

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

            OptionsManager.Initialize();

            registerEvent?.ForEach(a =>
            {
                try
                {
                    a?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"One of actions in register has failed! " + e);
                }
            });
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