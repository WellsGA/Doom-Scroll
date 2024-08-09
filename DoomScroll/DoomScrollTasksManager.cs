
using Doom_Scroll.Patches;
using Hazel;

namespace Doom_Scroll
{
    internal static class DoomScrollTasksManager
    {
        public static byte VotingTaskID { get; private set; } = 255;
        public static byte HeadlineTaskID { get; private set; } = 254;

        public static void AddHeadlineTaskInfos()
        {
            // We only create taskInfo maintained in the GameData class to track the completition of all tasks,
            // we do not create a PlayerTask! - no need for prefab, task steps, time, etc.
            foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers)
            {
                // Add Headline posting task
                player.Tasks.Add(new NetworkedPlayerInfo.TaskInfo(HeadlineTaskID, (uint)(HeadlineTaskID - player.PlayerId)));
                GameData.Instance.TotalTasks++;
                // Add Headline sorting task only for crewmates
                if (player.Role.Role != AmongUs.GameOptions.RoleTypes.Impostor)
                {
                    player.Tasks.Add(new NetworkedPlayerInfo.TaskInfo(VotingTaskID, VotingTaskID));
                    GameData.Instance.TotalTasks++;
                }
            }
        }
        public static void RPCCompleteHeadlinePostTask()
        {
            if (AmongUsClient.Instance.AmClient) 
            {
                if (DestroyableSingleton<HudManager>.InstanceExists)
                {
                    // Displays the "task completed" message
                    DestroyableSingleton<HudManager>.Instance.ShowTaskComplete();
                }
                // sets taskinfo's  Complete member to true, and increases the nr of completed tasks
                GameData.Instance.CompleteTask(PlayerControl.LocalPlayer, HeadlineTaskID); 
            }  
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.COMPLETEDUMMYTASK, (SendOption)1);
            messageWriter.Write(HeadlineTaskID);
            messageWriter.Write(PlayerControl.LocalPlayer.PlayerId);
            messageWriter.EndMessage();
        }

        // TO DO: check this, it's not called...
        public static void RPCCompleteVotingDummyTask()
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.COMPLETEDUMMYTASK, (SendOption)1);
            messageWriter.Write(VotingTaskID);
            messageWriter.Write(PlayerControl.LocalPlayer.PlayerId);
            messageWriter.EndMessage();
        }

        // I don't think this is needed
        public static void UpdateBlankHeadlineTaskCompletion()
        {
            foreach (NetworkedPlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (playerInfo.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    foreach (NetworkedPlayerInfo.TaskInfo task in playerInfo.Tasks)
                    {
                        if (task.Id == HeadlineTaskID - playerInfo.PlayerId)
                        {
                            // The code that they use
                            task.Complete = true;
                            if (PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.PlayerId == playerInfo.PlayerId)
                            {
                                if (DestroyableSingleton<HudManager>.InstanceExists)
                                {
                                    DestroyableSingleton<HudManager>.Instance.ShowTaskComplete();
                                    /*if (PlayerTask.AllTasksCompleted(PlayerControl.LocalPlayer))
                                    {
                                        StatsManager.Instance.IncrementStat(StringNames.StatsAllTasksCompleted);
                                    }*/
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }
        public static void UpdateHeadlineSortingCompletion(bool setCompleted)
        {

            foreach (NetworkedPlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                foreach (NetworkedPlayerInfo.TaskInfo task in playerInfo.Tasks)
                {
                    if (task.Id == VotingTaskID)
                    {
                        //DoomScroll._log.LogInfo("Voting task found!");
                        task.Complete = setCompleted;
                        break;
                    }
                }
            }
        }
    }
}
