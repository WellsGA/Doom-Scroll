using System;
using System.Collections.Generic;
//using Reactor;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;
using InnerNet;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(GameManager))]
    class InnerNetClientPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("RpcEndGame")]
        public static void PrefixRpcEndGame(GameManager __instance)
        {
            foreach (PlayerControl pi in PlayerControl.AllPlayerControls)
            {
                DoomScroll._log.LogInfo("player id: " + pi.name);
            }
            SecondaryWinCondition.Evaluate();  
            RPCManager.RPCSendSWCSuccessText(SecondaryWinCondition.ToString() + ", " + SecondaryWinCondition.SWCResultsText());
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
