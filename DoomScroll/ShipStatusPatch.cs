using HarmonyLib;
using System.Collections.Generic;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(ShipStatus))]
    class ShipStatusPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(ShipStatus __instance)
        {
            ScreenshotManager.Instance.ActivateCameraButton(true);
            NewsFeedManager.Instance.ActivateNewsButton(true);
            NewsFeedManager.Instance.CanPostNews(false); // by edfault player cannot create news

            DoomScroll._log.LogInfo("ShipStatusPatch.Start ---- CAMERA AND NEWS INIT");

            // list all tasks - for debug purposes
            DoomScroll._log.LogInfo("COMMON TASKS");
            foreach (NormalPlayerTask task in __instance.CommonTasks)
            {
                DoomScroll._log.LogInfo("Name: " + task.name + ", Index: " + task.Index +
                ", type: " + task.TaskType + ", task steps: " + task.MaxStep);
            }
            DoomScroll._log.LogInfo("LONG TASKS");
            foreach (NormalPlayerTask task in __instance.LongTasks)
            {
                DoomScroll._log.LogInfo("Name: " + task.name + ", Index: " + task.Index +
                ", type: " + task.TaskType + ", task steps: " + task.MaxStep);
            }
            DoomScroll._log.LogInfo("NORMAL TASKS");
            foreach (NormalPlayerTask task in __instance.NormalTasks)
            {
                DoomScroll._log.LogInfo("Name: " + task.name + ", Index: " + task.Index +
                ", type: " + task.TaskType + ", task steps: " + task.MaxStep);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Begin")] // only called when host
        public static void PostfixBegin()
        {
            SecondaryWinConditionManager.SetSecondaryWinConditions();
        }
    }
}
