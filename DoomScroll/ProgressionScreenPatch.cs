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
            DoomScroll._log.LogInfo("On Progression Screen, playerSWClist = " + SecondaryWinConditionManager.overallSWCResultsText());
            CustomText overallSWCText = new CustomText(__instance.XpBar.gameObject, "SWCResults", SecondaryWinConditionManager.overallSWCResultsText());
            overallSWCText.SetColor(Color.white);
            overallSWCText.SetSize(5f);
            Vector3 vec = new Vector3(overallSWCText.UIGameObject.transform.localPosition.x, overallSWCText.UIGameObject.transform.localPosition.y - 1, overallSWCText.UIGameObject.transform.localPosition.z);
            overallSWCText.SetLocalPosition(vec);
            SecondaryWinConditionManager.gameOver();
        }
    }
}
