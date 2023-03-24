﻿using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(Minigame))]
    public static class MiniGamePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Begin")]
        public static void PostfixBegin(Minigame __instance, PlayerTask task) 
        {
            // no prefab or not a player task or not among the assignable tasks
            /*if(__instance == null || __instance.TaskType == TaskTypes.None 
                || !TaskAssigner.Instance.AssignableTasksIDs.Contains(task.Id)) return;*/
            if (__instance == null || __instance.TaskType == TaskTypes.None || TaskAssigner.Instance.AssignableTasks == null) return; // all tasks will be assignable for testing purposes
            DoomScroll._log.LogInfo("Beging called");
            foreach (uint pt in TaskAssigner.Instance.AssignableTasks)
            {
                DoomScroll._log.LogInfo("task id: " + task.Id + "pt id" + pt);
                if (pt == task.Id)
                {
                    DoomScroll._log.LogInfo("Task assignable");
                    TaskAssigner.Instance.ActivatePanel(task.Id, true);
                    break;
                }
            }
        }

    }
}
