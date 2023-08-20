using Hazel;
using System.Collections.Generic;
using UnityEngine;
using Doom_Scroll.UI;
using Doom_Scroll.Common;
using System.Reflection;
using System.Threading.Tasks;
using static Il2CppSystem.Runtime.Remoting.RemotingServices;
using Sentry.Internal;
using static Il2CppMono.Security.X509.X520;
using System;
using Il2CppSystem.Threading.Tasks;

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
        public List<AssignedTask> AssignedTasks { get; private set; }
        // list ofassignable tasks
        public List<uint> AssignableTasks { get; private set; }
        public int MaxAssignableTasks { get; private set; }
        public Dictionary<byte, CustomButton> PlayerButtons { get; private set; }
        public uint CurrentMinigameTask { get; private set; }
        
        // UI elements
        public CustomModal PlayerButtonHolder { get; private set; }
        private Sprite panelSprite;
        private Sprite[] butttonSprite;
        private Sprite playerSprite;
        public bool isAssignerPanelActive;
        private Sprite cardSprite;

        // elements added by Alaina for flipping between pages of tasks
        private int numPages = 1;
        private int currentPage = 1;
        private CustomButton m_nextBtn;
        private CustomButton m_backBtn;

        // private constructor: the class cannot be instantiated outside of itself; therefore, this is the only instance that can exist in the system
        private TaskAssigner()
        {
            InitTaskAssigner();
            DoomScroll._log.LogInfo("TASK ASSIGNER CONSTRUCTOR");
        }

        private void InitTaskAssigner()
        {
            MaxAssignableTasks = 2;
            cardSprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.card.png");
            AssignableTasks = new List<uint>();
            AssignedTasks = new List<AssignedTask>();
            PlayerButtons = new Dictionary<byte, CustomButton>();
            isAssignerPanelActive = false;

            // set up stuff for folder display, paging through. Set it false for now because not necessary yet.
            CustomModal parent = FolderManager.Instance.GetFolderArea();
            numPages = 1;
            currentPage = 1;
            m_nextBtn = Page.AddRightButton(parent.UIGameObject, true);
            DoomScroll._log.LogInfo("Task page right button added");
            m_backBtn = Page.AddLeftButton(parent.UIGameObject, true);
            m_nextBtn.SetScale(new Vector3(-1, 1, 1));
            DoomScroll._log.LogInfo("Task page left button added");
            m_nextBtn.ButtonEvent.MyAction += OnClickRightButton;
            m_backBtn.ButtonEvent.MyAction += OnClickLeftButton;
            DoomScroll._log.LogInfo("Task page button events added");
            m_nextBtn.ActivateCustomUI(false);
            m_backBtn.ActivateCustomUI(false);
            DoomScroll._log.LogInfo("Task page buttons deactivated");
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
            messageWriter.Write(PlayerControl.LocalPlayer.PlayerId); // assigner
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
                    AssignedTasks.Add(assTask);
                    NotificationManager.ShowNotification(assTask.AssigneeName + " signed into a task.");
                    DoomScroll._log.LogInfo("New task assigned\n player: " + assTask.AssigneeName + ", task: " + assTask.Type);
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
            if (AssignedTasks.Count <= FileText.maxNumTextItems)
            {
                // to do: list it on a UI modal
                Vector3 pos = new Vector3(0, parent.GetSize().y / 2 - 0.8f, -10);
                foreach (AssignedTask task in AssignedTasks)
                {
                    task.DisplayTaskCard(parent, cardSprite);
                    pos.y -= task.Card.GetSize().y + 0.05f;
                    task.Card.SetLocalPosition(pos);
                    task.Card.ActivateCustomUI(true);
                }
            }
            else
            {
                // set allowed number of pages
                numPages = (int)Math.Ceiling((float)(AssignedTasks.Count) / FileText.maxNumTextItems);
                DoomScroll._log.LogInfo("Number of pages of tasks: " + numPages);

                // show buttons for flipping between pages
                m_nextBtn.ActivateCustomUI(true);
                m_backBtn.ActivateCustomUI(true);
                DoomScroll._log.LogInfo("Task page buttons activated");

                // to do: list it on a UI modal
                // always show page 1 first
                DisplayAssignedTasks(1);
            }
            DoomScroll._log.LogInfo("TASKS ASSIGNED SO FAR:\n " + ToString()); // debug
        }
        public void DisplayAssignedTasks(int displayPageNum)
        {
            DoomScroll._log.LogInfo($"Displaying page {displayPageNum} of tasks.");
            //this case probably won't happen; checking in meantime for first few tests
            if (displayPageNum < 1 || displayPageNum > numPages)
            {
                DisplayAssignedTasks(1);
            }

            // to do: list it on a UI modal
            int currentTaskIndex = (displayPageNum-1)*FileText.maxNumTextItems;
            CustomModal parent = FolderManager.Instance.GetFolderArea();
            Vector3 pos = new Vector3(0, parent.GetSize().y / 2 - 0.8f, -10);
            while (currentTaskIndex < AssignedTasks.Count && currentTaskIndex < displayPageNum * FileText.maxNumTextItems)
                // stops before index out of range and before printing tasks that should be on next page
            {
                AssignedTask task = AssignedTasks[currentTaskIndex];
                task.DisplayTaskCard(parent, cardSprite);
                pos.y -= task.Card.GetSize().y + 0.05f;
                task.Card.SetLocalPosition(pos);
                task.Card.ActivateCustomUI(true);
                currentTaskIndex++;
            }

            currentPage = displayPageNum;

            if (currentPage == 1)
            {
                m_backBtn.EnableButton(false);
            }
            else
            {

                m_backBtn.EnableButton(true);
            }
            if (currentPage == numPages)
            {
                m_nextBtn.EnableButton(false);
            }
            else
            {
                m_nextBtn.EnableButton(true);
            }

            DoomScroll._log.LogInfo("TASKS ASSIGNED SO FAR:\n " + ToString()); // debug
        }

        public void OnClickRightButton()
        {
            HideAssignedTasks();
            DisplayAssignedTasks(currentPage + 1);
        }

        public void OnClickLeftButton()
        {
            HideAssignedTasks();
            DisplayAssignedTasks(currentPage - 1);
        }

        public void CheckForDisplayedTasksPageButtonClicks()
        {
            try
            {
                //hovers
                if (m_nextBtn != null)
                {
                    m_nextBtn.ReplaceImgageOnHover();
                }
                if (m_backBtn != null)
                {
                    m_backBtn.ReplaceImgageOnHover();
                }
                //clicks
                if (m_nextBtn != null && m_nextBtn.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    m_nextBtn.ButtonEvent.InvokeAction();
                }
                if (m_backBtn != null && m_backBtn.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    m_backBtn.ButtonEvent.InvokeAction();
                }
            }
            catch (Exception e)
            {
                DoomScroll._log.LogError("Error invoking overlay button method: " + e);
            }
        }
        public void HidePageButtons()
        {
            m_nextBtn.ActivateCustomUI(false);
            m_backBtn.ActivateCustomUI(false);
            DoomScroll._log.LogInfo("Task page buttons deactivated");
        }

        public void HideAssignedTasks()
        {
            int currentTaskIndex = (currentPage - 1) * FileText.maxNumTextItems;
            while (currentTaskIndex < AssignedTasks.Count && currentTaskIndex < currentPage * FileText.maxNumTextItems)
            {
                AssignedTasks[currentTaskIndex].Card.ActivateCustomUI(false);
                currentTaskIndex++;
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
            PlayerButtons = new Dictionary<byte, CustomButton>();
            // Sprites: panel, button background, button icon
            panelSprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            butttonSprite = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.emptyBtn.png", slices);
            playerSprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.playerIcon.png");

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
            Vector3 topLeftPos = new Vector3(pos.x - size.x/2 + 0.5f, -0.1f, pos.z - 10);

            // add the players as buttons
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (!playerInfo.IsDead)
                {
                    CustomButton btn = new CustomButton(PlayerButtonHolder.UIGameObject, playerInfo.PlayerName, butttonSprite, topLeftPos, 0.45f);
                    CustomText label = new CustomText(btn.UIGameObject, playerInfo.PlayerName + "- label", playerInfo.PlayerName);
                    label.SetLocalPosition(new Vector3(0, -btn.GetSize().x/2 - 0.05f, -10));
                    label.SetSize(1.2f);
                    SpriteRenderer sr = btn.AddIconToButton(playerSprite, 0.7f);
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
