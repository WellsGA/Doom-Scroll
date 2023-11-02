using Doom_Scroll.Common;
using Doom_Scroll.UI;
using UnityEngine;
using System.Reflection;
using Doom_Scroll.Patches;
using Hazel;

namespace Doom_Scroll
{
    public class AssignedTask
    {
        public uint TaskId { get; private set; }
        public TaskTypes Type { get; private set; }
        public byte AssignedBy { get; private set; }
        public byte AssigneeId { get; private set; }
        public string AssigneeName { get; private set; }

        private CustomText cardText;
        private bool isTaskDone;

        public CustomButton PostButton { get; private set; }

        public CustomModal Card { get; private set; }

        // private Time timeAdded;
        // other fields

        public AssignedTask(uint task, TaskTypes type, byte player, byte assignee)
        {
            TaskId = task;
            Type = type;
            AssignedBy = player;
            AssigneeId = assignee;   
            GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(assignee);
            AssigneeName = playerInfo == null ? "Unknown player": playerInfo.PlayerName;  // if player has left, we don't know;
            CreateTaskCard();
        }
        public void DisplayTaskCard()
        {
            if (!isTaskDone) isTaskDone = CheckIfFinished(); // checking state again before displaying
            string taskState = isTaskDone ? " finished " : " is working on ";
            cardText.SetText(AssigneeName + taskState + Type.ToString());
            Card.ActivateCustomUI(true);
        }
        public void CreateTaskCard()
        {
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.card.png");
            CustomModal parent = FolderManager.Instance.GetFolderArea();
            Card = new CustomModal(parent.UIGameObject, "card item", spr);
            Card.SetSize(new Vector3(parent.GetSize().x - 2f, 0.3f, 0)); 
            cardText = new CustomText(Card.UIGameObject, "task", "Default");
            cardText.SetSize(1.5f);
            AddShareButton();
            Card.ActivateCustomUI(false); // not yet active in case sizing and positioning is needed
        }
        private void AddShareButton()
        {
            float shareBtnSize = Card.GetSize().y - 0.02f;
            Vector3 position = new Vector3(Card.GetSize().x / 2 - 0.05f, 0, -20);
            Sprite[] BtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.postButton.png", ImageLoader.slices2);
            PostButton = new CustomButton(Card.UIGameObject, "Post News", BtnSprites, position, shareBtnSize);
            PostButton.ButtonEvent.MyAction += OnClickShare;
        }

        private void OnClickShare()
        {
            string chatText = "<color=#366999><i>" + cardText.TextMP.text;
            if (DestroyableSingleton<HudManager>.Instance && AmongUsClient.Instance.AmClient)
            {
                string chatID = ChatControllerPatch.GetChatID();
                ChatControllerPatch.content = ChatContent.TEXT;
                chatText = chatID + chatText;
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, chatText, false);

                RpcPostTask(chatText);
                PostButton.EnableButton(false);
            }
        }

        private void RpcPostTask(string text)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDTEXTCHAT, (SendOption)1);
            messageWriter.Write(text);
            messageWriter.EndMessage();
        }

        public bool CheckIfFinished()
        {
            GameData.PlayerInfo player = GameData.Instance.GetPlayerById(AssignedBy);
            if(player == null) return false;
            foreach(GameData.TaskInfo task in player.Tasks)
            {
                if(task.Id == TaskId)
                {
                    return task.Complete;
                }
            }
            return false;
        }
    }
}
