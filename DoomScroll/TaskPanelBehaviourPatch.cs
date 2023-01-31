/*using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;
using Microsoft.Extensions.Logging;
using Il2CppSystem;
using Il2CppSystem.Text;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(TaskPanelBehaviour))]
    class TaskPanelBehaviorPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SetTaskText")]
        public static void PrefixSetTaskText(NormalPlayerTask __instance, ref string str)
        {
            str = str + "\nSWC: " + SecondaryWinConditionHolder.getThisPlayerSWC().SWCAssignText();
        }
    }
}*/