using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;
using Microsoft.Extensions.Logging;
using Il2CppSystem;
using Il2CppSystem.Text;
using Doom_Scroll.UI;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(ProgressionScreen))]
    class ProgressionScreenPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch("Activate")]
        public static void PostfixActivate(ProgressionScreen __instance)
        {
            DoomScroll._log.LogInfo("On Progression Screen, playerSWClist = " + SecondaryWinCondition.overallSWCResultsText());
            SecondaryWinCondition.m_overallSWCText = new CustomText("SWCResults", __instance.XpEarnedNowText.gameObject, "PLACEHOLDER" /*SecondaryWinCondition.overallSWCResultsText()*/);
            SecondaryWinCondition.gameOver();
        }
    }
}
