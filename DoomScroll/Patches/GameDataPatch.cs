using HarmonyLib;
using Hazel;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(GameData))]
    class GameDataPatch
    {
        public static byte votingTaskID { get; private set; } = 255;
        public static byte headlineTaskID { get; private set; } = 254;

        [HarmonyPostfix]
        [HarmonyPatch("AddPlayer")]
        public static void PostfixAddPlayer(PlayerControl pc) 
        {
            ScreenshotManager.Instance.AddPlayerToTheWaitList(pc.PlayerId); // fills up the waitlist for screenshot taking
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetTasks")]
        public static void PostfixSetTasks(ref byte playerId)
        {
            DoomScroll._log.LogInfo($"Running Postfix set tasks.");
            // set locally
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (playerInfo.PlayerId == playerId)
                {
                    AddDummyTasksToThisList(playerInfo);
                    return;
                }
            }
        }
        public static void RPCCompleteHeadlineDummyTask()
        {
            PlayerControl.LocalPlayer.RpcCompleteTask(GameDataPatch.headlineTaskID);
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

        public static void AddDummyTasksToThisList(GameData.PlayerInfo thisPlayer)
        {
            DoomScroll._log.LogInfo($"Found player with ID {thisPlayer.PlayerId}");

            // log stuff for debugging
            //DoomScroll._log.LogInfo("Player's Current List of Tasks:");
            foreach (GameData.TaskInfo task in thisPlayer.Tasks)
            {
                //DoomScroll._log.LogInfo($"TASK WITH ID {task.Id} AND TYPEID {task.TypeId}");
            }
            // end logs

            if (thisPlayer.Tasks != null || thisPlayer.Tasks.Count != 0)
            {
                DoomScroll._log.LogInfo($"This player does already have tasks!");

                // Adding Headline Task
                GameData.TaskInfo dummyHeadlineTask = new GameData.TaskInfo();
                dummyHeadlineTask.Id = (uint)(headlineTaskID - thisPlayer.PlayerId);
                //DoomScroll._log.LogInfo($"Headline task's task ID: " + dummyHeadlineTask.Id.ToString());
                dummyHeadlineTask.TypeId = headlineTaskID;
                thisPlayer.Tasks.Add(dummyHeadlineTask);
                //DoomScroll._log.LogInfo($"Added headline task for player {thisPlayer.PlayerId}!: " + dummyHeadlineTask.ToString());

                // Adding Voting Task
                GameData.TaskInfo dummyVotingTask = new GameData.TaskInfo();
                dummyVotingTask.Id = (uint)(votingTaskID);
                //DoomScroll._log.LogInfo($"Voting task's task ID: " + dummyVotingTask.Id.ToString());
                dummyVotingTask.TypeId = votingTaskID;
                thisPlayer.Tasks.Add(dummyVotingTask);
                //DoomScroll._log.LogInfo($"Added voting task for player {thisPlayer.PlayerId}!: " + dummyVotingTask.ToString());

                // log stuff
                //DoomScroll._log.LogInfo("Player's Updated List of Tasks:");
                foreach (GameData.TaskInfo task in thisPlayer.Tasks)
                {
                    //DoomScroll._log.LogInfo($"TASK WITH ID {task.Id} AND TYPEID {task.TypeId}");
                }
                // end logs
            }
            else
            {

                DoomScroll._log.LogInfo($"This player does NOT already have tasks. BOOO");
            }
        }

        public static void UpdateBlankHeadlineTaskCompletion(byte playerId)
        {
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (playerInfo.PlayerId == playerId)
                {
                    foreach (GameData.TaskInfo task in playerInfo.Tasks)
                    {

                        //DoomScroll._log.LogInfo($"task type id is {task.TypeId} and task id is {task.Id}");
                        if (task.Id == headlineTaskID-playerId)
                        {
                            //DoomScroll._log.LogInfo($"Does {task.Id} == {headlineTaskID-playerId}? Yes!");
                            //DoomScroll._log.LogInfo("Headline task found!");
                            // The code that they use
                            task.Complete = true;
                            if (PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.PlayerId == playerId)
                            {
                                if (DestroyableSingleton<HudManager>.InstanceExists)
                                {
                                    DestroyableSingleton<HudManager>.Instance.ShowTaskComplete();
                                    /*if (PlayerTask.AllTasksCompleted(PlayerControl.LocalPlayer))
                                    {
                                        StatsManager.Instance.IncrementStat(StringNames.StatsAllTasksCompleted);
                                    }*/
                                }
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
                        //DoomScroll._log.LogInfo("Not headline task.");
                    }
                }
            }
        }
        public static void UpdateDummyVotingTaskCompletion(bool  setCompleted)
        {

            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                foreach (GameData.TaskInfo task in playerInfo.Tasks)
                {

                    //DoomScroll._log.LogInfo($"task type id is {task.TypeId} and task id is {task.Id}");
                    if (task.Id == votingTaskID)
                    {
                        //DoomScroll._log.LogInfo($"Does {task.Id} == {votingTaskID}? Yes!");
                        //DoomScroll._log.LogInfo("Voting task found!");
                        task.Complete = setCompleted;
                    }
                    //DoomScroll._log.LogInfo("Voting headline task.");
                }
            }
        }
    }
}
