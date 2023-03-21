using Hazel;
using System.Collections.Generic;
using UnityEngine;
using Doom_Scroll.UI;
using Doom_Scroll.Common;
using System.Reflection;

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
        // list of tasks assigned by each player - this will be displayed during meetings
        private static List<(byte playerId, string taskName)> AssignedTasks;
        // byte array to hold the assignable task IDs
        public uint[] AssignableTasksIDs { get; private set; }
        public int MaxAssignableTasks { get; private set; }
        public Dictionary<byte, CustomButton> PlayerButtons { get; private set; }
        public uint CurrentMinigameTask { get; private set; }


        // UI elements
        private Sprite panelSprite;
        private Sprite[] butttonSprite;
        private Sprite playerSprite;
        
        // private constructor: the class cannot be instantiated outside of itself; therefore, this is the only instance that can exist in the system
        private TaskAssigner()
        {
            InitTaskAssigner();
            DoomScroll._log.LogInfo("TASK ASSIGNER CONSTRUCTOR");
        }

        private void InitTaskAssigner()
        {
            MaxAssignableTasks = 2;
            AssignableTasksIDs = new uint[MaxAssignableTasks];
            AssignedTasks = new List<(byte, string)>();
            PlayerButtons = new Dictionary<byte, CustomButton>();
            panelSprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            butttonSprite = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.emptyBtn.png", slices);
            playerSprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.playerIcon.png");
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

        public void SetAssignableTask(uint[] id) 
        {
            AssignedTasks = new List<(byte, string)>();
            if (id.Length <= AssignableTasksIDs.Length)
            {
                AssignableTasksIDs = id;
            }
        }

        public void AssignPlayerToTask(uint taskId, byte playerId)
        {
            PlayerButtons = new Dictionary<byte, CustomButton>(); // reinit this Dictionary, so it will be empty when the Miigame opens

            // assign the task to the selected player and notify others
            RPCAddToAssignedTasks(playerId, taskId);       
        }

        public void DisplayAssignedTasks()
        {
            // to do: list it on a UI modal 
            DoomScroll._log.LogInfo("TASKS ASSIGNED SO FAR:\n " + ToString()); // debug
        }

        public override string ToString()
        {
            string assignedTasks = "<NAME>\t\t<TASK> \n  " +
                                   "=====================================\n";
            foreach (var entry in AssignedTasks)
            {
                GameData.PlayerInfo player = GameData.Instance.GetPlayerById(entry.playerId);
                if (player == null) { continue; } // if player has left, we leave them out
                assignedTasks += player.PlayerName + "\t\t" + entry.taskName + "\n";
            }
            return assignedTasks;
        }

        // creates the panel with player buttons for each opened assignable minigame 
        // it's going to be a child objecy of the Minigame prefab, therefore, it gets destroyed when the parent is destroyed!
        public void CreateTaskAssignerPanel(GameObject closeBtn, uint taskId)
        {
            CurrentMinigameTask = taskId;

            GameObject parentPanel = closeBtn.transform.parent.gameObject;
            CustomModal playerButtonHolder = new CustomModal(parentPanel, "Button holder", panelSprite);
            Vector2 size = new Vector2(GameData.Instance.AllPlayers.Count/2 + 1f, 0.5f);
            Vector3 pos = new Vector3(closeBtn.transform.localPosition.x + size.x/ 2 + 0.5f, closeBtn.transform.localPosition.y + 0.3f, closeBtn.transform.localPosition.z - 10);
            playerButtonHolder.SetSize(size);
            playerButtonHolder.SetLocalPosition(pos);
            Vector3 topLeftPos = new Vector3(closeBtn.transform.localPosition.x + 0.8f, pos.y, pos.z-20);

            // add the players as buttons
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (!playerInfo.IsDead)
                {                  
                    CustomButton btn = new CustomButton(parentPanel, playerInfo.PlayerName, butttonSprite, topLeftPos, 0.4f);
                    SpriteRenderer sr = btn.AddIconToButton(playerSprite);
                    sr.color = Palette.PlayerColors[playerInfo.DefaultOutfit.ColorId];
                    PlayerButtons.Add(playerInfo.PlayerId, btn);
                    DoomScroll._log.LogInfo("Playercolor: " + playerInfo.ColorName );
                    topLeftPos.x += 0.4f;
                }
            }
            // inactive at first, gets activated on task completition
            // playerButtonHolder.UIGameObject.SetActive(false);
        }

        public void CheckForPlayerButtonClick()
        {
            if (PlayerButtons.Count == 0) return;
            foreach( KeyValuePair<byte, CustomButton> item in PlayerButtons)
            {
                item.Value.ReplaceImgageOnHover();
                if (item.Value.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    AssignPlayerToTask(CurrentMinigameTask, item.Key);
                    DoomScroll._log.LogInfo("Task assigned: " + CurrentMinigameTask + ", to player: " + item.Key);
                }
            }
        }

        public void Reset()
        {
            InitTaskAssigner();
            DoomScroll._log.LogInfo("TASK ASSIGNER MANAGER RESET");
        }

    }
}
