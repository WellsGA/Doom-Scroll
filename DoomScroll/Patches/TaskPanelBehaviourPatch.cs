﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;
using Microsoft.Extensions.Logging;
using Il2CppSystem;
using Il2CppSystem.Text;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(TaskPanelBehaviour))]
    class TaskPanelBehaviorPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SetTaskText")]
        public static void PrefixSetTaskText(ref string str)
        {
            // why did we check for game running?
            if (SecondaryWinConditionManager.LocalPLayerSWC != null)
            {
                str = str + "<color=\"orange\">\nSWC: " + SecondaryWinConditionManager.LocalPLayerSWC.ToString() + "</color>";
                //DoomScroll._log.LogInfo("SWC ToString added to task list: " + SecondaryWinCondition.ToString());
            }
        }
    }
}