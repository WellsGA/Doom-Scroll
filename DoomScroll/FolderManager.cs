using System;
using UnityEngine;
using Doom_Scroll.UI;
using Doom_Scroll.Common;
using System.Collections.Generic;

namespace Doom_Scroll
{
    // Manager class of the folder system 
    // basic singleton pattern - not thread safe
    public sealed class FolderManager
    {

        //modal
        private CustomModal m_folderArea;
        private CustomText m_pathText;
        //buttons on modal
        private CustomButton m_folderToggleBtn;
        private CustomButton m_homeBtn;
        private CustomButton m_backBtn;

        // folders
        private Folder m_root;
        private IDirectory m_previous;
        private IDirectory m_current;
        private FileText m_tasks;
        private FileText m_posts;
        private Folder m_screenshots;

        // tooltips
        private Tooltip m_folderToggleTooltip;
        private Tooltip m_chatWindowTooltip;
        private Tooltip m_tasksTooltip;
        private Tooltip m_postsTooltip;
        private Tooltip m_screenshotsTooltip;
        private Tooltip m_taskFolderTooltip;
        private Tooltip m_postFolderTooltip;
        private Tooltip m_postFolderVotingTooltip;
        private Tooltip m_screenshotFolderTooltip;
        private Tooltip m_HeadlinesTooltip;

        private HudManager hudManagerInstance;

        private static FolderManager _instance;
        public static FolderManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FolderManager();
                }
                return _instance;
            }
        }

        private FolderManager()
        {
            hudManagerInstance = HudManager.Instance;
            InitializeFolderManager();
            DoomScroll._log.LogInfo("FOLDER MANAGER CONSTRUCTOR");
        }

        public void CheckForButtonClicks()
        {
            if (hudManagerInstance == null) return;
            // Change buttons icon on hover
            m_folderToggleBtn.ReplaceImgageOnHover();
            m_homeBtn.ReplaceImgageOnHover();
            m_backBtn.ReplaceImgageOnHover();

            // Activate FolderToggle Button if Chat is open and hide if it's closed
            if (hudManagerInstance.Chat.IsOpenOrOpening && !m_folderToggleBtn.IsActive)
            {
                m_folderToggleBtn.ActivateCustomUI(true);
            }
            else if (hudManagerInstance.Chat.IsClosedOrClosing && m_folderToggleBtn.IsActive)
            {
                if (m_folderArea.IsModalOpen)
                {
                    CloseFolderOverlay();
                    DoomScroll._log.LogInfo("NEED TO CLOSE CURRENT FOLDER ");
                }
                m_folderToggleBtn.ActivateCustomUI(false);
            }

            // If chat is open and the foder toggle button is active invoke toggle on mouse click 
            if (hudManagerInstance.Chat.IsOpenOrOpening && m_folderToggleBtn.IsActive)
            {
                try
                {
                    m_folderArea.ListenForButtonClicks();
                }
                catch (Exception e)
                {
                    DoomScroll._log.LogError("Error invoking folderToggle: " + e);
                }
            }
            // If chat and folder overlay are open invoke events on button clicks
            if (hudManagerInstance.Chat.State == ChatControllerState.Open && m_folderArea.IsModalOpen)
            {
                try
                {
                    if (m_homeBtn.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        m_homeBtn.ButtonEvent.InvokeAction();
                    }
                    if (m_backBtn.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        m_backBtn.ButtonEvent.InvokeAction();
                    }
                    // if current is folder, check for clicks on any of its contents
                    if(m_current is Folder currFolder)
                    {
                        foreach (IDirectory dir in currFolder.Content)
                        {
                            dir.Btn.ReplaceImgageOnHover();
                            if (dir.Btn.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                            {
                                ChangeDirectory(dir);
                            }
                        }
                    }
                    // code for checking page turns in task folder
                    if (m_current is FileText && TaskAssigner.Instance != null) // Trying to see if currently displaying tasks??
                    {
                        TaskAssigner.Instance.CheckForDisplayedTasksPageButtonClicks();
                    }
                    if (m_current is FileText && HeadlineManager.Instance != null) // Trying to see if currently displaying tasks??
                    {
                        HeadlineDisplay.Instance.CheckForDisplayedNewsPageButtonClicks();
                    }
                }
                catch (Exception e)
                {
                    DoomScroll._log.LogError("Error invoking overlay button method: " + e);
                }
            }
        }

        private void InitializeFolderManager()
        {
            if (hudManagerInstance == null) return;
            CreateFolderOverlayUI();
            InitFolderStructure();
            m_homeBtn.ButtonEvent.MyAction += OnClickHomeButton;
            m_backBtn.ButtonEvent.MyAction += OnClickBackButton;
        }
        private void CreateFolderOverlayUI()
        {
            GameObject chatScreen = hudManagerInstance.Chat.gameObject;
            if(chatScreen != null) { DoomScroll._log.LogInfo("Scroller ???? " + chatScreen.name); }
            m_folderToggleBtn = FolderOverlay.CreateFolderBtn(chatScreen);
            m_folderToggleBtn.ButtonEvent.MyAction += ToggleFolders; // has to sign up before the custom modal is created!!!

            m_folderToggleTooltip = new Tooltip(m_folderToggleBtn.UIGameObject, "FolderToggleBtn", "Click here to\ninvestigate the\nevidence.", 0.5f, 1.6f, new Vector3(0, -0.5f, 0), 1f);
            m_chatWindowTooltip = new Tooltip(m_folderToggleBtn.UIGameObject, "ChatWindow", "Use :) or :( to like or dislike chat posts.", 0.25f, 11f, new Vector3(-4f, -3.1f, 0), 1.5f);

            m_folderArea = FolderOverlay.CreateFolderOverlay(chatScreen, m_folderToggleBtn);
            m_homeBtn = FolderOverlay.AddHomeButton(m_folderArea);
            m_backBtn = FolderOverlay.AddBackButton(m_folderArea);
            m_pathText = FolderOverlay.AddPath(m_folderArea.UIGameObject);
            m_folderArea.CloseButton.ButtonEvent.MyAction += CloseFolder;

            // Tooltip in-folder setup
            m_screenshotFolderTooltip = new Tooltip(m_pathText.UIGameObject, "InsideScreenshotsFolder", "Notice any clues? How can you use these\nphotos in your arguments?", 0.4f, 7.5f, new Vector3(0, -0.4f, 0), 1.5f);
            m_taskFolderTooltip = new Tooltip(m_pathText.UIGameObject, "InsideTasksFolder", "Who is doing their tasks? Who isn't? Think about this how this\ninformation might reflect on each player's reliability", 6f, 7f, new Vector3(0, -0.3f, 0), 1.5f);
            m_postFolderTooltip = new Tooltip(m_pathText.UIGameObject, "InsidePostsFolder", "True posts can give vital clues about other players, but some\nposts might be misleading! Can you tell the difference? How?", .65f, 6f, new Vector3(0, -0.3f, 0), 1.5f);
            m_postFolderVotingTooltip = new Tooltip(m_pathText.UIGameObject, "InsidePostsVoting", "Discuss whether you think each post is TRUSTWORTHY or not.\nALL Crewmates must achieve 100% accuracy to complete the final task.", 0.5f, 9.7f, new Vector3(0, -4.0f, 0), 1.5f);
            m_HeadlinesTooltip = new Tooltip(m_pathText.UIGameObject, "InsidePostsFolder", "FAKE NEWS:\r\nEmotional/polarizing\r\nHyperbolic\r\nPartisan/biased\r\nMany claims at once\r\nMisleading data\r\nConspiracy theories\r\nTrolling\r\nAttacks opponents\n\nBAD SOURCES:\r\nImpersonators\r\nMisleading domains\r\nUnreliable sponsors\n(blogs, forums)", 3f, 0.29f, new Vector3(-3.5f, -2.3f, 0), 1.4f);
            m_HeadlinesTooltip.ImageObject.SetSize(new Vector2(1.5f, 3f));
            m_taskFolderTooltip.ImageObject.SetSize(new Vector2(4.2f, .36f));
            m_postFolderTooltip.ImageObject.SetSize(new Vector2(4.4f, .36f));

            m_screenshotFolderTooltip.ActivateToolTip(false);
            m_taskFolderTooltip.ActivateToolTip(false);
            m_postFolderTooltip.ActivateToolTip(false);
            m_postFolderVotingTooltip.ActivateToolTip(false);

            m_HeadlinesTooltip.ActivateToolTip(false);


        }
        private void InitFolderStructure()
        {
            m_root = new Folder("", "Home", m_folderArea);
            m_screenshots = new Folder(m_root.Path, "Images", m_folderArea);
            m_screenshotsTooltip = new Tooltip(m_screenshots.Dir, "ScreenshotsFolderBtn", "Your photos will\nshow up here.", 0.5f, 3f, new Vector3(0, -1f, 0), 2f);
            m_tasks = new FileText(m_root.Path, "Tasks", m_folderArea, FileTextType.TASKS);
            m_tasksTooltip = new Tooltip(m_tasks.Dir, "TasksFolderBtn", "Your task sign-ins will\nshow up here.", 0.5f, 4f, new Vector3(0, -1f, 0), 2f);
            m_posts = new FileText(m_root.Path, "Headlines", m_folderArea, FileTextType.NEWS);
            m_postsTooltip = new Tooltip(m_posts.Dir, "PostsFolderBtn", "Your headlines will\nshow up here.", 0.5f, 3.5f, new Vector3(0, -1f, 0), 2f);
            m_root.AddItem(m_screenshots);
            m_root.AddItem(m_tasks);
            m_root.AddItem(m_posts);

            m_current = m_root;
            m_previous = m_root;

        }

        private void ToggleFolders()
        {
            if (!m_folderToggleBtn.IsActive) return;
            if (m_folderArea.IsModalOpen)
            {
                m_current.HideContent();
                DoomScroll._log.LogInfo("CURRENT FOLDER CLOSED");
            }
            else
            {
                DoomScroll._log.LogInfo("ROOT FOLDER OPEN");
                // (re)sets root as the current and m_previous folder and displays its content
                m_previous = m_root;
                m_current = m_root;
                m_current.Btn.ButtonEvent.InvokeAction();
                RectifyFolderTooltips();
            }
        }

        private void ChangeDirectory(IDirectory folder)
        {
            if (m_folderToggleBtn.IsActive && m_folderArea.IsModalOpen)
            { 
                if (folder is FileScreenshot) 
                {
                    folder.Btn.ButtonEvent.InvokeAction();
                }
                else
                {
                    m_previous = m_current;
                    m_current = folder;
                    m_pathText.SetText(folder.Path);
                    m_previous.HideContent();
                    m_current.Btn.ButtonEvent.InvokeAction();

                    RectifyFolderTooltips();
                }
            }
        }
        public static void RectifyFolderTooltips()
        {
            if (FolderManager.Instance != null)
            {
                FolderManager __instance = FolderManager.Instance;
                if (__instance.m_current != null && __instance.m_screenshotFolderTooltip != null && __instance.m_taskFolderTooltip != null && __instance.m_postFolderTooltip != null && __instance.m_postFolderVotingTooltip != null)
                {   
                    // Tooltip in-folder toggling
                    if (__instance.m_current == __instance.m_screenshots)
                    {
                        __instance.m_screenshotFolderTooltip.ActivateToolTip(true);
                        __instance.m_taskFolderTooltip.ActivateToolTip(false);
                        __instance.m_postFolderTooltip.ActivateToolTip(false);
                        __instance.m_postFolderVotingTooltip.ActivateToolTip(false);
                        __instance.m_HeadlinesTooltip.ActivateToolTip(false);
                    }
                    else if (__instance.m_current == __instance.m_tasks)
                    {
                        __instance.m_screenshotFolderTooltip.ActivateToolTip(false);
                        __instance.m_taskFolderTooltip.ActivateToolTip(true);
                        __instance.m_postFolderTooltip.ActivateToolTip(false);
                        __instance.m_postFolderVotingTooltip.ActivateToolTip(false);
                        __instance.m_HeadlinesTooltip.ActivateToolTip(false);
                    }
                    else if (__instance.m_current == __instance.m_posts)
                    {
                        __instance.m_screenshotFolderTooltip.ActivateToolTip(false);
                        __instance.m_taskFolderTooltip.ActivateToolTip(false);
                        __instance.m_postFolderTooltip.ActivateToolTip(true);
                        __instance.m_postFolderVotingTooltip.ActivateToolTip(true);
                        __instance.m_HeadlinesTooltip.ActivateToolTip(true);
                    }
                    else
                    {
                        __instance.m_screenshotFolderTooltip.ActivateToolTip(false);
                        __instance.m_taskFolderTooltip.ActivateToolTip(false);
                        __instance.m_postFolderTooltip.ActivateToolTip(false);
                        __instance.m_postFolderVotingTooltip.ActivateToolTip(false);
                        __instance.m_HeadlinesTooltip.ActivateToolTip(false);
                    }
                }
            }
        }

        public void OnClickHomeButton()
        {
            ChangeDirectory(m_root);
        }

        public void OnClickBackButton()
        {
            ChangeDirectory(m_previous);
        }

        public void CloseFolderOverlay()
        {
            m_folderArea.CloseButton.ButtonEvent.InvokeAction();         
        }

        public void CloseFolder()
        {
            ChangeDirectory(m_root);
            m_current.HideContent();
            DoomScroll._log.LogInfo("FOLDER CLOSED");
        }

        public void AddImageToScreenshots(int id, byte[] img)
        {
            FileScreenshot file = new FileScreenshot(m_screenshots.Path, "Uploading ..." , m_folderArea, img, id);
            m_screenshots.AddItem(file);
        }

        public CustomModal GetFolderArea()
        {
            return m_folderArea;
        }
        public bool IsFolderOpen()
        {
            return m_folderArea.IsModalOpen;
        }
        public List<FileScreenshot> GetScreenshots() 
        {
            List<FileScreenshot> screenshots = new List<FileScreenshot>();
            foreach(IDirectory file in m_screenshots.Content)
            {
                screenshots.Add((FileScreenshot)file);
            }
            return screenshots;
        }

        public void Reset()
        {
            if (hudManagerInstance == null)
            {
                hudManagerInstance = HudManager.Instance;
                InitializeFolderManager();
            }
            DoomScroll._log.LogInfo("FOLDER MANAGER RESET");
        }
    }
}
