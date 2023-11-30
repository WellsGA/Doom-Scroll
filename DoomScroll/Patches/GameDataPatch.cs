using Doom_Scroll.Common;
using HarmonyLib;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(GameData))]
    class GameDataPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("RpcSetTasks")]
        public static async void PostfixRPCSetTasks(GameData __instance)
        {
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                while (playerInfo.Tasks == null || playerInfo.Tasks.Count == 0)
                {
                    await Task.Delay(500);
                }
                GameData.TaskInfo dummyTask = new GameData.TaskInfo();
                dummyTask.Id = 255;
                dummyTask.TypeId = 255;
                playerInfo.Tasks.Add(dummyTask);
                DoomScroll._log.LogInfo($"Added headline task for player {playerInfo.PlayerId}!: " + dummyTask.ToString());
            }
        }

        public static void UpdateBlankHeadlineTaskCompletion()
        {
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (playerInfo.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    foreach (GameData.TaskInfo task in playerInfo.Tasks)
                    {
                        DoomScroll._log.LogInfo("Not headline task.");
                        if (task.TypeId == 255)
                        {
                            DoomScroll._log.LogInfo("Headline task found!");
                            task.Complete = true;
                            PlayerControl.LocalPlayer.RpcCompleteTask(task.TypeId);
                            DoomScroll._log.LogInfo("RPC for Complete headline task sent!");
                            break;
                        }
                    }
                }
            }
        }
    }
}
