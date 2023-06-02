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
        private CustomInputField m_headline;
        private CustomInputField m_content;
        private CustomButton m_togglePanelButton;
        private CustomButton m_submitButton;
        public bool IsInputpanelOpen { get; private set; }
        private bool canCreateNews;
        
        // list of news created randomly and by the selected players -  will be displayed during meetings
        private static List<string> newsList;

        private NewsFeedManager()
        {
            newsList = new List<string>();
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
            newsList.Add(news);
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
                    headline.Replace("{0}", GetRandomPlayerName());
                    headline.Replace("{1}", GetRandomPlayerName());
                    break;
                case 2:
                    // rand = UnityEngine.Random.Range(0, NewsStrings.headlines1p1n.Length);
                    int num = UnityEngine.Random.Range(0,6);
                    headline = NewsStrings.headlines1p1n[0].Replace("{0}", GetRandomPlayerName()); // currently one item
                    headline.Replace("{1}", num.ToString());
                    break;
            }
            return headline;
        }

        public void DisplayNews()
        {
            string allnews = "\nNEWS FEED\n";
            foreach(string news in newsList)
            {
                allnews += news +"\n";
            }
            DoomScroll._log.LogInfo(allnews);
        }

        public void Reset()
        {
            IsInputpanelOpen = false;
            canCreateNews = false;
            newsList = new List<string>();
            if (hudManagerInstance == null)
            {
                hudManagerInstance = HudManager.Instance;
                InitializeInputPanel();
            }
            DoomScroll._log.LogInfo("NEWS MANAGER RESET");
        }
    }
}
