using Doom_Scroll.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Doom_Scroll
{  
    // Singleton with static initialization: thread safe without explicitly coding for it,
    // relies on the common language runtime to initialize the variable
    public class NewsFeedManager
    {
        private static readonly NewsFeedManager _instance = new NewsFeedManager(); // readonly: can be assigned only during static initialization
        public static NewsFeedManager Instance
        {
            get
            {
                return _instance;
            }
        }

        private HudManager hudManagerInstance;

        private CustomModal m_inputPanel;
        private CustomInputField m_headline;
        private CustomInputField m_content;
        private CustomButton m_togglePanelButton;
        private CustomButton m_submitButton;
        public bool IsInputpanelOpen { get; private set; }
        private bool canCreateNews;
        
        // list of news created randomly and by the selected players -  will be displayed during meetings
        private static List<AssignedTask> AssignedTasks;

        private NewsFeedManager()
        {
            IsInputpanelOpen = false;
            canCreateNews = false;
            hudManagerInstance = HudManager.Instance;
            InitializeInputPanel();
            DoomScroll._log.LogInfo("NEWS FEED MANAGER CONSTRUCTOR");
        }

        private void InitializeInputPanel()
        {
            m_togglePanelButton = NewsFeedOverlay.CreateNewsInputButton(hudManagerInstance);
            m_inputPanel = NewsFeedOverlay.InitInputOverlay(hudManagerInstance);

            m_submitButton = NewsFeedOverlay.CreateSubmitButton(m_inputPanel.UIGameObject);
            m_headline = NewsFeedOverlay.AddInputField(m_inputPanel);
            m_togglePanelButton.ButtonEvent.MyAction += OnClickNews;
            m_submitButton.ButtonEvent.MyAction += OnClickSubmitNews;
            ActivateNewsButton(false);
        }

        public void OnClickNews()
        {
            ToggleNewsForm();
        }

        public void ToggleNewsForm()
        {
            if (!m_inputPanel.UIGameObject) { return; }
            if (IsInputpanelOpen)
            {
                m_inputPanel.ActivateCustomUI(false);
                m_submitButton.EnableButton(false);
                IsInputpanelOpen = false;
            }
            else
            {
                if (ScreenshotManager.Instance.IsCameraOpen) { ScreenshotManager.Instance.ToggleCamera(); } // close camera if oopen
                m_inputPanel.ActivateCustomUI(true);
                m_submitButton.EnableButton(true);
                IsInputpanelOpen = true;
            }
        }

        public void OnClickSubmitNews()
        {  
            DoomScroll._log.LogInfo("NEWS FORM SUBMITTED");
            ToggleNewsForm();
        }
        public void ActivateNewsButton(bool value)
        {
            m_togglePanelButton.UIGameObject.SetActive(value);
        }

        public void CanCreateNews(bool value)
        {
            canCreateNews = value;
            m_togglePanelButton.EnableButton(canCreateNews);
        }

        public void CheckButtonClicks()
        {
            if (hudManagerInstance == null || !canCreateNews) return;
            
            // Replace sprite on mouse hover for both buttons
            m_togglePanelButton.ReplaceImgageOnHover();
            m_submitButton.ReplaceImgageOnHover();

            try
            {
                // Invoke methods on mouse click - open news form overlay
                if (m_togglePanelButton.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    m_togglePanelButton.ButtonEvent.InvokeAction();
                }
                // Invoke methods on mouse click - submit news
                if (m_submitButton.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    m_submitButton.ButtonEvent.InvokeAction();
                }
            }
            catch (Exception e)
            {
                DoomScroll._log.LogError("Error invoking method: " + e);
            }
        }

        public void Reset()
        {
            IsInputpanelOpen = false;
            canCreateNews = false;
            if (hudManagerInstance == null)
            {
                hudManagerInstance = HudManager.Instance;
                InitializeInputPanel();
            }
            DoomScroll._log.LogInfo("NEWS MANAGER RESET");
        }
    }
}
