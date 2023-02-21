using System;
using System.Collections.Generic;
//using Reactor;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;
using InnerNet;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(InnerNetClient))]
    class InnerNetClientPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("StartEndGame")]
        public static void PrefixStartEndGame(InnerNetClient __instance)
        {
            SecondaryWinCondition.Evaluate();
            DoomScroll._log.LogInfo("Sending PLACEHOLDER RPC!");
            RPCManager.RPCSendSWCSuccessText("PLACEHOLDER");
        }
    }

    [HarmonyPatch(typeof(EndGameManager))]
    class EndGameManagerPatch
    {
        
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
