
using BepInEx;
using HarmonyLib;
namespace BALDI_FULL_INTERFACE
{
    [BepInPlugin("imystman12.baldifull.interface", "BB+ Application Interface", "1.0")]
    public class Invoker : BaseUnityPlugin
    {
        public static Invoker Instance => instance;
        private static Invoker instance;
        void Awake()
        {
            instance = this;
            new Harmony("imystman12.baldifull.interface").PatchAll();
            StartCoroutine(DEBUG.DEBUG.Start(this));
        }
    }
}