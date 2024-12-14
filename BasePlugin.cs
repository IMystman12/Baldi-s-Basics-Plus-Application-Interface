using BepInEx;
using HarmonyLib;
namespace BALDI_FULL_INTERFACE
{
    [BepInPlugin("imystman12.baldifull.interface", "BB+ Application Interface", "1.0")]
    public class BasePlugin : Singleton<BaseUnityPlugin>
    {
        void Awake()
        {
            Harmony harmony = new Harmony("imystman12.baldifull.interface");
            harmony.PatchAll();
        }
    }
}