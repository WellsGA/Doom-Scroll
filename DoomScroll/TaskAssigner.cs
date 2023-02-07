using Doom_Scroll.Common;
using Hazel;
using UnityEngine;
using System.Collections.Generic;
using Doom_Scroll.UI;
using System;
using Il2CppInterop.Runtime;

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
        private static Dictionary<byte, byte> AssignedTasks = new Dictionary<byte, byte>();
        // byte array to keep the assignable task IDs
        public byte[] AssignableTasksIDs { get; private set; }
        // private constructor: the class cannot be instantiated outside of itself; therefore, this is the only instance that can exist in the system
        private TaskAssigner()
        {
            AssignableTasksIDs = new byte[2];
            DoomScroll._log.LogInfo("TASK ASSIGNER CONSTRUCTOR");
        }

        public void AddToAssignedTasks(byte playerID, byte taskId)
        {
            // add to dictionary  (maybe we need to check if the task was already assigned and change if so??)
            AssignedTasks.Add(playerID, taskId);
        }

        public void RPCAddToAssignedTasks(byte player, byte task) 
        {
            if (AmongUsClient.Instance.AmClient)
            {
                AddToAssignedTasks(player, task);
            }
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDASSIGNEDTASK, (SendOption)11);
            messageWriter.Write(player);
            messageWriter.Write(task);
            messageWriter.EndMessage();
        }

        public override string ToString()
        {
            string assignedTasks = "NAME ========= TASK ========\n";
            foreach(KeyValuePair<byte, byte> entry in AssignedTasks)
            {
                GameData.PlayerInfo player = GameData.Instance.GetPlayerById(entry.Key);
                NormalPlayerTask task = ShipStatus.Instance.GetTaskById(entry.Value);
                if(player == null || task == null)
                {
                    continue;
                }
                assignedTasks += player.PlayerName + " " + task.name + "\n";
            }
            return assignedTasks;
        }

        public void SelectRandomTasks(byte[] tasks) 
        {
            if (tasks.Length == 0 || tasks == null)
            {
                DoomScroll._log.LogInfo("NO TASKS ...");
                return; 
            }
            for (int i = 0; i < AssignableTasksIDs.Length; i++)
            {
                int index = UnityEngine.Random.Range(0, tasks.Length - 1);
                AssignableTasksIDs[i] = tasks[index];
                DoomScroll._log.LogInfo("TASKS YOU CAN ASSIGN: " + tasks[index]);
            }  
        }

        public void AssignPlayerToTask(byte typeid)
        {
            // random select a player for now
            Il2CppSystem.Collections.Generic.List<PlayerControl> players = PlayerControl.AllPlayerControls;
            if (players.Count == 0 || players == null) { return; }
            int index = UnityEngine.Random.Range(0, players.Count-1);
            PlayerControl player = players[index];
            RPCAddToAssignedTasks(player.PlayerId, typeid);       
        }

        public void DisplayAssignedTasks()
        {
            DoomScroll._log.LogInfo("TASKS ASSIGNED SO FAR:\n " + ToString());
        }

    }
}
