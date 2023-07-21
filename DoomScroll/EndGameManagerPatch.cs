using HarmonyLib;
using Il2CppSystem;
using UnityEngine;

namespace Doom_Scroll
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
            /*if (__instance.WinText.color != Color.red && !SecondaryWinConditionManager.LocalPLayerSWC.CheckSuccess()) //If won but SWC not successful
            {
                StatsManager.Instance.AddLoseReason(TempData.EndReason);
                __instance.WinText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Defeat, (Il2CppSystem.Object[])Array.Empty<object>());
                __instance.WinText.color = Color.red;
            }*/
            __instance.WinText.text += "\n<size=20%><color=\"white\"> { SWC Results } " + SecondaryWinConditionManager.LocalPLayerSWC.SWCResultsText() + "</color></size>";
        }
    }
}
