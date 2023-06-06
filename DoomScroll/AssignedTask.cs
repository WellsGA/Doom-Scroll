using Doom_Scroll.Common;
using Doom_Scroll.UI;
using UnityEngine;
using System.Reflection;
using static Il2CppSystem.Runtime.Remoting.RemotingServices;

namespace Doom_Scroll
{
    public class AssignedTask
    {
        public uint TaskId { get; private set; }
        public TaskTypes Type { get; private set; }
        public byte AssignedBy { get; private set; }
        public byte AssigneeId { get; private set; }
        public string AssigneeName { get; private set; }

        public CustomModal Card { get; private set; }
        private static Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.card.png");

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
        public void DisplayTaskCard(CustomModal parent)
        {
            Card = new CustomModal(parent.UIGameObject, "card item", spr);
            Card.SetSize(new Vector3(parent.GetSize().x - 2f, 0.4f, 0));
            CustomText assignedTask = new CustomText(Card.UIGameObject, "task", AssigneeName + " " + Type.ToString());
            assignedTask.SetSize(1.5f);
            //assignedTask.SetTextAlignment(TMPro.TextAlignmentOptions.BaselineLeft);
            Card.ActivateCustomUI(false); // not yet active in case sizing and positioning is needed
        }
        public bool IsTaskFinished()
        {
            // chack if task is completed
            return true;
        }
    }
}
