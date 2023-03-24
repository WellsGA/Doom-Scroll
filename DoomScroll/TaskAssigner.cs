using Hazel;
using System.Collections.Generic;
using UnityEngine;
using Doom_Scroll.UI;
using Doom_Scroll.Common;
using System.Reflection;
using static Il2CppSystem.Threading.SemaphoreSlim;

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
        private static List<AssignedTask> AssignedTasks;
        // list ofassignable tasks
        public List<uint> AssignableTasks { get; private set; }
        public int MaxAssignableTasks { get; private set; }
        public Dictionary<byte, CustomButton> PlayerButtons { get; private set; }
        public uint CurrentMinigameTask { get; private set; }
        
        // UI elements
        public CustomModal PlayerButtonHolder { get; private set; }
        private CustomText panelTitle;
        private Sprite panelSprite;
        private Sprite[] butttonSprite;
        private Sprite playerSprite;
        public bool isAssignerPanelActive;

        // private constructor: the class cannot be instantiated outside of itself; therefore, this is the only instance that can exist in the system
        private TaskAssigner()
        {
            InitTaskAssigner();
            DoomScroll._log.LogInfo("TASK ASSIGNER CONSTRUCTOR");
        }

        private void InitTaskAssigner()
        {
            MaxAssignableTasks = 3;
            AssignableTasks = new List<uint>();
            AssignedTasks = new List<AssignedTask>();
            PlayerButtons = new Dictionary<byte, CustomButton>();
            isAssignerPanelActive = false;
        }

        public void ActivatePanel(bool flag) 
        {
            PlayerButtonHolder.UIGameObject.SetActive(flag);
            isAssignerPanelActive = flag;
        }

        public void SetCurrentMinigameTask(uint id)
        {
            CurrentMinigameTask = id;
        }
        public void SetAssignableTasks(List<uint> tasks)
        {
            AssignableTasks = tasks;
        }
       
        public void RPCAddToAssignedTasks(byte player, uint task) 
        {
            if (AmongUsClient.Instance.AmClient)
            {
                AddToAssignedTasks(PlayerControl.LocalPlayer, player, task);
                AssignableTasks.Remove(task);
            }
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDASSIGNEDTASK, (SendOption)1);
            messageWriter.Write(player);
            messageWriter.Write(task);
            messageWriter.EndMessage();
        }
        public void AddToAssignedTasks(PlayerControl sender, byte playerID, uint taskId)
        {
            foreach (PlayerTask task in sender.myTasks)
            {
                if (task.Id == taskId)
                {
                    AssignedTasks.Add(new AssignedTask(task.Id, task.TaskType, sender.PlayerId, playerID));
                    DoomScroll._log.LogInfo("TASK ASSIGNED TO PLAYER: " + playerID + ", TASK ID:" + task.TaskType);
                }
            }
        }
        
        public void CheckForPlayerButtonClick()
        {
            if (PlayerButtons == null || PlayerButtons.Count == 0) return;
            foreach( KeyValuePair<byte, CustomButton> item in PlayerButtons)
            {
                item.Value.ReplaceImgageOnHover();
                if (item.Value.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    RPCAddToAssignedTasks(item.Key, CurrentMinigameTask);
                    ActivatePanel(false);
                    DoomScroll._log.LogInfo("Task assigned: " + CurrentMinigameTask + ", to player: " + item.Key);
                }
            }
        }

        public void Reset()
        {
            InitTaskAssigner();
            DoomScroll._log.LogInfo("TASK ASSIGNER MANAGER RESET");
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
            foreach (AssignedTask entry in AssignedTasks)
            {     
                assignedTasks += entry.AssigneeName + "\t\t" + entry.Type + "\n";
            }
            return assignedTasks;
        }

        // UI Elements
        // creates the panel with player buttons for each opened assignable minigame 
        public void CreateTaskAssignerPanel()
        {
            if (!HudManager.Instance) return;
            // Sprites: panel, button background, button icon
            panelSprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            butttonSprite = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.emptyBtn.png", slices);
            playerSprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.playerIcon.png");

            // create the panel
            DoomScroll._log.LogInfo("player count: " + GameData.Instance.AllPlayers.Count);
            GameObject parentPanel = HudManager.Instance.gameObject;
            PlayerButtonHolder = new CustomModal(parentPanel, "Button holder", panelSprite);
            Vector2 size = new Vector2(GameData.Instance.AllPlayers.Count/1.5f + 1f, 1f);
            Vector3 pos = new Vector3(0, 0,- 50);
            PlayerButtonHolder.SetSize(size);
            PlayerButtonHolder.SetLocalPosition(pos);
            CustomText title = new CustomText(PlayerButtonHolder.UIGameObject,"Panel Title", "Select a player to assign this task.");
            title.SetLocalPosition(new Vector3(0, 0.3f, -10));
            title.SetSize(1.6f);
            Vector3 topLeftPos = new Vector3(pos.x - size.x/2 + 0.5f, pos.y - 0.1f, pos.z - 10);

            // add the players as buttons
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (!playerInfo.IsDead)
                {
                    CustomButton btn = new CustomButton(PlayerButtonHolder.UIGameObject, playerInfo.PlayerName, butttonSprite, topLeftPos, 0.45f);
                    CustomText label = new CustomText(btn.UIGameObject, playerInfo.PlayerName + "- label", playerInfo.PlayerName);
                    label.SetLocalPosition(new Vector3(0, -btn.GetSize().x/2 - 0.05f, -10));
                    label.SetSize(1.2f);
                    SpriteRenderer sr = btn.AddIconToButton(playerSprite);
                    sr.color = Palette.PlayerColors[playerInfo.DefaultOutfit.ColorId];
                    PlayerButtons.Add(playerInfo.PlayerId, btn);
                    DoomScroll._log.LogInfo("Playercolor: " + playerInfo.ColorName);
                    topLeftPos.x += 0.6f;
                }
            }
            // inactive at first, gets activated on task completition
            ActivatePanel(false);
        }

    }
}
