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
        public int NumberOfNewsOptions { get; private set; } = 5;

        private HudManager hudManagerInstance;
        private CustomModal m_inputPanel;
        private CustomButton m_togglePanelButton;
        public bool IsInputpanelOpen { get; private set; }
        private bool canPostNews;
        // list of news created randomly if the player can create news
        private Dictionary<int, NewsItem> newsOptions;
        private List<CustomButton> newsButtons;
        // list of news created randomly and by the selected players -  will be displayed during meetings
        private List<NewsItem> allNewsList;

        // elements added by Alaina for flipping between pages of news
        private int numPages = 1;
        private int currentPage = 1;
        private CustomButton m_nextBtn;
        private CustomButton m_backBtn;

        private NewsFeedManager()
        {
            Reset();
            DoomScroll._log.LogInfo("NEWS FEED MANAGER CONSTRUCTOR");
        }

        private void InitializeInputPanel()
        {
            m_togglePanelButton = NewsFeedOverlay.CreateNewsButton(hudManagerInstance);
            m_inputPanel = NewsFeedOverlay.InitInputOverlay(hudManagerInstance);
            newsButtons = new List<CustomButton>();
            Vector2 parentSize = m_inputPanel.GetSize();
            float inputHeight = 0.5f;
            for (int i = 0; i < NumberOfNewsOptions; i++)
            {    
                CustomButton btn = NewsFeedOverlay.CreateNewsItemButton(m_inputPanel);
                btn.SetLocalPosition(new Vector3(0, parentSize.y / 2 - inputHeight, -10));
                // btn.Label.SetText("LABEL");
                inputHeight += btn.GetSize().y + 0.02f;
                newsButtons.Add(btn);
            }
            m_togglePanelButton.ButtonEvent.MyAction += OnClickNews;
            ActivateNewsButton(false);

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
                foreach(CustomButton btn in newsButtons) { btn.EnableButton(false); }
                IsInputpanelOpen = false;
            }
            else
            {
                if (ScreenshotManager.Instance.IsCameraOpen) { ScreenshotManager.Instance.ToggleCamera(); } // close camera if oopen
                m_inputPanel.ActivateCustomUI(true);
                foreach (CustomButton btn in newsButtons) { btn.EnableButton(true); }
                IsInputpanelOpen = true;
            }
        }

        public void OnSelectNewsItem(int news)
        {
            DoomScroll._log.LogInfo("NEWS FORM SUBMITTED" + newsOptions[news].Title);
            newsOptions[news].SetAuthor(PlayerControl.LocalPlayer.PlayerId);
            RPCShareNews(newsOptions[news]);
            CanPostNews(false);
            ToggleNewsForm();
        }
        public void ActivateNewsButton(bool value)
        {
            m_togglePanelButton.ActivateCustomUI(value); ;
        }

        public void CanPostNews(bool value)
        {
            canPostNews = value;
            m_togglePanelButton.EnableButton(canPostNews);
            if (canPostNews)
            {
                newsOptions = new Dictionary<int, NewsItem>();
                NewsItem news;
                int rand = UnityEngine.Random.Range(0, newsButtons.Count);
                for (int i = 0; i < newsButtons.Count; i++)
                {
                    if (i == rand)
                    {
                        news = CreateTrueNews();
                    }
                    else
                    {
                        news = CreateFakeNews();
                    }
                    newsButtons[i].Label.SetText(news.Title + ", by: " + news.Source + ", " + news.IsTrue);
                    newsOptions.Add(i, news);
                }
            }
        }

        public void CheckButtonClicks()
        {
            if (hudManagerInstance == null || !m_togglePanelButton.IsActive || !canPostNews) return;
            
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
                if(m_togglePanelButton.IsEnabled && IsInputpanelOpen)
                {
                    foreach (CustomButton btn in newsButtons)
                    {
                        btn.ReplaceImgageOnHover();
                        if (btn.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                        {
                            int index = newsButtons.IndexOf(btn);
                            OnSelectNewsItem(index);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DoomScroll._log.LogError("Error invoking method: " + e);
            }
        }

        public void CheckForShareClicks()
        {
            if (hudManagerInstance == null) return;
            // If chat and folder overlay are open invoke events on button clicks
            if (hudManagerInstance.Chat.State == ChatControllerState.Open && FolderManager.Instance.IsFolderOpen())
            {
                try
                {
                    foreach (NewsItem news in allNewsList)
                    {
                        news.PostButton.ReplaceImgageOnHover();
                        if (news.PostButton.IsEnabled && news.PostButton.IsActive && news.PostButton.isHovered() && Input.GetKey(KeyCode.Mouse0))
                        {
                            news.PostButton.ButtonEvent.InvokeAction(); 
                        }
                    }
                }
                catch (Exception e)
                {
                    DoomScroll._log.LogError("Error invoking overlay button method: " + e);
                }
            }

        }
        // Sets CanPostNews for player by host
        // the host selects the player(s) who can create news - at the beginning and after each meeting
        public void SelectPLayersWhoCanPostNews()
        {
            if (!PlayerControl.LocalPlayer.AmOwner) return;
            // select 1/5th of the players randomly and enable news creation for them
            List<PlayerControl> allPlayer = new List<PlayerControl>();
            double numberWhoCanPost = Math.Ceiling((double)PlayerControl.AllPlayerControls.Count/5);
            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                allPlayer.Add(pc);
            }
            for (int i = 0; i < numberWhoCanPost; i++)
            {
                int playerIndex = UnityEngine.Random.Range(0, allPlayer.Count);
                RPCPLayerCanCreateNews(allPlayer[playerIndex]);
                allPlayer.RemoveAt(playerIndex);
            }
            RPCPLayerCanCreateNews(PlayerControl.LocalPlayer); //debug: host can always post
            DoomScroll._log.LogInfo("============== SELECT PLAYER TP POST CALLED =============");
        }

        public void RPCPLayerCanCreateNews(PlayerControl player)
        {
            // if selected player local player set locally
            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                CanPostNews(true);
            }
            // inform the others
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDPLAYERCANPOST, (SendOption)1);
            messageWriter.Write(player.PlayerId);
            messageWriter.Write(player.name);
            messageWriter.EndMessage();
        }

        // Automatic News Creation - Only if player is host!
        public NewsItem CreateFakeNews() 
        {
            int rand = UnityEngine.Random.Range(0, NewsStrings.fakeSources.Length);
            string source = NewsStrings.fakeSources[rand];
            string headline = GetRandomHeadline();
            return new NewsItem(255, headline, false, source);
        }

        public NewsItem CreateTrueNews() 
        {
            int type = UnityEngine.Random.Range(0, 2); // random type of news
            int rand = UnityEngine.Random.Range(0, NewsStrings.trustedSources.Length);
            string source = NewsStrings.trustedSources[rand];
            
            int playerNr = UnityEngine.Random.Range(0, PlayerControl.AllPlayerControls.Count);
            PlayerControl player = PlayerControl.AllPlayerControls[playerNr]; //random player
            string headline = player.name;
            switch (type) 
            {
                case 0: // get the number of completed tasks // TEST THIS!!!! FOR NO SWCs TOO!!
                    int i = 0;
                    foreach (PlayerTask task in player.myTasks)
                    {  
                        if (task.IsComplete) i++;
                    }
                    headline += " completed " + i.ToString() + " tasks.";
                    break;
                case 1:  // get their swc (if any)
                    List<SecondaryWinCondition> swcs = SecondaryWinConditionManager.GetSWCList();
                    DoomScroll._log.LogInfo("SWC length: " + swcs.Count);
                    foreach(SecondaryWinCondition swc in swcs)
                    {
                        DoomScroll._log.LogInfo("FOUND PLAYER: " + swc.GetPayerId());
                        if (player.PlayerId == swc.GetPayerId())
                        {
                            if(swc.GetGoal() == Goal.Protect)
                            {
                                int rnd = UnityEngine.Random.Range(0, NewsStrings.headlinesProtect.Length);
                                headline = NewsStrings.headlinesProtect[rnd];
                                headline = headline.Replace("{0}", swc.GetPlayerName());
                                headline = headline.Replace("{1}", swc.GetTargetName());
                            }
                            else if(swc.GetGoal() == Goal.Frame)
                            {
                                int rnd = UnityEngine.Random.Range(0, NewsStrings.headlinesProtect.Length);
                                headline = NewsStrings.headlinesProtect[rnd];
                                headline = headline.Replace("{0}", swc.GetPlayerName());
                                headline = headline.Replace("{1}", swc.GetTargetName());
                            }
                        }
                    }
                    break;
            }
            return new NewsItem(255, headline, true, source);
        }

        public void RPCShareNews(NewsItem news)
        {
            // set locally
            AddNews(news);
            // share
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDNEWS, (SendOption)1);
            messageWriter.Write(news.AuthorID);
            messageWriter.Write(news.Title);
            messageWriter.Write(news.IsTrue);
            messageWriter.Write(news.Source);
            messageWriter.EndMessage();
        }

        public void AddNews(NewsItem news) 
        {
            NotificationManager.ShowNotification("News posted\n " + news.Title + " [" + news.Source + "]");
            if(news.AuthorID != 255)
            {
                news.CreateAuthorIcon();
            }
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
            int type = UnityEngine.Random.Range(0,4);
            switch (type)
            {
                case 0:
                    rand = UnityEngine.Random.Range(0, NewsStrings.headlines1p.Length);
                    string player = GetRandomPlayerName();
                    headline = NewsStrings.headlines1p[rand].Replace("{0}", player);
                    break;
                case 1:
                    rand = UnityEngine.Random.Range(0, NewsStrings.headlinesProtect.Length);
                    headline = NewsStrings.headlinesProtect[rand];
                    headline = headline.Replace("{0}", GetRandomPlayerName());
                    headline = headline.Replace("{1}", GetRandomPlayerName());
                    break;
                case 2:
                    rand = UnityEngine.Random.Range(0, NewsStrings.headlinesFrame.Length);
                    headline = NewsStrings.headlinesFrame[rand];
                    headline = headline.Replace("{0}", GetRandomPlayerName());
                    headline = headline.Replace("{1}", GetRandomPlayerName());
                    break;
                case 3:
                    // rand = UnityEngine.Random.Range(0, NewsStrings.headlines1p1n.Length);
                    int num = UnityEngine.Random.Range(0,6);
                    headline = NewsStrings.headlines1p1n[0].Replace("{0}", GetRandomPlayerName()); // currently one item
                    headline = headline.Replace("{1}", num.ToString());
                    break;
            }
            return headline;
        }

        // lists posts during meetings
        public void DisplayNews()
        {
            CustomModal parent = FolderManager.Instance.GetFolderArea();
            if (allNewsList.Count <= FileText.maxNumTextItems)
            {
                Vector3 pos = new Vector3(0, parent.GetSize().y / 2 - 0.8f, -10);
                foreach (NewsItem news in allNewsList)
                {
                    news.DisplayNewsCard();
                    pos.y -= news.Card.GetSize().y + 0.05f;
                    news.Card.SetLocalPosition(pos);
                    news.Card.ActivateCustomUI(true);
                    news.PostButton.ActivateCustomUI(true);
                }
                DoomScroll._log.LogInfo(ToString()); // debug
            }
            else
            {
                // set allowed number of pages
                numPages = (int)Math.Ceiling((float)(allNewsList.Count) / FileText.maxNumTextItems);
                DoomScroll._log.LogInfo("Number of pages of tasks: " + numPages);

                // show buttons for flipping between pages
                m_nextBtn.ActivateCustomUI(true);
                m_backBtn.ActivateCustomUI(true);
                DoomScroll._log.LogInfo("Task page buttons activated");

                // to do: list it on a UI modal
                // always show page 1 first
                DisplayNews(1);
            }
        }
        //MESSY SOLUTION: PRACTICALLY A COPY-PASTE OF TASK METHOD. CAN CONDENSE METHODS LATER.
        public void DisplayNews(int displayPageNum)
        {

            DoomScroll._log.LogInfo($"Displaying page {displayPageNum} of tasks.");
            //this case probably won't happen; checking in meantime for first few tests
            if (displayPageNum < 1 || displayPageNum > numPages)
            {
                DisplayNews(1);
            }

            // to do: list it on a UI modal
            int currentNewsIndex = (displayPageNum - 1) * FileText.maxNumTextItems;
            CustomModal parent = FolderManager.Instance.GetFolderArea();
            Vector3 pos = new Vector3(0, parent.GetSize().y / 2 - 0.8f, -10);
            while (currentNewsIndex < allNewsList.Count && currentNewsIndex < displayPageNum * FileText.maxNumTextItems)
            // stops before index out of range and before printing tasks that should be on next page
            {
                NewsItem news = allNewsList[currentNewsIndex];
                news.DisplayNewsCard();
                pos.y -= news.Card.GetSize().y + 0.05f;
                news.Card.SetLocalPosition(pos);
                news.Card.ActivateCustomUI(true);
                news.PostButton.ActivateCustomUI(true);
                currentNewsIndex++;
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
            HideNews();
            DisplayNews(currentPage + 1);
        }

        public void OnClickLeftButton()
        {
            HideNews();
            DisplayNews(currentPage - 1);
        }

        public void CheckForDisplayedNewsPageButtonClicks()
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


        public void HideNews()
        {
            int currentNewsIndex = (currentPage - 1) * FileText.maxNumTextItems;
            while (currentNewsIndex < allNewsList.Count && currentNewsIndex < currentPage * FileText.maxNumTextItems)
            {
                allNewsList[currentNewsIndex].Card.ActivateCustomUI(false);
                allNewsList[currentNewsIndex].PostButton.ActivateCustomUI(false);
                currentNewsIndex++;
            }
        }
        public override string ToString()
        {
            string allnews = "\nNEWS POSTED\n";
            foreach (NewsItem news in allNewsList)
            {
                allnews += news.ToString() + "\n";
            }
            return allnews;
        }

        public void PrintCurrentNewsPublisher(string name)
        {
            DoomScroll._log.LogMessage( name +" can publish news!");
        }
        public void Reset()
        {
            IsInputpanelOpen = false;
            canPostNews = false;
            allNewsList = new List<NewsItem>();
            newsOptions = new Dictionary<int, NewsItem>();
            hudManagerInstance = HudManager.Instance;
            InitializeInputPanel();
            DoomScroll._log.LogInfo("NEWS MANAGER RESET");
        }
    }
}
