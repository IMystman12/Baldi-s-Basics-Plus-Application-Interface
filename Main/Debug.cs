
using UnityEngine;
using System.Collections;
using BALDI_FULL_INTERFACE.Patches;

namespace BALDI_FULL_INTERFACE.DEBUG
{
    public static class DEBUG
    {
        public static IEnumerator Start(MonoBehaviour behaviour)
        {
            yield return new WaitForMainMenu();
        }
        public class TST_NPC : NPC
        {
        }
        public class TST_Builder : ObjectBuilder
        {
        }
        public class TST_RandomEvent : RandomEvent
        {
        }
    }
}