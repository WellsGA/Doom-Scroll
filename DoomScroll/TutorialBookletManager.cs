using System;
using UnityEngine;
using Doom_Scroll.UI;
using Doom_Scroll.Common;
using System.Collections.Generic;

namespace Doom_Scroll
{
    // Manager class of the Tutorial Booklet system 
    // basic singleton pattern - not thread safe
    public sealed class TutorialBookletManager
    {
        private Pageable tutorialBookletPager;

        ////modal
        private CustomButton m_tutorialBookletToggleBtn;
        private CustomModal m_tutorialBookletArea;

        // pages
        private TutorialBookletPage m_titlePage;
        private TutorialBookletPage m_headlines;
        private TutorialBookletPage m_headlinesThree;
        private TutorialBookletPage m_headlinesTwo;
        private TutorialBookletPage m_signInForms;
        //private Page m_screenshots;
        private TutorialBookletPage m_infiniteChatLogs;
        private TutorialBookletPage m_swcs;
        private TutorialBookletPage m_folderSystem;

        private List<CustomUI> m_pageOrder;

        private int m_currentIndex;

        // text for different pages

        private string m_titlePageText = "Click through the following pages to learn more about DoomScroll and its\n misinformation features!";
        private string m_headlinesText = "Headlines can provide useful clues, but are they always trustworthy? When\n the NEWS button is yellow, click to choose a headline that will\n help your fellow crewmates (or trick them).\r\nTIP: Evaluate the evidence thoroughly to look for mistakes or inconsistencies.\r\n";
        private string m_headlinesTextThree = "* During meetings, use the like :) and dislike :( buttons to share your opinions\r\n on different posts. Then click the arrow to share to the chat.\r\n"; //* Click the T or F button to vote whether you think the post is TRUE\r\n or FALSE. Everyone’s scores will be revealed at the end of the game!\r\n";
        private string m_headlinesTextTwo = "Common signs of fake news:\r\n\r\n* Emotionally provocative/polarizing content          \r\n* Hyperbolic claims\r\n* Many claims at once          \r\n* Hyperpartisan bias\r\n* Misleading/biased statistics          \r\n* Conspiracy theories\r\n* Trolling          \r\n* Discredits or attacks opposing individuals/groups\r\n* Common signs of untrustworthy sources:          \r\n* Domains impersonating more reputable sources\r\n* Fake/Misleading domains          \r\n* Less trustworthy sponsors (e.g. blogs, forums)\r\n\r\n";
        private string m_signInFormsText = "Sign in to your tasks! You’ll be prompted to sign in to random tasks.\n Click on a player to confirm who completed it. This can create\n evidence that you (or others) are a contributing crewmate.\nYou can share sign-ins in the chat during meetings.\r\nTIP: Compare everyone’s arguments with the available\r\nevidence to help determine who you trust.";

        //private String m_screenshotsText = "These are screenshots! They don't work right now.";
        private string m_infiniteChatLogsText = "Chat logs from earlier meetings remain available throughout the match.\n Look back at old messages to see if everyone’s arguments stay consistent.\r\nTIP: What are everyone’s motivations? How is that influencing their arguments?";
        private string m_swcText = "SWCs are secondary objectives for you to complete along with your role as \ncrewmate or imposter. To win, you must do your duties as crewmate or\n imposter AND either PROTECT or FRAME your randomly assigned target player.\r\n* Protect - Keep the player alive       \r\n* Frame - Get the player eliminated\r\nTIP: How might information be being taken out of context?";
        private string m_folderSystemText = "Headline Reports and the Sign-In Sheet are saved in the Folder System.\n During meetings, take a moment to investigate the evidence and see how\n it can help support your argument (or how it refutes someone else’s).\r\nTIP: Evaluating the evidence first will strengthen your arguments.";

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
            DoomScroll._log.LogInfo("TUTORIAL BOOKLET MANAGER CONSTRUCTOR");
        }

        public void CheckForButtonClicks()
        {
            //DoomScroll._log.LogInfo("Checking for tutorial booklet manager button clicks.");
            if (lobbyBehaviourInstance == null || m_tutorialBookletToggleBtn == null) return;
            // Change buttons icon on hover
            m_tutorialBookletToggleBtn.ReplaceImgageOnHover();
            try
            {
                m_tutorialBookletArea.ListenForButtonClicks();
            }
            catch (Exception e)
            {
                DoomScroll._log.LogError("Error invoking tutorialBookletToggle: " + e);
            }
            // If TutorialBooklet overlay is open invoke events on button clicks
            if (m_tutorialBookletArea.IsModalOpen && tutorialBookletPager != null)
            {
                try
                {
                    tutorialBookletPager.CheckForDisplayedPageButtonClicks();
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
        }
        private void CreateTutorialBookletOverlayUI()
        {
            GameObject bottomCodeText = GameObject.Find("GameRoomButton") ? GameObject.Find("GameRoomButton") : lobbyBehaviourInstance.gameObject;
            //GameObject chatScreen = lobbyBehaviourInstance.Chat.OpenKeyboardButton.transform.parent.gameObject;
            m_tutorialBookletToggleBtn = TutorialBookletOverlay.CreateTutorialBookletBtn(bottomCodeText);
            m_tutorialBookletToggleBtn.ButtonEvent.MyAction += TogglePager;
            m_tutorialBookletArea = TutorialBookletOverlay.CreateTutorialBookletOverlay(bottomCodeText, m_tutorialBookletToggleBtn);
            m_tutorialBookletArea.CloseButton.ButtonEvent.MyAction += ClosePager;
            tutorialBookletPager = new Pageable(m_tutorialBookletArea, new List<CustomUI>(), 1, false);
        }
        private void InitTutorialBookletStructure()
        {
            m_titlePage = new TutorialBookletPage("DoomScroll Tutorial Booklet", m_titlePageText, m_tutorialBookletArea, "Doom_Scroll.Assets.MainMenu_Button_Basic.png");
            m_headlines = new TutorialBookletPage("Headlines", m_headlinesText, m_tutorialBookletArea, "Doom_Scroll.Assets.newsSelect.png", "Doom_Scroll.Assets.newsFolder.png", new Tuple<string, float>("Doom_Scroll.Assets.newsButtonExample.png", 1f));
            m_headlinesThree = new TutorialBookletPage("Headlines", m_headlinesTextThree, m_tutorialBookletArea);
            m_headlinesTwo = new TutorialBookletPage("Headlines", m_headlinesTextTwo, m_tutorialBookletArea);
            m_signInForms = new TutorialBookletPage("Sign-In Forms", m_signInFormsText, m_tutorialBookletArea, "Doom_Scroll.Assets.taskSelect.png", "Doom_Scroll.Assets.taskFolder.png", new Tuple<string, float>("Doom_Scroll.Assets.taskButtonsExample.png", 0.3f));
            m_infiniteChatLogs = new TutorialBookletPage("Infinite Chat Logs", m_infiniteChatLogsText, m_tutorialBookletArea, "Doom_Scroll.Assets.infiniteChatAfter.png", "Doom_Scroll.Assets.infiniteChatBefore.png");
            m_swcs = new TutorialBookletPage("SWCs", m_swcText, m_tutorialBookletArea, "Doom_Scroll.Assets.SWCInTaskList_WithArrow.png", "Doom_Scroll.Assets.SWCStartScreen.png");
            //m_screenshots = new Page("Images", "Doom_Scroll.Assets.file.png", m_screenshotsText, m_tutorialBookletArea);
            m_folderSystem = new TutorialBookletPage("Folder System", m_folderSystemText, m_tutorialBookletArea, "Doom_Scroll.Assets.folderWhereInChat_WithArrow.png", "Doom_Scroll.Assets.folderOpen.png", new Tuple<string, float>("Doom_Scroll.Assets.folderButtonExample.png", 0.75f));

            m_pageOrder = new List<CustomUI>
            {
                m_titlePage,
                m_headlines,
                m_headlinesThree,
                m_headlinesTwo,
                m_signInForms,
                m_infiniteChatLogs,
                m_swcs,
                m_folderSystem
            };

            tutorialBookletPager.UpdatePages(m_pageOrder);

            m_currentIndex = 0;
        }

        private void TogglePager()
        {
            if (!m_tutorialBookletToggleBtn.IsActive) return;
            if (m_tutorialBookletArea.IsModalOpen)
            {
                tutorialBookletPager.HidePage();
                DoomScroll._log.LogInfo("CURRENT PAGE CLOSED");
            }
            else
            {
                DoomScroll._log.LogInfo("ROOT PAGE OPEN");
                // (re)sets root as the current and m_previous folder and displays its content
                m_currentIndex = 0;
                tutorialBookletPager.DisplayPage(1);
            }
        }

        public void ClosePager()
        {
            if (m_tutorialBookletArea.IsModalOpen)
            {
                tutorialBookletPager.HidePage();
                DoomScroll._log.LogInfo("TUTORIAL BOOKLET OVERLAY CLOSED");
            }
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
