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
        public SourceDisplay Sources { get; private set; }
        //modal
        private CustomImage m_folderArea;

        // folders
        private IDirectory m_previous;
        private IDirectory m_current;
        private FileText m_tasks;
        private FileText m_posts;
        private FileText m_sources;
        private Folder m_screenshots;
        private List<IDirectory> m_tabs;

        // tooltips
        // private Tooltip m_chatWindowTooltip;
        /*private Tooltip m_tasksTooltip;
        private Tooltip m_postsTooltip;
        private Tooltip m_screenshotsTooltip;
        private Tooltip m_sourcesTooltip;
*/
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
            InitializeFolderManager();
            DoomScroll._log.LogInfo("FOLDER MANAGER CONSTRUCTOR");
        }

        public void CheckForButtonClicks()
        {
            if (hudManagerInstance == null) return;
            if (hudManagerInstance.Chat.IsOpenOrOpening && !m_folderArea.UIGameObject.active)
            {
                Instance.ActivateEvidenceFolder(true);
            }
            else if(hudManagerInstance.Chat.IsClosedOrClosing && m_folderArea.UIGameObject.active)
            {
                Instance.ActivateEvidenceFolder(false);
            }
                // If chat and folder overlay are open invoke events on button clicks
            if (m_folderArea.UIGameObject.active)
            {
                try
                {
                    foreach(IDirectory dir in m_tabs)
                    {
                        dir.Btn.ReplaceImgageOnHover();
                        if (dir.Btn.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                        {
                            ChangeDirectory(dir);
                        }
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
                    if (Sources.AreSourcesDisplayed)
                    {
                        Sources.CheckForDisplayedSourcesPageButtonClicks();
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
            hudManagerInstance = HudManager.Instance;
            if (hudManagerInstance == null) return;

            GameObject chatScreen = hudManagerInstance.Chat.gameObject;
            m_folderArea = FolderOverlay.CreateFolderOverlay(chatScreen);
            Sources = new SourceDisplay(m_folderArea);
            
            m_screenshots = FolderOverlay.AddScreenshotsBtn(m_folderArea);
            m_tasks = FolderOverlay.AddTasksBtn(m_folderArea);
            m_posts = FolderOverlay.AddPostsBtn(m_folderArea);
            m_sources = FolderOverlay.AddSourcesBtn(m_folderArea);

            m_tabs = new List<IDirectory>() { m_posts, m_tasks, m_screenshots, m_sources };

            m_current = m_posts;
            m_previous = m_posts;

        }

        private void ChangeDirectory(IDirectory folder)
        {
            if (m_folderArea.UIGameObject.active)
            {
                m_previous = m_current;
                m_previous.Btn.SelectButton(false);
                m_previous.HideContent();
                m_current = folder;
                m_current.Btn.SelectButton(true);
                m_current.Btn.ButtonEvent.InvokeAction();
                DoomScroll._log.LogInfo(" =================== OPEN TAB " + m_current.Path + "==========================");
            }
        }
      

        public void ActivateEvidenceFolder(bool isActive) { 
            m_folderArea.ActivateCustomUI(isActive);
            if(isActive)
            {
                ChangeDirectory(m_posts);
            }
            else
            {
                CloseEvidenceFolder();
            }
            
        }

        private void CloseEvidenceFolder()
        {
            ChangeDirectory(m_posts);
            m_current.HideContent();
        }

        public void AddImageToScreenshots(int id, byte[] img)
        {
            FileScreenshot file = new FileScreenshot(m_screenshots.Path, "Uploading ..." , m_folderArea, img, id);
            m_screenshots.AddItem(file);
        }

        public CustomImage GetFolderArea()
        {
            return m_folderArea;
        }
        public bool IsFolderOpen()
        {
            return m_folderArea.UIGameObject.active ;
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
                InitializeFolderManager();
            }
            DoomScroll._log.LogInfo("FOLDER MANAGER RESET");
        }
    }
}
