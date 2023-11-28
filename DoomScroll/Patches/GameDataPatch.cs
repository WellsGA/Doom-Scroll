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
            dummyTask.Id= 0;
            dummyTask.TypeId = 255;
            localPlayerInfo.Tasks.Add(dummyTask);
        }

        public static void UpdateBlankHeadlineTaskCompletion()
        {
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (playerInfo.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    foreach (GameData.TaskInfo task in playerInfo.Tasks)
                    {
                        if (task.TypeId == 255)
                        {
                            task.Complete = true;
                        }
                    }
                }
            }
        }
    }
}
