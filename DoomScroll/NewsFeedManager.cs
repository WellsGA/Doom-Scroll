using Doom_Scroll.Common;
using Doom_Scroll.UI;
using Hazel;
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
        private CustomButton m_togglePanelButton;
        public bool IsInputpanelOpen { get; private set; }
        private bool canCreateNews;
        private int numberOfNewsOptions = 5;
        //list of news created randomly if the player can create news
        private List<CustomButton> newsOptions;
        // list of news created randomly and by the selected players -  will be displayed during meetings
        private static List<string> allNewsList;


        private NewsFeedManager()
        {
            Reset();
            DoomScroll._log.LogInfo("NEWS FEED MANAGER CONSTRUCTOR");
        }

        private void InitializeInputPanel()
        {
            m_togglePanelButton = NewsFeedOverlay.CreateNewsInputButton(hudManagerInstance);
            m_inputPanel = NewsFeedOverlay.InitInputOverlay(hudManagerInstance);
            newsOptions = NewsFeedOverlay.AddNewsSelect(m_inputPanel, numberOfNewsOptions);
           
            m_togglePanelButton.ButtonEvent.MyAction += OnClickNews;
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
                foreach(CustomButton button in newsOptions) { button.EnableButton(false); }
                IsInputpanelOpen = false;
            }
            else
            {
                if (ScreenshotManager.Instance.IsCameraOpen) { ScreenshotManager.Instance.ToggleCamera(); } // close camera if oopen
                m_inputPanel.ActivateCustomUI(true);
                foreach (CustomButton button in newsOptions) { button.EnableButton(true); }
                IsInputpanelOpen = true;
            }
        }

        public void OnSelectNewsItem(CustomButton button)
        {  
            DoomScroll._log.LogInfo("NEWS FORM SUBMITTED" + button.UIGameObject.name);
            CanCreateNews(false);
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
            try
            {
                // Invoke methods on mouse click - open news form overlay
                if (m_togglePanelButton.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    m_togglePanelButton.ButtonEvent.InvokeAction();
                }
                // Invoke methods on mouse click - submit news
                foreach (CustomButton button in newsOptions) 
                {
                    button.ReplaceImgageOnHover();
                    if (button.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        OnSelectNewsItem(button);
                    }
                }
                
            }
            catch (Exception e)
            {
                DoomScroll._log.LogError("Error invoking method: " + e);
            }
        }

        // Automatic News Creation - Only if player is host!
        public void CreateFakeNews() 
        {
            int rand = UnityEngine.Random.Range(0, NewsStrings.fakeSources.Length);
            string headline = GetRandomHeadline();
            headline += "\n\t - " + NewsStrings.fakeSources[rand] + " -";
            RPCShareNews(headline);
        }

        public void CreateTrueNews() 
        {
            int type = UnityEngine.Random.Range(0, 2); // random type of news
            int rand = UnityEngine.Random.Range(0, NewsStrings.trustedSources.Length);
            int playerNr = UnityEngine.Random.Range(0, PlayerControl.AllPlayerControls.Count); 
            PlayerControl player = PlayerControl.AllPlayerControls[playerNr]; //random player
            string headline = player.name;
            switch (type) 
            {
                case 0: // get the number of completed tasks
                    int i = 0;
                    foreach (PlayerTask task in player.myTasks)
                    {  
                        if (task.IsComplete) i++;
                    }
                    headline += " completed " + i + " tasks so far.";
                    break;
                case 1:  // get their swc (if any)
                    List<SecondaryWinCondition> swcs = SecondaryWinConditionManager.GetSWCList();
                    string swcString = "had no swc";
                    foreach(SecondaryWinCondition swc in swcs)
                    {
                        if(player.PlayerId == swc.GetPayerId())
                        {
                            swcString = swc.SendableResultsText();
                        }
                    }
                    headline += swcString;
                    break;
            }
            headline += "\n\t - " + NewsStrings.trustedSources[rand] + " -";
            RPCShareNews(headline);
        }

        public void RPCShareNews(string news)
        {
            AddNews(news); // add locally
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDNEWS, (SendOption)1);
            messageWriter.Write(news);
            messageWriter.EndMessage();
        }

        public void AddNews(string news) 
        {
            allNewsList.Add(news);
        }

        private string GetRandomPlayerName() 
        {
            int rand = UnityEngine.Random.Range(0, PlayerControl.AllPlayerControls.Count);
            return PlayerControl.AllPlayerControls[rand].name;
        }

        private string GetRandomHeadline() 
        {
            string headline = "";
            int rand;
            int type = UnityEngine.Random.Range(0,3);
            switch (type)
            {
                case 0:
                    rand = UnityEngine.Random.Range(0, NewsStrings.headlines1p.Length);
                    string player = GetRandomPlayerName();
                    headline = NewsStrings.headlines1p[rand].Replace("{0}", player);
                    break;
                case 1:
                    rand = UnityEngine.Random.Range(0, NewsStrings.headlines2p.Length);
                    headline = NewsStrings.headlines2p[rand];
                    headline = headline.Replace("{0}", GetRandomPlayerName());
                    headline = headline.Replace("{1}", GetRandomPlayerName());
                    break;
                case 2:
                    // rand = UnityEngine.Random.Range(0, NewsStrings.headlines1p1n.Length);
                    int num = UnityEngine.Random.Range(0,6);
                    headline = NewsStrings.headlines1p1n[0].Replace("{0}", GetRandomPlayerName()); // currently one item
                    headline = headline.Replace("{1}", num.ToString());
                    break;
            }
            return headline;
        }

        public void DisplayNews()
        {
            string allnews = "\nNEWS FEED\n";
            foreach(string news in allNewsList)
            {
                allnews += news +"\n";
            }
            DoomScroll._log.LogInfo(allnews);
        }

        public void Reset()
        {
            IsInputpanelOpen = false;
            canCreateNews = false;
            allNewsList = new List<string>();
            newsOptions = new List<CustomButton>();
            hudManagerInstance = HudManager.Instance;
            InitializeInputPanel();
            DoomScroll._log.LogInfo("NEWS MANAGER RESET");
        }
    }
}
