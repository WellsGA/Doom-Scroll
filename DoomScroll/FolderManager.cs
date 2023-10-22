using System;
using UnityEngine;
using Doom_Scroll.UI;
using Doom_Scroll.Common;

namespace Doom_Scroll
{
    // Manager class of the folder system 
    // basic singleton pattern - not thread safe
    public sealed class FolderManager
    {
        //buttons
        private CustomButton m_folderToggleBtn;
        private CustomButton m_closeBtn;
        private CustomButton m_homeBtn;
        private CustomButton m_backBtn;

        //tooltips
        private Tooltip m_folderToggleTooltip;
        private Tooltip m_chatWindowTooltip;

        //modal
        private bool m_isFolderOverlayOpen;
        private CustomModal m_folderArea;
        private CustomText m_pathText;
        
        // folders
        private Folder m_root;
        private IDirectory m_previous;
        private IDirectory m_current;
        private FileText m_tasks;
        private FileText m_posts;
        private Folder m_screenshots;

        // tooltips
        private Tooltip m_tasksTooltip;
        private Tooltip m_postsTooltip;
        private Tooltip m_screenshotsTooltip;
        private Tooltip m_taskFolderTooltip;
        private Tooltip m_postFolderTooltip;
        private Tooltip m_postFolderVotingTooltip;
        private Tooltip m_screenshotFolderTooltip;

        // image sending
        private ImageSender m_imageSender;

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
            ActivateFolderOverlay(false);
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
                m_folderToggleBtn.ActivateCustomUI(false);
                // hide overlay and current folders too if it was still open
                if (m_isFolderOverlayOpen)
                {
                    m_folderToggleBtn.ButtonEvent.InvokeAction();
                }
            }

            // If chat is open and the foder toggle button is active invoke toggle on mouse click 
            if (hudManagerInstance.Chat.IsOpenOrOpening && m_folderToggleBtn.IsActive)
            {
                try
                {
                    if (m_folderToggleBtn.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        m_folderToggleBtn.ButtonEvent.InvokeAction();
                    }
                }
                catch (Exception e)
                {
                    DoomScroll._log.LogError("Error invoking folderToggle: " + e);
                }
            }
            // If chat and folder overlay are open invoke events on button clicks
            if (hudManagerInstance.Chat.State == ChatControllerState.Open && m_isFolderOverlayOpen)
            {
                try
                {
                    if (m_closeBtn.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        m_folderToggleBtn.ButtonEvent.InvokeAction();
                    }
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
                    if (m_current is FileText && NewsFeedManager.Instance != null) // Trying to see if currently displaying tasks??
                    {
                        NewsFeedManager.Instance.CheckForDisplayedNewsPageButtonClicks();
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
            CreateImageSender();
            m_folderToggleBtn.ButtonEvent.MyAction += OnClickFolderBtn;
            m_homeBtn.ButtonEvent.MyAction += OnClickHomeButton;
            m_backBtn.ButtonEvent.MyAction += OnClickBackButton;
        }
        private void CreateFolderOverlayUI()
        {
            GameObject chatScreen = hudManagerInstance.Chat.gameObject;
            if(chatScreen != null) { DoomScroll._log.LogInfo("Scroller ???? " + chatScreen.name); }
            m_isFolderOverlayOpen = false;
            m_folderToggleBtn = FolderOverlay.CreateFolderBtn(chatScreen);
            m_folderToggleTooltip = new Tooltip(m_folderToggleBtn.UIGameObject, "FolderToggleBtn", "Click here to\ninvestigate the\nevidence.", 0.5f, new Vector3(0, -0.5f, 0), 1f);
            m_chatWindowTooltip = new Tooltip(m_folderToggleBtn.UIGameObject, "ChatWindow", "Use :) or :( to like or dislike chat posts.", 0.25f, new Vector3(-4f, -3.1f, 0), 1.5f);

            m_folderArea = FolderOverlay.CreateFolderOverlay(chatScreen);
            m_closeBtn = FolderOverlay.AddCloseButton(m_folderArea);
            m_homeBtn = FolderOverlay.AddHomeButton(m_folderArea);
            m_backBtn = FolderOverlay.AddBackButton(m_folderArea);
            m_pathText = FolderOverlay.AddPath(m_folderArea.UIGameObject);

            // Tooltip in-folder setup
            m_screenshotFolderTooltip = new Tooltip(m_pathText.UIGameObject, "InsideScreenshotsFolder", "Notice any clues? How can you use these\nphotos in your arguments?", 0.5f, new Vector3(0, -0.3f, 0), 1.5f);
            m_taskFolderTooltip = new Tooltip(m_pathText.UIGameObject, "InsideTasksFolder", "Who is doing their tasks? Who isn't?", 0.5f, new Vector3(0, -0.25f, 0), 2f);
            m_postFolderTooltip = new Tooltip(m_pathText.UIGameObject, "InsidePostsFolder", "True posts can give vital clues about other players,\nbut some posts might be misleading!", 0.5f, new Vector3(0, -0.3f, 0), 1.5f);
            m_postFolderVotingTooltip = new Tooltip(m_pathText.UIGameObject, "InsidePostsVoting", "Vote whether you think a post is trustworthy or not.\nFinal accuracy scores will be revealed after the match.", 0.5f, new Vector3(0, -4.0f, 0), 1.5f);
            m_screenshotFolderTooltip.ActivateToolTip(false);
            m_taskFolderTooltip.ActivateToolTip(false);
            m_postFolderTooltip.ActivateToolTip(false);
            m_postFolderVotingTooltip.ActivateToolTip(false);


        }
        private void InitFolderStructure()
        {
            m_root = new Folder("", "Home", m_folderArea);
            m_screenshots = new Folder(m_root.Path, "Images", m_folderArea);
            m_screenshotsTooltip = new Tooltip(m_screenshots.Dir, "ScreenshotsFolderBtn", "Your photos will\nshow up here.", 0.5f, new Vector3(0, -1f, 0), 2f);
            m_tasks = new FileText(m_root.Path, "Tasks", m_folderArea, FileTextType.TASKS);
            m_screenshotsTooltip = new Tooltip(m_tasks.Dir, "TasksFolderBtn", "Your task sign-ins will\nshow up here.", 0.5f, new Vector3(0, -1f, 0), 2f);
            m_posts = new FileText(m_root.Path, "Posts", m_folderArea, FileTextType.NEWS);
            m_screenshotsTooltip = new Tooltip(m_posts.Dir, "PostsFolderBtn", "Your posts will\nshow up here.", 0.5f, new Vector3(0, -1f, 0), 2f);
            m_root.AddItem(m_screenshots);
            m_root.AddItem(m_tasks);
            m_root.AddItem(m_posts);

            m_current = m_root;
            m_previous = m_root;
        }
        private void CreateImageSender()
        {
            m_imageSender = new ImageSender();
            DoomScroll._log.LogInfo("m_imageSender created: " + m_imageSender.ToString());
        }

        private void ToggleFolderOverlay()
        {
            if (m_isFolderOverlayOpen)
            {
                ActivateFolderOverlay(false);
                m_current.HideContent();
                DoomScroll._log.LogInfo("CURRENT FOLDER CLOSED");
            }
            else
            {
                ActivateFolderOverlay(true);
                DoomScroll._log.LogInfo("ROOT FOLDER OPEN");
                // (re)sets root as the current and m_previous folder and displays its content
                m_previous = m_root;
                m_current = m_root;
                m_current.Btn.ButtonEvent.InvokeAction();
            }
        }
       
        private void ActivateFolderOverlay(bool value)
        {
            m_isFolderOverlayOpen = value;
            m_folderArea.ActivateCustomUI(value);
        }

        private void ChangeDirectory(IDirectory folder)
        {
            if (m_folderToggleBtn.IsActive && m_isFolderOverlayOpen)
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

                    // Tooltip in-folder toggling
                    if (m_current == m_screenshots)
                    {
                        m_screenshotFolderTooltip.ActivateToolTip(true);
                        m_taskFolderTooltip.ActivateToolTip(false);
                        m_postFolderTooltip.ActivateToolTip(false);
                        m_postFolderVotingTooltip.ActivateToolTip(false);
                    }
                    else if (m_current == m_tasks)
                    {
                        m_screenshotFolderTooltip.ActivateToolTip(false);
                        m_taskFolderTooltip.ActivateToolTip(true);
                        m_postFolderTooltip.ActivateToolTip(false);
                        m_postFolderVotingTooltip.ActivateToolTip(false);
                    }
                    else if (m_current == m_posts)
                    {
                        m_screenshotFolderTooltip.ActivateToolTip(false);
                        m_taskFolderTooltip.ActivateToolTip(false);
                        m_postFolderTooltip.ActivateToolTip(true);
                        m_postFolderVotingTooltip.ActivateToolTip(true);
                    }
                    else
                    {
                        m_screenshotFolderTooltip.ActivateToolTip(false);
                        m_taskFolderTooltip.ActivateToolTip(false);
                        m_postFolderTooltip.ActivateToolTip(false);
                        m_postFolderVotingTooltip.ActivateToolTip(false);
                    }
                }
            }
        }
        public void OnClickFolderBtn()
        {
            if (m_folderToggleBtn.IsActive)
            {
                ToggleFolderOverlay();
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
            if (m_isFolderOverlayOpen)
            {
                ActivateFolderOverlay(false);
                m_current.HideContent();
                DoomScroll._log.LogInfo("MEETING OVER, FOLDER CLOSED");
            }
        }

        public void AddImageToScreenshots(string name, byte[] img)
        {
            FileScreenshot file = new FileScreenshot(m_screenshots.Path, name, m_folderArea, img);
            m_screenshots.AddItem(file);
            if (m_imageSender != null)
            {
                m_imageSender.SendCurrentImageMethod(file, img);
            }
            else
            {
                DoomScroll._log.LogInfo("Could not send image in background. ImageSender was null.");
            }
        }

        public CustomModal GetFolderArea()
        {
            return m_folderArea;
        }
        public bool IsFolderOpen()
        {
            return m_isFolderOverlayOpen;
        }
        public void Reset()
        {
            if (hudManagerInstance == null)
            {
                hudManagerInstance = HudManager.Instance;
                InitializeFolderManager();
                ActivateFolderOverlay(false);
            }
            DoomScroll._log.LogInfo("FOLDER MANAGER RESET");
        }
    }
}
