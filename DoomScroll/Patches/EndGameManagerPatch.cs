using HarmonyLib;
using Il2CppSystem;
using UnityEngine;

namespace Doom_Scroll.Patches
{
    public enum CustomGameOverReason : byte
    {
        SWCNotCompleted
    }

    [HarmonyPatch(typeof(EndGameManager))]
    class EndGameManagerPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch("SetEverythingUp")]
        public static void PostfixSetEverythingUp(EndGameManager __instance)
        {
            if (__instance.WinText.text == DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Victory) && !SecondaryWinConditionManager.LocalPLayerSWC.CheckSuccess()) //If won but SWC not successful
            {
                __instance.WinText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Defeat);
                __instance.WinText.color = Color.red;
                __instance.WinText.text += "\n<size=20%><color=\"white\"> { SWC Results } <color=\"red\">" + SecondaryWinConditionManager.LocalPLayerSWC.SWCResultsText() + "</color></color></size>";
            }
            else if (!SecondaryWinConditionManager.LocalPLayerSWC.CheckSuccess())
            {
                __instance.WinText.text += "\n<size=20%><color=\"white\"> { SWC Results } <color=\"red\">" + SecondaryWinConditionManager.LocalPLayerSWC.SWCResultsText() + "</color></color></size>";
            }
            else
            {
                __instance.WinText.text += "\n<size=20%><color=\"white\"> { SWC Results } <color=\"blue\">" + SecondaryWinConditionManager.LocalPLayerSWC.SWCResultsText() + "</color></color></size>";
            }

        }
    }
}
