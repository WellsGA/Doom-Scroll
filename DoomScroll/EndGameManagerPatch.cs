using System;
using System.Collections.Generic;
//using Reactor;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(EndGameManager))]
    class EndGameManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetEverythingUp")]
        public static void PostfixSetEverythingUp(EndGameManager __instance)
        {
            SecondaryWinCondition.Evaluate();
            __instance.WinText.text += "\n<size=20%><color=\"white\"> { SWC Results } " + SecondaryWinCondition.SWCResultsText() + "</color></size>";
            DoomScroll._log.LogInfo("Sending PLACEHOLDER RPC!");
            // once RPC verified working, "PLACEHOLDER" will be SecondaryWinCondition.sendableResultsText()
            RPCManager.RPCSendSWCSuccessText("PLACEHOLDER");
            DoomScroll._log.LogInfo("After sending RPC, playerSWClist = " + SecondaryWinCondition.overallSWCResultsText());
        }
    }
}
