﻿using System;
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
            SecondaryWinCondition.m_overallSWCText = new CustomText("SWCResults", __instance.XpBar.gameObject, "PLACEHOLDER" /*SecondaryWinCondition.overallSWCResultsText()*/);
            SecondaryWinCondition.m_overallSWCText.SetColor(Color.white);
            SecondaryWinCondition.m_overallSWCText.SetSize(1.5f);
            Vector3 vec = new Vector3(SecondaryWinCondition.m_overallSWCText.TextObject.transform.localPosition.x, SecondaryWinCondition.m_overallSWCText.TextObject.transform.localPosition.y + 30, SecondaryWinCondition.m_overallSWCText.TextObject.transform.localPosition.z);
            SecondaryWinCondition.m_overallSWCText.SetlocalPosition(vec);
            SecondaryWinCondition.gameOver();
        }
    }
}
