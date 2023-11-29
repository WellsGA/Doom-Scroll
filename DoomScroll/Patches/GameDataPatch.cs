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
            GameData.PlayerInfo localPlayerInfo = null;

            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (playerInfo.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    localPlayerInfo = playerInfo;
                }
            }
            if (localPlayerInfo == null)
            {
                return;
            }
            while (localPlayerInfo.Tasks == null || localPlayerInfo.Tasks.Count == 0)
            {
                await Task.Delay(500);
            }
            GameData.TaskInfo dummyTask = new GameData.TaskInfo();
            dummyTask.Id= 255;
            dummyTask.TypeId = 255;
            localPlayerInfo.Tasks.Add(dummyTask);
            DoomScroll._log.LogInfo("Added headline task!: " + dummyTask.ToString());
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
