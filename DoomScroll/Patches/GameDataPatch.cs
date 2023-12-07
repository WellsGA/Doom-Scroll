using Doom_Scroll.Common;
using HarmonyLib;
using Hazel;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(GameData))]
    class GameDataPatch
    {
        private static byte votingTaskID = 255;
        private static byte headlineTaskID = 254;

        [HarmonyPostfix]
        [HarmonyPatch("SetTasks")]
        public static void PostfixSetTasks(ref byte playerId)
        {
            // set locally
            foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
            {
                if (playerControl.PlayerId == playerId)
                {
                    AddDummyTasksToThisList(playerControl.PlayerId);
                    return;
                }
            }
        }
        public static void RPCCompleteHeadlineDummyTask()
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.COMPLETEDUMMYTASK, (SendOption)1); // sends through the TaskTypeID as well as the id of the player sending it
            messageWriter.Write(headlineTaskID);
            messageWriter.Write(PlayerControl.LocalPlayer.PlayerId);
            messageWriter.EndMessage();
        }
        public static void RPCCompleteVotingDummyTask()
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.COMPLETEDUMMYTASK, (SendOption)1);
            messageWriter.Write(votingTaskID);
            messageWriter.Write(PlayerControl.LocalPlayer.PlayerId);
            messageWriter.EndMessage();
        }

        public static void AddDummyTasksToThisList(byte playerID)
        {
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (playerInfo.PlayerId == playerID)
                {
                    DoomScroll._log.LogInfo($"Found player with ID {playerID}");
                    if (playerInfo.Tasks != null || playerInfo.Tasks.Count != 0)
                    {
                        DoomScroll._log.LogInfo($"This player does already have tasks!");

                        // Adding Headline Task
                        GameData.TaskInfo dummyHeadlineTask = new GameData.TaskInfo();
                        dummyHeadlineTask.Id = (uint)(headlineTaskID - playerInfo.PlayerId);
                        DoomScroll._log.LogInfo($"Headline task's task ID: " + dummyHeadlineTask.Id.ToString());
                        dummyHeadlineTask.TypeId = headlineTaskID;
                        playerInfo.Tasks.Add(dummyHeadlineTask);
                        DoomScroll._log.LogInfo($"Added headline task for player {playerInfo.PlayerId}!: " + dummyHeadlineTask.ToString());

                        // Adding Voting Task
                        GameData.TaskInfo dummyVotingTask = new GameData.TaskInfo();
                        dummyVotingTask.Id = (uint)(votingTaskID);
                        DoomScroll._log.LogInfo($"Voting task's task ID: " + dummyVotingTask.Id.ToString());
                        dummyVotingTask.TypeId = votingTaskID;
                        playerInfo.Tasks.Add(dummyVotingTask);
                        DoomScroll._log.LogInfo($"Added voting task for player {playerInfo.PlayerId}!: " + dummyVotingTask.ToString());
                    }
                    else
                    {

                        DoomScroll._log.LogInfo($"This player does NOT already have tasks. BOOO");
                    }
                }
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

                        DoomScroll._log.LogInfo($"task type id is {task.TypeId}");
                        if (task.TypeId == headlineTaskID)
                        {
                            DoomScroll._log.LogInfo($"Does {task.TypeId} == {headlineTaskID}? Yes!");
                            DoomScroll._log.LogInfo("Headline task found!");
                            // The code that they use
                            task.Complete = true;
                            if (PlayerControl.LocalPlayer)
                            {
                                if (DestroyableSingleton<HudManager>.InstanceExists)
                                {
                                    DestroyableSingleton<HudManager>.Instance.ShowTaskComplete();
                                    /*if (PlayerTask.AllTasksCompleted(PlayerControl.LocalPlayer))
                                    {
                                        StatsManager.Instance.IncrementStat(StringNames.StatsAllTasksCompleted);
                                    }*/
                                }
                                PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);
                                DoomScroll._log.LogInfo("RPC for Complete headline task sent!");
                                break;
                            }
                            //My old code, which works on practicemode
                            /*
                            DoomScroll._log.LogInfo("Headline task found!");
                            task.Complete = true;
                            PlayerControl.LocalPlayer.RpcCompleteTask(task.TypeId);
                            DoomScroll._log.LogInfo("RPC for Complete headline task sent!");
                            break;
                            */
                        }
                        DoomScroll._log.LogInfo("Not headline task.");
                    }
                }
            }
        }
        public static void UpdateBlankVotingTaskCompletion()
        {

        }
    }
}
