using Doom_Scroll.Common;
using HarmonyLib;
using System;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(ShipStatus))]
    class ShipStatusPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(ShipStatus __instance)
        {
            ScreenshotManager.Instance.ActivateCameraButton(true);
            HeadlineManager.Instance.ActivateNewsButton(true);
            HeadlineManager.Instance.CanPostNews(false); // by edfault player cannot create news

            DoomScroll._log.LogInfo("ShipStatusPatch.Start ---- CAMERA AND NEWS INIT");

            // list all tasks - for debug purposes
            // PrintAllTasksToConsole(__instance);

        }

        [HarmonyPostfix]
        [HarmonyPatch("Begin")] // only called when host
        public static void PostfixBegin()
        {
            GameLogger.InitFileWriter("GameTracking");
            GameLogger.Write("========================================= \n" + GameLogger.GetTime() + " - New Game Started\n Number of players: " + GameData.Instance.AllPlayers.Count +
                             "\n  PLayer names and roles:\n");
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers) 
            {
                GameLogger.Write("\t" + player.PlayerName + " [" + player.Role.Role + "]");
            }

            SecondaryWinConditionManager.SetSecondaryWinConditions();
            HeadlineManager.Instance.SelectPLayersWhoCanPostNews();
        }

        public static void PrintAllTasksToConsole(ShipStatus instance)
        {
            DoomScroll._log.LogInfo("COMMON TASKS");
            foreach (NormalPlayerTask task in instance.CommonTasks)
            {
                DoomScroll._log.LogInfo("Name: " + task.name + ", Index: " + task.Index +
                ", type: " + task.TaskType + ", task steps: " + task.MaxStep);
            }
            DoomScroll._log.LogInfo("LONG TASKS");
            foreach (NormalPlayerTask task in instance.LongTasks)
            {
                DoomScroll._log.LogInfo("Name: " + task.name + ", Index: " + task.Index +
                ", type: " + task.TaskType + ", task steps: " + task.MaxStep);
            }
            DoomScroll._log.LogInfo("SHORT TASKS");
            foreach (NormalPlayerTask task in instance.ShortTasks)
            {
                DoomScroll._log.LogInfo("Name: " + task.name + ", Index: " + task.Index +
                ", type: " + task.TaskType + ", task steps: " + task.MaxStep);
            }
        }
    }
}
