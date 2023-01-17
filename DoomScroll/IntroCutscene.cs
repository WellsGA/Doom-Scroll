﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;
using Microsoft.Extensions.Logging;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(IntroCutscene))]
    class IntroCutscenePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("BeginCrewmate")]
        public static void PrefixBeginCrewmate(IntroCutscene __instance)
        {
            SecondaryWinConditionHolder.assignPlayerID(PlayerControl.LocalPlayer._cachedData.PlayerId);
            //#DoomScroll._log.LogInfo("Select Roles Patch is running!!\n There should be Secondary Win Conditions below:\n");
            SecondaryWinConditionHolder.clearPlayerSWCList(); // ensures list is empty before game starts
            GameData.PlayerInfo localPlayer;
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                if (player.PlayerId == PlayerControl.LocalPlayer._cachedData.PlayerId)
                {
                    localPlayer = player;
                    if (localPlayer.Role.IsImpostor)
                    {
                        PlayerSWCTracker secWinCondImpostor = new PlayerSWCTracker(player.PlayerId);
                        secWinCondImpostor.impostorSWC();
                        SecondaryWinConditionHolder.assignLocalPlayerSWCTracker(secWinCondImpostor);
                        //#Logger<DoomScrollPlugin>.Info("PID: " + secWinCondImpostor.getPlayerID() + ", Player Name: " + IntroCutscenePatch.getPlayerNameFromID(secWinCondImpostor.getPlayerID()) + ", SWC: " + secWinCondImpostor.getSWC().SWCAssignText());
                    }
                    else if (!localPlayer.Role.IsImpostor)
                    {
                        PlayerSWCTracker secWinCondCrewmate = new PlayerSWCTracker(player.PlayerId);
                        SecondaryWinConditionHolder.assignLocalPlayerSWCTracker(secWinCondCrewmate);
                        //#Logger<DoomScrollPlugin>.Info("PID: " + secWinCondCrewmate.getPlayerID() + ", Player Name: " + IntroCutscenePatch.getPlayerNameFromID(secWinCondCrewmate.getPlayerID()) + ", SWC: " + secWinCondCrewmate.getSWC().SWCAssignText());
                    }
                    break;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("BeginCrewmate")]
        public static void PostfixBeginCrewmate(IntroCutscene __instance)
        {
            __instance.TeamTitle.text += "\n<size=20%><color=\"white\">Secondary Win Condition: " + SecondaryWinConditionHolder.getThisPlayerSWC().SWCAssignText() + "</color></size>";
            //will replace "TESTCONDITION" with SecondaryWinConditionHolder.getSomePlayerSWC(PlayerControl.LocalPlayer._cachedData.PlayerId).SWCAssignText();
        }

        [HarmonyPrefix]
        [HarmonyPatch("BeginImpostor")]
        public static void PrefixBeginImpostor(IntroCutscene __instance)
        {
            SecondaryWinConditionHolder.assignPlayerID(PlayerControl.LocalPlayer._cachedData.PlayerId);
            //#DoomScroll._log.LogInfo("Select Roles Patch is running!!\n There should be Secondary Win Conditions below:\n");
            SecondaryWinConditionHolder.clearPlayerSWCList(); // ensures list is empty before game starts
            GameData.PlayerInfo localPlayer;
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                if (player.PlayerId == PlayerControl.LocalPlayer._cachedData.PlayerId)
                {
                    localPlayer = player;
                    if (localPlayer.Role.IsImpostor)
                    {
                        PlayerSWCTracker secWinCondImpostor = new PlayerSWCTracker(player.PlayerId);
                        secWinCondImpostor.impostorSWC();
                        SecondaryWinConditionHolder.assignLocalPlayerSWCTracker(secWinCondImpostor);
                        //#Logger<DoomScrollPlugin>.Info("PID: " + secWinCondImpostor.getPlayerID() + ", Player Name: " + IntroCutscenePatch.getPlayerNameFromID(secWinCondImpostor.getPlayerID()) + ", SWC: " + secWinCondImpostor.getSWC().SWCAssignText());
                    }
                    else if (!localPlayer.Role.IsImpostor)
                    {
                        PlayerSWCTracker secWinCondCrewmate = new PlayerSWCTracker(player.PlayerId);
                        SecondaryWinConditionHolder.assignLocalPlayerSWCTracker(secWinCondCrewmate);
                        //#Logger<DoomScrollPlugin>.Info("PID: " + secWinCondCrewmate.getPlayerID() + ", Player Name: " + IntroCutscenePatch.getPlayerNameFromID(secWinCondCrewmate.getPlayerID()) + ", SWC: " + secWinCondCrewmate.getSWC().SWCAssignText());
                    }
                    break;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("BeginImpostor")]
        public static void PostfixBeginImpostor(IntroCutscene __instance)
        {
            __instance.TeamTitle.text += "\n<size=20%><color=\"white\">Secondary Win Condition: " + SecondaryWinConditionHolder.getThisPlayerSWC().SWCAssignText() + "</color></size>";
            //will replace "TESTCONDITION" with SecondaryWinConditionHolder.getSomePlayerSWC(PlayerControl.LocalPlayer._cachedData.PlayerId).SWCAssignText();
        }

        [HarmonyPrefix]
        [HarmonyPatch("ShowRole")]
        public static void PrefixShowRole(IntroCutscene __instance)
        {
            __instance.RoleText.text += "\n<size=10%><color=\"white\">Secondary Win Condition: " + SecondaryWinConditionHolder.getThisPlayerSWC().SWCAssignText() + "</color></size>";
            //will replace "TESTCONDITION" with SecondaryWinConditionHolder.getSomePlayerSWC(PlayerControl.LocalPlayer._cachedData.PlayerId).SWCAssignText();
        }


        public static string getPlayerNameFromID(byte playID)
        {
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (playerInfo.PlayerId == playID)
                {
                    return playerInfo.PlayerName;
                }
            }
            return "";
        }
    }
}
