using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using BALDI_FULL_INTERFACE.Options;
using HarmonyLib;
using UnityEngine;
using UnityInterface;
namespace BALDI_FULL_INTERFACE.Patches
{
    public class WaitForMainMenu : CustomYieldInstruction
    {
        public override bool keepWaiting => !Patch.MainMenuLoaded;
    }
    [HarmonyPatch]
    public class Patch
    {
        public static bool MainMenuLoaded;
        [HarmonyPatch(typeof(NameManager), "Awake"), HarmonyPrefix]
        public static bool Prefix()
        {
            MainMenuLoaded = true;
            return true;
        }
        [HarmonyPatch(typeof(OptionsMenu), "Awake"), HarmonyPrefix]
        public static bool Prefix(OptionsMenu __instance)
        {
            OptionsManager.Initialize(__instance);
            return true;
        }
        [HarmonyPatch(typeof(LocalizationManager), "LoadLocalizedText", typeof(string), typeof(Language)), HarmonyPostfix]
        public static void Postfix(LocalizationManager __instance, string fileName, Language language)
        {
            Dictionary<string, string> d = __instance.GetValue<Dictionary<string, string>>("localizedText");
            for (int i = 0; i < UnityManager.Plugins.Count; i++)
            {
                string p = Path.Combine(AssetManager.GetProjectFolder(UnityManager.Plugins[i]), "Subtitles", fileName);
                if (File.Exists(p))
                {
                    LocalizationData localizationData = JsonUtility.FromJson<LocalizationData>(File.ReadAllText(p));
                    for (int j = 0; j < localizationData.items.Length; j++)
                    {
                        d.Add(localizationData.items[j].key, localizationData.items[j].value);
                    }
                }
                else
                {
                    Debug.Log(p + " cannot find file!");
                }
            }
        }
    }
}