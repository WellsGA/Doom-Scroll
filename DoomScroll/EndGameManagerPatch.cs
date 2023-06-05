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
            if (SecondaryWinConditionManager.LocalPLayerSWC.ToString() != "No secondary win condition")
            {
                __instance.WinText.text += "\n<size=20%><color=\"white\"> { SWC Results } " + SecondaryWinConditionManager.LocalPLayerSWC.SWCResultsText() + "</color></size>";
                // once RPC verified working, "PLACEHOLDER" will be SecondaryWinCondition.sendableResultsText()
                //DoomScroll._log.LogInfo("After sending RPC, playerSWClist = " + SecondaryWinConditionManager.overallSWCResultsText());
            }
        }
    }
}
