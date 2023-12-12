using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;
using Microsoft.Extensions.Logging;
using Il2CppSystem;
using Il2CppSystem.Text;
using static Il2CppSystem.Net.UnsafeNclNativeMethods.HttpApi;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(TaskPanelBehaviour))]
    class TaskPanelBehaviorPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SetTaskText")]
        public static void PrefixSetTaskText(ref string str)
        {
            if (HeadlineManager.Instance != null)
            {
                if (HeadlineManager.Instance.NewsPostedByLocalPLayer > 0)
                {
                    str += "\n<color=#00DD00FF>";
                    str += "Headline Post button: Post a Headline (1/1)</color>";
                }
                else
                {
                    str += "\n<color=#FFFF00FF>";
                    str += "Headline Post button: Post a Headline (0/1)</color>";
                }
            }

            if (DoomScrollVictoryManager.VotingTaskCompleteAsOfLastMeeting)
            {
                str += "\n<color=#00DD00FF>";
                str += "Crewmate Voting: Everyone vote on headlines correctly</color>";
            }
            else
            {
                str += "\n<color=#FFFF00FF>";
                str += "Crewmate Voting: Everyone vote on headlines correctly</color>";
            }

            if (SecondaryWinConditionManager.LocalPLayerSWC != null)
            {
                str = str + "<color=\"orange\">\nSWC: " + SecondaryWinConditionManager.LocalPLayerSWC.ToString() + "</color>";
                //DoomScroll._log.LogInfo("SWC ToString added to task list: " + SecondaryWinCondition.ToString());
            }
        }
    }
}