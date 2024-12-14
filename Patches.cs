using HarmonyLib;
namespace BALDI_FULL_INTERFACE.Patches
{
    [HarmonyPatch]
    public class Patch
    {
        [HarmonyPatch(typeof(OptionsMenu), "Awake"), HarmonyPrefix]
        public static bool Prefix(OptionsMenu __instance)
        {
            OptionsManager.Initialize(__instance);
            return true;
        }
    }
}