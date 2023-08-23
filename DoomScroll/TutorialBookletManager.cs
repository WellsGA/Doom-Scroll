﻿using System;
using UnityEngine;
using Doom_Scroll.UI;
using Doom_Scroll.Common;
using MS.Internal.Xml.XPath;
using System.Collections.Generic;

namespace Doom_Scroll
{
    // Manager class of the Tutorial Booklet system 
    // basic singleton pattern - not thread safe
    public sealed class TutorialBookletManager
    {
        //buttons
        private CustomButton m_tutorialBookletToggleBtn;
        private CustomButton m_closeBtn;
        private CustomButton m_nextBtn;
        private CustomButton m_backBtn;

        //modal
        private bool m_isTutorialBookletOverlayOpen;
        private CustomModal m_tutorialBookletArea;

        // pages
        private Page m_titlePage;
        private Page m_headlines;
        private Page m_signInForms;
        //private Page m_screenshots;
        private Page m_infiniteChatLogs;
        private Page m_swcs;
        private Page m_folderSystem;

        private List<Page> m_pageOrder;

        private int m_currentIndex;

        // text for different pages
        private String m_titlePageText = "Click through the following pages to learn more about DoomScroll and its\n misinformation features!";
        private String m_headlinesText = "Headlines can provide useful clues, but are they always trustworthy? When\n the NEWS button is yellow, click to choose a headline that will\n help your fellow crewmates (or trick them).\n";
        private String m_signInFormsText = "Sign in to your tasks! You’ll be prompted to sign in to random tasks.\n Click on a player to confirm who completed it. This can create\n evidence that you (or others) are a contributing crewmate.\n";
        //private String m_screenshotsText = "These are screenshots! They don't work right now.";
        private String m_infiniteChatLogsText = "Chat logs from earlier meetings remain available throughout the match.\n Look back at old messages to see if everyone’s arguments stay consistent.\r\n";
        private String m_swcText = "SWCs are secondary objectives for you to complete along with your role as \ncrewmate or imposter. To win, you must do your duties as crewmate or\n imposter AND either PROTECT or FRAME your randomly assigned target player.\r\n* Protect - Keep the player alive       \r\n* Frame - Get the player eliminated\r\n\r\n";
        private String m_folderSystemText = "Headline Reports and the Sign-In Sheet are saved in the Folder System.\n During meetings, take a moment to investigate the evidence and see how\n it can help support your argument (or how it refutes someone else’s).\n";

        private LobbyBehaviour lobbyBehaviourInstance;

        private static TutorialBookletManager _instance;
        public static TutorialBookletManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TutorialBookletManager();
                }
                return _instance;
            }
        }

        private TutorialBookletManager()
        {
            DoomScroll._log.LogInfo("Initializing tutorial booklet manager instance.");
            lobbyBehaviourInstance = LobbyBehaviour.Instance;
            InitializeTutorialBookletManager();
            ActivateTutorialBookletOverlay(false);
            DoomScroll._log.LogInfo("TUTORIAL BOOKLET MANAGER CONSTRUCTOR");
        }

        public void CheckForButtonClicks()
        {
            //DoomScroll._log.LogInfo("Checking for tutorial booklet manager button clicks.");
            if (lobbyBehaviourInstance == null) return;
            // Change buttons icon on hover
            if (m_tutorialBookletToggleBtn != null)
            {
                m_tutorialBookletToggleBtn.ReplaceImgageOnHover();
            }

            // If the TutorialBooklet toggle button is active invoke toggle on mouse click 
            if (m_tutorialBookletToggleBtn != null)
            {
                try
                {
                    if (m_tutorialBookletToggleBtn.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        m_tutorialBookletToggleBtn.ButtonEvent.InvokeAction();
                    }
                }
                catch (Exception e)
                {
                    DoomScroll._log.LogError("Error invoking tutorialBookletToggle: " + e);
                }
            }
            // If TutorialBooklet overlay is open invoke events on button clicks
            if (m_isTutorialBookletOverlayOpen)
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
                    if (m_closeBtn != null && m_closeBtn.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        m_tutorialBookletToggleBtn.ButtonEvent.InvokeAction();
                    }
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
        }

        private void InitializeTutorialBookletManager()
        {
            if (lobbyBehaviourInstance == null) return;
            CreateTutorialBookletOverlayUI();
            InitTutorialBookletStructure();
            m_tutorialBookletToggleBtn.ButtonEvent.MyAction += OnClickTutorialBookletBtn;
            m_nextBtn.ButtonEvent.MyAction += OnClickRightButton;
            m_backBtn.ButtonEvent.MyAction += OnClickLeftButton;
            m_tutorialBookletToggleBtn.EnableButton(true);
            m_tutorialBookletToggleBtn.ActivateCustomUI(true);
        }
        private void CreateTutorialBookletOverlayUI()
        {
            GameObject bottomCodeText = GameObject.Find("GameRoomButton");
            //GameObject chatScreen = lobbyBehaviourInstance.Chat.OpenKeyboardButton.transform.parent.gameObject;
            m_isTutorialBookletOverlayOpen = false;
            m_tutorialBookletToggleBtn = TutorialBookletOverlay.CreateTutorialBookletBtn(bottomCodeText);
            m_tutorialBookletArea = TutorialBookletOverlay.CreateTutorialBookletOverlay(bottomCodeText);
            m_closeBtn = TutorialBookletOverlay.AddCloseButton(m_tutorialBookletArea.UIGameObject);
            m_nextBtn = Page.AddRightButton(m_tutorialBookletArea.UIGameObject, false);
            m_backBtn = Page.AddLeftButton(m_tutorialBookletArea.UIGameObject, false);
            m_nextBtn.SetScale(new Vector3(-1, 1, 1));
        }
        private void InitTutorialBookletStructure()
        {
            m_titlePage = new Page("DoomScroll Tutorial Booklet", "Doom_Scroll.Assets.MainMenu_Button_Basic.png", m_titlePageText, m_tutorialBookletArea);
            m_headlines = new Page("Headlines", "Doom_Scroll.Assets.newsSelect.png", m_headlinesText, m_tutorialBookletArea, "Doom_Scroll.Assets.newsFolder.png", new Tuple<string, float>("Doom_Scroll.Assets.newsButtonExample.png", 1f));
            m_signInForms = new Page("Sign-In Forms", "Doom_Scroll.Assets.taskSelect.png", m_signInFormsText, m_tutorialBookletArea, "Doom_Scroll.Assets.taskFolder.png", new Tuple<string, float>("Doom_Scroll.Assets.taskButtonsExample.png", 0.3f));
            m_infiniteChatLogs = new Page("Infinite Chat Logs", "Doom_Scroll.Assets.infiniteChatAfter.png", m_infiniteChatLogsText, m_tutorialBookletArea, "Doom_Scroll.Assets.infiniteChatBefore.png");
            m_swcs = new Page("SWCs", "Doom_Scroll.Assets.SWCInTaskList_WithArrow.png", m_swcText, m_tutorialBookletArea, "Doom_Scroll.Assets.SWCStartScreen.png");
            //m_screenshots = new Page("Images", "Doom_Scroll.Assets.file.png", m_screenshotsText, m_tutorialBookletArea);
            m_folderSystem = new Page("Folder System", "Doom_Scroll.Assets.folderWhereInChat_WithArrow.png", m_folderSystemText, m_tutorialBookletArea, "Doom_Scroll.Assets.folderOpen.png", new Tuple<string, float>("Doom_Scroll.Assets.folderButtonExample.png", 0.75f));

            m_pageOrder = new List<Page>();
            m_pageOrder.Add(m_titlePage);
            m_pageOrder.Add(m_headlines);
            m_pageOrder.Add(m_signInForms);
            m_pageOrder.Add(m_infiniteChatLogs);
            m_pageOrder.Add(m_swcs);
            m_pageOrder.Add(m_folderSystem);

            m_currentIndex = 0;
        }

        private void ToggleTutorialBookletOverlay()
        {
            if (m_isTutorialBookletOverlayOpen)
            {
                ActivateTutorialBookletOverlay(false);
                m_pageOrder[m_currentIndex].HidePage();
                DoomScroll._log.LogInfo("CURRENT PAGE CLOSED");
            }
            else
            {
                ActivateTutorialBookletOverlay(true);
                DoomScroll._log.LogInfo("ROOT PAGE OPEN");
                // (re)sets root as the current and m_previous folder and displays its content
                m_currentIndex = 0;
                m_pageOrder[m_currentIndex].DisplayPage();
            }
        }

        private void ActivateTutorialBookletOverlay(bool value)
        {
            m_isTutorialBookletOverlayOpen = value;
            m_tutorialBookletArea.ActivateCustomUI(value);
        }

        private void FlipPage(int leftOrRight) //negative means left, positive means left
        {
            if (m_tutorialBookletToggleBtn.IsActive && m_isTutorialBookletOverlayOpen)
            {
                if (leftOrRight < 0)
                {
                    if (m_currentIndex > 0)
                    {
                        m_pageOrder[m_currentIndex].HidePage();
                        m_currentIndex--;
                        m_pageOrder[m_currentIndex].DisplayPage();
                    }
                }
                else if (leftOrRight > 0)
                {
                    if (m_currentIndex < m_pageOrder.Count-1)
                    {
                        m_pageOrder[m_currentIndex].HidePage();
                        m_currentIndex++;
                        m_pageOrder[m_currentIndex].DisplayPage();
                    }
                }
            }
        }
        public void OnClickTutorialBookletBtn()
        {
            if (m_tutorialBookletToggleBtn.IsActive)
            {
                ToggleTutorialBookletOverlay();
            }
        }

        public void OnClickRightButton()
        {
            FlipPage(1);
        }

        public void OnClickLeftButton()
        {
            FlipPage(-1);
        }

        public void CloseTutorialBookletOverlay()
        {
            if (m_isTutorialBookletOverlayOpen)
            {
                ActivateTutorialBookletOverlay(false);
                m_pageOrder[m_currentIndex].HidePage();
                DoomScroll._log.LogInfo("TUTORIAL BOOKLET OVERLAY CLOSED");
            }
        }

        public CustomModal GetTutorialBookletArea()
        {
            return m_tutorialBookletArea;
        }
        public void Reset()
        {
            if (lobbyBehaviourInstance == null)
            {
                lobbyBehaviourInstance = LobbyBehaviour.Instance;
                _instance = new TutorialBookletManager();
            }
            DoomScroll._log.LogInfo("TUTORIAL BOOKLET MANAGER RESET");
        }
    }
}
