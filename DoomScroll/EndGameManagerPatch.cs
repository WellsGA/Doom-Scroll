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
            RPCManager.RPCSendSWCSuccessText(SecondaryWinConditionHolder.getThisPlayerTracker().sendableResultsText());
            //will replace "TESTCONDITION" with SecondaryWinConditionHolder.getSomePlayerSWC(PlayerControl.LocalPlayer._cachedData.PlayerId).SWCAssignText();
            //will replace "TESTRESULT" with SecondaryWinConditionHolder.getSomePlayerSWC(PlayerControl.LocalPlayer._cachedData.PlayerId).SWCResultsText();
            DoomScroll._log.LogInfo(SecondaryWinConditionHolder.overallSWCResultsText());
        }
    }
}
