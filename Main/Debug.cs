using System.Collections;
using UnityEngine;
using UnityInterface;

namespace BALDI_FULL_INTERFACE.DEBUG
{
    public static class DEBUG
    {
        public static IEnumerator Start()
        {
            for (int i = 0; i < 99; i++)
            {
                OptionsManager.AddCategory($"Opt_Tst_{i}");
            }
            yield return new WaitForBuiltInResourceLoaded();
            foreach (var item in Resources.FindObjectsOfTypeAll<LevelObject>())
            {
                item.forcedNpcs = item.forcedNpcs.AddAs();
            }
        }
        public class TST_NPC : NPC
        {
        }
    }
}