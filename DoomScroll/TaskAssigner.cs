using Hazel;
using System.Collections.Generic;
using UnityEngine;
using Doom_Scroll.UI;
using Doom_Scroll.Common;
using System.Reflection;
using System;
using Doom_Scroll.Patches;

namespace Doom_Scroll
{
    // Singleton with static initialization: thread safe without explicitly coding for it,
    // relies on the common language runtime to initialize the variable
    public sealed class TaskAssigner
    {
        private int maxTaskItemsPerPage = 9; // THIS VALUE SHOULD NOT BE CHANGED IN CLASS

        private static readonly TaskAssigner _instance = new TaskAssigner(); // readonly: can be assigned only during static initialization
        public static TaskAssigner Instance
        {
            get
            {
                return _instance;
            }
        }
        // list of tasks assigned by each player - this will be displayed during meetings
        public List<AssignedTask> AssignedTasks { get; private set; }
        // list ofassignable tasks
        public List<uint> AssignableTasks { get; private set; }
        public int MaxAssignableTasks { get; private set; }
        public CustomSelect<byte> PlayerButtons { get; private set; }
        public uint CurrentMinigameTask { get; private set; }
        
        // UI elements
        public CustomModal PlayerButtonHolder { get; private set; }
        private Tooltip playerButtonHolderTooltip;
        public bool isAssignerPanelActive;

        // elements added by Alaina for flipping between pages of tasks
        private Pageable taskPageHolder;
        private int numPages = 1;

        // private constructor: the class cannot be instantiated outside of itself; therefore, this is the only instance that can exist in the system
        private TaskAssigner()
        {
            InitTaskAssigner();
            DoomScroll._log.LogInfo("TASK ASSIGNER CONSTRUCTOR");
        }

        private void InitTaskAssigner()
        {
            MaxAssignableTasks = 2;
            AssignableTasks = new List<uint>();
            AssignedTasks = new List<AssignedTask>();
            isAssignerPanelActive = false;

            // set up stuff for folder display, paging through. Set it false for now because not necessary yet.
            CustomModal parent = FolderManager.Instance.GetFolderArea();
            taskPageHolder = new Pageable(parent, new List<CustomUI>(), maxTaskItemsPerPage); // sets up an empty pageable 
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
            messageWriter.Write(player); // assignee
            messageWriter.Write(task); //task
            messageWriter.EndMessage();
        }
        public void AddToAssignedTasks(PlayerControl sender, byte playerID, uint taskId)
        {
            foreach (PlayerTask task in sender.myTasks)
            {
                if (task.Id == taskId) // check if sender really had that task and get the type
                {
                    AssignedTask assTask = new AssignedTask(task.Id, task.TaskType, sender.PlayerId, playerID);
                    AssignedTasks.Insert(0, assTask);
                    NotificationManager.ShowNotification(assTask.AssigneeName + " signed into a task.");
                    DoomScroll._log.LogInfo("New task assigned\n player: " + assTask.AssigneeName + ", task: " + assTask.Type);
                    if (AmongUsClient.Instance.AmHost)
                    {
                        GameLogger.Write(GameLogger.GetTime() + " - " + sender.name + " signed " + assTask.AssigneeName  + " into a task: " + assTask.Type);
                    }
                }
            }
        }
        
        public void CheckForPlayerButtonClick()
        {
            if (PlayerButtons == null || PlayerButtons.GetNumberOfOptions() == 0) return;
            PlayerButtons.ListenForSelection();
            if (PlayerButtons.HasSelected)
            {
                RPCAddToAssignedTasks(PlayerButtons.Selected.Key, CurrentMinigameTask);
                ActivatePanel(false);
            }
        }

        public void CheckForShareTaskClicks()
        {
            if (HudManager.Instance == null) return;
            // If chat and folder overlay are open invoke events on button clicks
            if (HudManager.Instance.Chat.State == ChatControllerState.Open && FolderManager.Instance.IsFolderOpen())
            {
                try
                {
                    foreach (AssignedTask task in AssignedTasks)
                    {
                        task.PostButton.ReplaceImgageOnHover();
                        if (task.PostButton.IsEnabled && task.PostButton.IsActive && task.PostButton.IsHovered() && Input.GetKey(KeyCode.Mouse0))
                        {
                            task.PostButton.ButtonEvent.InvokeAction();
                        }
                    }
                }
                catch (Exception e)
                {
                    DoomScroll._log.LogError("Error invoking share task button method: " + e);
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
            CustomModal parent = FolderManager.Instance.GetFolderArea();
            numPages = (int)Math.Ceiling((float)(AssignedTasks.Count) / FileText.maxNumTextItems);
            DoomScroll._log.LogInfo("Number of pages of news: " + numPages);

            List<CustomUI> taskCards = new List<CustomUI>();
            for (int displayPageNum = 1; displayPageNum <= numPages; displayPageNum++)
            {
                Vector3 pos = new Vector3(0, parent.GetSize().y / 2 - 0.8f, -10);
                for (int currentTaskIndex = (displayPageNum - 1) * maxTaskItemsPerPage; currentTaskIndex < AssignedTasks.Count && currentTaskIndex < displayPageNum * maxTaskItemsPerPage; currentTaskIndex++)
                // stops before index out of range and before printing tasks that should be on next page
                {
                    DoomScroll._log.LogInfo($"Current task Index: {currentTaskIndex}, AssignedTasks list Count: {AssignedTasks.Count}");
                    AssignedTask task = AssignedTasks[currentTaskIndex];
                    task.DisplayTaskCard();
                    pos.y -= task.Card.GetSize().y + 0.05f;
                    task.Card.SetLocalPosition(pos);
                    task.Card.ActivateCustomUI(true);
                    task.PostButton.ActivateCustomUI(true);

                    taskCards.Add(task.Card);
                    task.Card.ActivateCustomUI(false); // unsure if necessary>
                }
            }

            // always show page 1 first
            if (taskPageHolder == null)
            {
                DoomScroll._log.LogInfo($"Creating new pageable");
                taskPageHolder = new Pageable(parent, taskCards, maxTaskItemsPerPage); // sets up an empty pageable 
            }
            else
            {
                DoomScroll._log.LogInfo($"Updating pageable");
                taskPageHolder.UpdatePages(taskCards);
            }
            taskPageHolder.DisplayPage(1);
        }
        public void CheckForDisplayedTasksPageButtonClicks()
        {
            if (taskPageHolder != null)
            {
                taskPageHolder.CheckForDisplayedPageButtonClicks();
            }
        }

        public void HideAssignedTasks()
        {
            if (taskPageHolder != null)
            {
                taskPageHolder.HidePage();
            }
        }
        public override string ToString()
        {
            string assignedTasks = "NAME\t\tTASK \n  " +
                                   "=====================================\n";
            foreach (AssignedTask entry in AssignedTasks)
            {
                // we can check for completition here
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
            Sprite panelSprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");            
            Sprite[] playerButttonSprite = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.playerBtn.png", ImageLoader.slices4);

            // create the panel
            DoomScroll._log.LogInfo("player count: " + GameData.Instance.AllPlayers.Count);
            GameObject parentPanel = HudManager.Instance.gameObject;
            PlayerButtonHolder = new CustomModal(parentPanel, "Button holder", panelSprite);
            Vector2 size = new Vector2(GameData.Instance.AllPlayers.Count/1.5f, 1f);
            Vector2 bounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            Vector3 pos = new Vector3(0, -bounds.y/2+size.y/2,- 50);
            PlayerButtonHolder.SetSize(size);
            PlayerButtonHolder.SetLocalPosition(pos);
            CustomText title = new CustomText(PlayerButtonHolder.UIGameObject,"Panel Title", "Who is completing this task?");
            title.SetLocalPosition(new Vector3(0, 0.3f, -10));
            title.SetSize(1.6f);
            playerButtonHolderTooltip = new Tooltip(PlayerButtonHolder.UIGameObject, "HeadlinePopup", "Choose whether to protect\n or frame, then choose a\ntarget.This will generate a\nheadline about your target.", 0.75f, 2f, new Vector3(-3f, 0, 0), 1f);

            // add the players as buttons
            PlayerButtons = new CustomSelect<byte>(size);

            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (!playerInfo.IsDead && !playerInfo.Disconnected)
                {
                    CustomButton btn = new CustomButton(PlayerButtonHolder.UIGameObject, playerInfo.PlayerName, playerButttonSprite);
                    btn.Label.SetText(playerInfo.PlayerName);                 
                    btn.SetDefaultBtnColor(btn.TopIcon, Palette.PlayerColors[playerInfo.DefaultOutfit.ColorId]);
                    PlayerButtons.AddSelectOption(playerInfo.PlayerId, btn);      
                }
                PlayerButtons.ArrangeButtons(0.48f, PlayerButtons.GetNumberOfOptions(), 0.48f, 0.7f);
            }
            // inactive at first, gets activated on task completition
            ActivatePanel(false);
        }
    }
}
