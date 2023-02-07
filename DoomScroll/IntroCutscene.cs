using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;
using Microsoft.Extensions.Logging;
using Il2CppSystem;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(IntroCutscene))]
    class IntroCutscenePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("BeginCrewmate")]
        public static void PrefixBeginCrewmate(IntroCutscene __instance)
        {
            SecondaryWinCondition.InitSecondaryWinCondition();
            DoomScroll._log.LogInfo("SecondaryWinCondition initialized: " + SecondaryWinCondition.ToString());
        }

        // displays SWC if local player is crewmate
        [HarmonyPostfix]
        [HarmonyPatch("BeginCrewmate")]
        public static void PostfixBeginCrewmate(IntroCutscene __instance)
        {
            __instance.TeamTitle.text += "\n<size=20%><color=\"white\">Secondary Win Condition: " + SecondaryWinCondition.ToString() + "</color></size>";
            DoomScroll._log.LogInfo("SecondaryWinCondition showing under role assignment: " + SecondaryWinCondition.ToString());
        }

        // replaces SWC with empty SWC if local player is impostor (impostors can't have SWCs)
        [HarmonyPrefix]
        [HarmonyPatch("BeginImpostor")]
        public static void PrefixBeginImpostor(IntroCutscene __instance)
        {
            SecondaryWinCondition.InitSecondaryWinCondition();
            DoomScroll._log.LogInfo("SecondaryWinCondition initialized: " + SecondaryWinCondition.ToString());
            GameData.PlayerInfo localPlayer;
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                if (player.PlayerId == PlayerControl.LocalPlayer._cachedData.PlayerId)
                {
                    localPlayer = player;
                    DoomScroll._log.LogInfo("local player assigned: " + localPlayer + ", playerID: " + player.PlayerId);
                    if (localPlayer.Role.IsImpostor)
                    {
                        SecondaryWinCondition.assignImpostorValues();
                        DoomScroll._log.LogInfo("You're the impostor! SecondaryWinCondition re-initialized: " + SecondaryWinCondition.ToString());
                    }
                    break;
                }
            }
        }

        // displays SWC if local player is impostor
        [HarmonyPostfix]
        [HarmonyPatch("BeginImpostor")]
        public static void PostfixBeginImpostor(IntroCutscene __instance)
        {
            __instance.TeamTitle.text += "\n<size=20%><color=\"white\">Secondary Win Condition: " + SecondaryWinCondition.ToString() + "</color></size>";
            DoomScroll._log.LogInfo("SecondaryWinCondition showing under role assignment: " + SecondaryWinCondition.ToString());
        }

        // displays SWC on second role text screen
        // **NEED TO CHECK if this works!!**
        [HarmonyPrefix]
        [HarmonyPatch("ShowRole")]
        public static void PrefixShowRole(IntroCutscene __instance)
        {
            __instance.RoleText.text += "\n<size=10%><color=\"white\">Secondary Win Condition: " + SecondaryWinCondition.ToString() + "</color></size>";
            DoomScroll._log.LogInfo("SecondaryWinCondition showing under second role screen: " + SecondaryWinCondition.ToString());
        }
    }
}
