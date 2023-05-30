using System;
using UnityEngine;
using Doom_Scroll.UI;
using Doom_Scroll.Common;
using MS.Internal.Xml.XPath;

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

        //modal
        private bool m_isFolderOverlayOpen;
        private CustomModal m_folderArea;
        private CustomText m_pathText;
        
        // folders
        private Folder m_root;
        private IDirectory m_previous;
        private IDirectory m_current;
        private FileTask m_tasks;
        private Folder m_screenshots;

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
            if (hudManagerInstance.Chat.IsOpen && !m_folderToggleBtn.IsActive)
            {
                m_folderToggleBtn.ActivateCustomUI(true);
            }
            else if (!hudManagerInstance.Chat.IsOpen && m_folderToggleBtn.IsActive)
            {
                m_folderToggleBtn.ActivateCustomUI(false);
                // hide overlay and current folders too if it was still open
                if (m_isFolderOverlayOpen)
                {
                    m_folderToggleBtn.ButtonEvent.InvokeAction();
                }
            }

            // If chat is open and the foder toggle button is active invoke toggle on mouse click 
            if (hudManagerInstance.Chat.IsOpen && m_folderToggleBtn.IsActive)
            {
                try
                {
                    if (m_folderToggleBtn.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
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
            if (hudManagerInstance.Chat.IsOpen && m_isFolderOverlayOpen)
            {
                try
                {
                    if (m_closeBtn.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        m_folderToggleBtn.ButtonEvent.InvokeAction();
                    }
                    if (m_homeBtn.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        m_homeBtn.ButtonEvent.InvokeAction();
                    }
                    if (m_backBtn.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        m_backBtn.ButtonEvent.InvokeAction();
                    }
                    // if current is folder, check for clicks on any of its contents
                    if(m_current is Folder currFolder)
                    {
                        foreach (IDirectory dir in currFolder.Content)
                        {
                            dir.Btn.ReplaceImgageOnHover();
                            if (dir.Btn.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                            {
                                ChangeDirectory(dir);
                            }
                        }
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
            m_folderToggleBtn.ButtonEvent.MyAction += OnClickFolderBtn;
            m_homeBtn.ButtonEvent.MyAction += OnClickHomeButton;
            m_backBtn.ButtonEvent.MyAction += OnClickBackButton;
        }
        private void CreateFolderOverlayUI()
        {
            GameObject chatScreen = hudManagerInstance.Chat.OpenKeyboardButton.transform.parent.gameObject;
            m_isFolderOverlayOpen = false;
            m_folderToggleBtn = FolderOverlay.CreateFolderBtn(chatScreen);
            m_folderArea = FolderOverlay.CreateFolderOverlay(chatScreen);
            m_closeBtn = FolderOverlay.AddCloseButton(m_folderArea.UIGameObject);
            m_homeBtn = FolderOverlay.AddHomeButton(m_folderArea.UIGameObject);
            m_backBtn = FolderOverlay.AddBackButton(m_folderArea.UIGameObject);
            m_pathText = FolderOverlay.AddPath(m_folderArea.UIGameObject);
        }
        private void InitFolderStructure()
        {
            m_root = new Folder("", "Home", m_folderArea.UIGameObject);
            m_screenshots = new Folder(m_root.Path, "Images", m_folderArea.UIGameObject);
            
            m_tasks = new FileTask(m_root.Path, "Tasks", m_folderArea.UIGameObject);
            m_root.AddItem(m_screenshots);
            m_root.AddItem(m_tasks);
            m_root.AddItem(new Folder(m_root.Path, "Checkpoints", m_folderArea.UIGameObject));

            m_current = m_root;
            m_previous = m_root;
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

        public void AddImageToScreenshots(string name, byte[] img)
        {
            m_screenshots.AddItem(new FileScreenshot(m_screenshots.Path, name, m_folderArea.UIGameObject, img));
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
