﻿using System;
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
        [HarmonyPrefix]
        [HarmonyPatch("SetEverythingUp")]
        public static void PrefixSetEverythingUp(EndGameManager __instance)
        {
            SecondaryWinCondition.Evaluate();
            DoomScroll._log.LogInfo("Sending PLACEHOLDER RPC!");
            RPCManager.RPCSendSWCSuccessText("PLACEHOLDER");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch("SetEverythingUp")]
        public static void PostfixSetEverythingUp(EndGameManager __instance)
        {
            __instance.WinText.text += "\n<size=20%><color=\"white\"> { SWC Results } " + SecondaryWinCondition.SWCResultsText() + "</color></size>";
            // once RPC verified working, "PLACEHOLDER" will be SecondaryWinCondition.sendableResultsText()
            DoomScroll._log.LogInfo("After sending RPC, playerSWClist = " + SecondaryWinCondition.overallSWCResultsText());
        }
    }
}
