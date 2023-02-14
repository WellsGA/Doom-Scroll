using Hazel;
using System.Collections.Generic;


namespace Doom_Scroll
{
    // Singleton with static initialization: thread safe without explicitly coding for it,
    // relies on the common language runtime to initialize the variable
    public sealed class TaskAssigner
    {
        private static readonly TaskAssigner _instance = new TaskAssigner(); // readonly: can be assigned only during static initialization
        public static TaskAssigner Instance
        {
            get
            {
                return _instance;
            }
        }
        // list for all the assigned task - this will be displayed during meetings
        private static List<(byte playerId, string taskName)> AssignedTasks = new List<(byte, string)>();
        // byte array to keep the assignable task IDs
        public uint[] AssignableTasksIDs { get; private set; }
        // private constructor: the class cannot be instantiated outside of itself; therefore, this is the only instance that can exist in the system
        private TaskAssigner()
        {
            AssignableTasksIDs = new uint[2];
            DoomScroll._log.LogInfo("TASK ASSIGNER CONSTRUCTOR");
        }

        public void AddToAssignedTasks(PlayerControl sender, byte playerID, uint taskId)
        {
            string taskName = " o_O ";
            foreach (PlayerTask task in sender.myTasks)
            {
                if(task.Id == taskId)
                {
                    taskName = task.name;
                }
            }
            // add to list  (maybe we need to check if the task was already assigned and change if so??)
            AssignedTasks.Add((playerID, taskName));
            DoomScroll._log.LogInfo("TASK ASSIGNED TO PLAYER: " + playerID + ", TASK ID:" + taskName);
        }

        public void RPCAddToAssignedTasks(byte player, uint task) 
        {
            if (AmongUsClient.Instance.AmClient)
            {
                AddToAssignedTasks(PlayerControl.LocalPlayer, player, task);
            }
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDASSIGNEDTASK, (SendOption)1);
            messageWriter.Write(player);
            messageWriter.Write(task);
            messageWriter.EndMessage();
        }

        public override string ToString()
        {
            string assignedTasks = "\t<NAME>\t\t\t<TASK> \t\n  " +
                                   "=====================================\n";
            foreach(var entry in AssignedTasks)
            {
                GameData.PlayerInfo player = GameData.Instance.GetPlayerById(entry.playerId);
                if (player == null) { continue; } // if player has left, we leave them out
                assignedTasks += player.PlayerName + "\t\t\t " + entry.taskName + "\n";
            }
            return assignedTasks;
        }

        public void SelectRandomTasks(Il2CppSystem.Collections.Generic.List<PlayerTask> tasks) 
        {
            AssignedTasks = new List<(byte, string)>();
            if (tasks.Count == 0 || tasks == null) return;
            for (int i = 0; i < AssignableTasksIDs.Length; i++)
            {
                int index = UnityEngine.Random.Range(0, tasks.Count - 1);
                AssignableTasksIDs[i] = tasks[index].Id; /// AAAAAAA
                DoomScroll._log.LogInfo("Task You Can Assign: " + tasks[index].name + "(" + tasks[index].Id.ToString() + ")" );
            }
        }

        public void AssignPlayerToTask(uint id)
        {
            // random select a player for now 
            if (PlayerControl.AllPlayerControls.Count == 0 || PlayerControl.AllPlayerControls == null) { return; }
            int index = UnityEngine.Random.Range(0, PlayerControl.AllPlayerControls.Count-1);
            PlayerControl player = PlayerControl.AllPlayerControls[index];
            RPCAddToAssignedTasks(player.PlayerId, id);       
        }

        public void DisplayAssignedTasks()
        {
            DoomScroll._log.LogInfo("TASKS ASSIGNED SO FAR:\n " + ToString());
        }
    }
}
