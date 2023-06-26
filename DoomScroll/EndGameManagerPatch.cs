using HarmonyLib;

namespace Doom_Scroll
{ 

    [HarmonyPatch(typeof(EndGameManager))]
    class EndGameManagerPatch
    {
        
        [HarmonyPostfix]
        [HarmonyPatch("SetEverythingUp")]
        public static void PostfixSetEverythingUp(EndGameManager __instance)
        {
            __instance.WinText.text += "\n<size=20%><color=\"white\"> { SWC Results } " + SecondaryWinConditionManager.LocalPLayerSWC.SWCResultsText() + "</color></size>";
        }
    }
}
