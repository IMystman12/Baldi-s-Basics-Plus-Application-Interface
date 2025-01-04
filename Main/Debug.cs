using BALDI_FULL_INTERFACE.Options.Editor;
using BALDI_FULL_INTERFACE.Options;
using UnityEngine;
using System.Collections;
using BALDI_FULL_INTERFACE.Patches;

namespace BALDI_FULL_INTERFACE.DE_DE_DEBUG
{
    public static class DEBUG
    {
        public static IEnumerator Start(MonoBehaviour behaviour)
        {
            Category category = OptionsManager.CreateCategory("DEBUG");
            category.AddOption(new SliderOption(), "TST_Slider");
            category.AddOption(new DropDownOption(), "TST_DropDown");
            category.AddOption(new InputFieldOption(), "TST_InputField");

            Category category0 = category.CreateCategory("SUB_DEBUG");
            category0.AddOption(new SliderOption(), "TST_Slider");
            category0.AddOption(new DropDownOption(), "TST_DropDown");
            category0.AddOption(new InputFieldOption(), "TST_InputField");
            yield return new WaitForMainMenu();
            behaviour.StartCoroutine(AssetInjector.AddRoom(UnityInterface.AssetManager.GetGameAssetsFromType<LevelAsset>()[0].ConvertToRoomAsset(0)));
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