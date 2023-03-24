using UnityEngine;

namespace Doom_Scroll
{
    public class AssignedTask
    {
        public uint TaskId { get; private set; }
        public TaskTypes Type { get; private set; }
        public byte AssignedBy { get; private set; }
        public byte AssigneeId { get; private set; }
        public string AssigneeName { get; private set; }
        // private Time timeAdded;
        // other fields

        public AssignedTask(uint task, TaskTypes type, byte player, byte assignee)
        {
            TaskId = task;
            Type = type;
            AssignedBy = player;
            AssigneeId = assignee;   
            GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(assignee);
            AssigneeName = playerInfo == null ? "Unknown player": playerInfo.PlayerName;  // if player has left, we don't know
        }
    }
}
