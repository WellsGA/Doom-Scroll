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
            __instance.WinText.text += "\n<size=20%><color=\"white\"> { SWC Results } " + SecondaryWinConditionHolder.getThisPlayerSWC().SWCResultsText() + "</color></size>";
            RPCManager.RPCSendSWCSuccessText("PLACEHOLDER");
            //SecondaryWinConditionHolder.getThisPlayerTracker().sendableResultsText()
            DoomScroll._log.LogInfo("After sending RPC, " + SecondaryWinConditionHolder.overallSWCResultsText());
        }
    }
}
