﻿using HarmonyLib;
using UnityEngine;
using Doom_Scroll.UI;
using Doom_Scroll.Common;
using System.Collections.Generic;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(ProgressionScreen))]
    class ProgressionScreenPatch
    {
        public static bool progressionScreenOpen = false;
        private static float fontSize = 2f;

        [HarmonyPostfix]
        [HarmonyPatch("Activate")]
        public static void PostfixActivate(ProgressionScreen __instance)
        {
            progressionScreenOpen = true;
            DoomScroll._log.LogInfo("On Progression Screen, playerSWClist = " + SecondaryWinConditionManager.OverallSWCResultsText());
            
            string results = "";
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                SecondaryWinCondition swc = SecondaryWinConditionManager.GetSwcByPlayerID(player.PlayerId);
                if (swc != null)
                {
                    results += swc.SendableResultsText();
                    if (NewsFeedManager.Instance.PlayerScores.ContainsKey(player.PlayerId))
                    {
                        results += NewsFeedManager.Instance.PlayerScores[player.PlayerId];
                    }
                }
            }
            // game log
            if (AmongUsClient.Instance.AmHost)
            {
                GameLogger.Write(GameLogger.GetTime() + " - GAME ENDED - RESULTS \n" + results);
            }

            CustomText overallResult = new CustomText(__instance.XpBar.gameObject, "SWCResults", results);
            overallResult.SetColor(Color.white);
            float size = fontSize;
            if (SecondaryWinConditionManager.GetSWCList().Count >= 8)
            {
                size = fontSize / 2f;
            }
            overallResult.SetSize(size);
            Vector3 textPos = new Vector3(overallResult.UIGameObject.transform.localPosition.x, overallResult.UIGameObject.transform.localPosition.y - 0.9f, overallResult.UIGameObject.transform.localPosition.z);
            overallResult.SetLocalPosition(textPos);
            
            SecondaryWinConditionManager.Reset();
        }
    }
}
