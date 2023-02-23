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
    class GameManagerPatch
    {

        //Alaina versions:

        /*
            //This is where the host sends the RPC
        [HarmonyPrefix]
        [HarmonyPatch("RpcEndGame")]
        public static void PrefixRpcEndGame(GameManager __instance)
        {
            SecondaryWinCondition.Evaluate();
            DoomScroll._log.LogInfo("In RPCEndGame, sending PLACEHOLDER RPC!");
            RPCManager.RPCSendSWCSuccessText("PLACEHOLDER");
        }
        
            //This is so all the other players send the RPCs
        [HarmonyPrefix]
        [HarmonyPatch("HandleRPC")]
        public static void PrefixHandleRPC(GameManager __instance)
        {
            SecondaryWinCondition.Evaluate();
            DoomScroll._log.LogInfo("In HandleRPC, sending PLACEHOLDER RPC!");
            RPCManager.RPCSendSWCSuccessText("PLACEHOLDER");
        }
        */
    }

    [HarmonyPatch(typeof(EndGameManager))]
    class EndGameManagerPatch
    {
        
        [HarmonyPostfix]
        [HarmonyPatch("SetEverythingUp")]
        public static void PostfixSetEverythingUp(EndGameManager __instance)
        {
            __instance.WinText.text += "\n<size=20%><color=\"white\"> { SWC Results } " + SecondaryWinConditionManager.LocalPLayerSWC.SWCResultsText() + "</color></size>";
            // once RPC verified working, "PLACEHOLDER" will be SecondaryWinCondition.sendableResultsText()
            DoomScroll._log.LogInfo("After sending RPC, playerSWClist = " + SecondaryWinConditionManager.overallSWCResultsText());
        }
    }
}
