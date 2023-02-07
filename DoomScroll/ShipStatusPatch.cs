using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

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
            DoomScroll._log.LogInfo("ShipStatusPatch.Start ---- CAMERA INIT");

            SecondaryWinCondition.initSecondaryWinCondition(PlayerControl.LocalPlayer._cachedData.PlayerId);
            DoomScroll._log.LogInfo("SecondaryWinCondition initialized: " + SecondaryWinCondition.ToString());
            
            // list all tasks - for debug purposes
            DoomScroll._log.LogInfo("TASK INFO");            
            foreach (NormalPlayerTask task in __instance.CommonTasks)
            {
                DoomScroll._log.LogInfo("COMMON name: " + task.name + ", Index: " + task.Index +
                ", type: " + task.TaskType);
            }
            foreach (NormalPlayerTask task in __instance.LongTasks)
            {
                DoomScroll._log.LogInfo("LONG name: " + task.name + ", Index: " + task.Index +
                ", type: " + task.TaskType);
            }
            foreach (NormalPlayerTask task in __instance.NormalTasks)
            {
                DoomScroll._log.LogInfo("NORMAL name: " + task.name + ", Index: " + task.Index +
                ", type: " + task.TaskType);
            }
        }

        // called in AmongUsClient CoStartGameHost() - only shows up on the host's screen
        [HarmonyPostfix]
        [HarmonyPatch("Begin")]
        public static void PostfixBegin(ShipStatus __instance)
        {
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                // Check players and their tasks
                DoomScroll._log.LogInfo("player: " + player.PlayerName + ", role: " + player.RoleType);
                if (player.Tasks != null && player.Tasks.Count > 0)
                {
                    foreach (GameData.TaskInfo taskinfo in player.Tasks)
                    {
                        NormalPlayerTask playerTask = __instance.GetTaskById(taskinfo.TypeId);
                        DoomScroll._log.LogInfo("TASK: name: " + playerTask.name + ", Index: " + playerTask.Index +
                            ", type: " + playerTask.TaskType); // debug
                    }
                }             
            }
           
        }
    }
}
