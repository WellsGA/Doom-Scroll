using Doom_Scroll.Common;
using Doom_Scroll.UI;
using Hazel;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Doom_Scroll.Patches;

namespace Doom_Scroll
{
    // Singleton with static initialization: thread safe without explicitly coding for it,
    // relies on the common language runtime to initialize the variable
    public class NewsFeedManager
    {
        private int maxNewsItemsPerPage = 7; // THIS VALUE SHOULD NOT BE CHANGED IN CLASS

        private static readonly NewsFeedManager _instance = new NewsFeedManager(); // readonly: can be assigned only during static initialization
        public static NewsFeedManager Instance
        {
            get
            {
                return _instance;
            }
        }
     
        private HudManager hudManagerInstance;
        private CustomModal newsModal;
        private CustomButton toggleModalBtn;
        private Tooltip headlineBtnTooltip;
        private Tooltip headlinePopupModalTooltip;

        private Sprite[] playerButtonSprites;
        private CustomSelect<byte> playerButtons;
        private CustomSelect<bool> frameOrProtect;

        public bool IsInputpanelOpen { get; private set; }
        private bool canPostNews;
        public List<NewsItem> AllNewsList { get; private set; }
        private Pageable newsPageHolder;
        private int numPages = 1;
        public int NewsPostedByLocalPLayer { get; set; }
        public Dictionary<byte, string> PlayerScores;
        private NewsFeedManager()
        {
            // init
            AllNewsList = new List<NewsItem>();
            PlayerScores = new Dictionary<byte, string>();
            Reset();
            DoomScroll._log.LogInfo("NEWS FEED MANAGER CONSTRUCTOR");
        }

        private void InitializeInputPanel()
        {
            // button sprites
            Sprite[] radioBtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.radioButton.png", ImageLoader.slices3);
            playerButtonSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.playerBtn.png", ImageLoader.slices4);
            
            // news modal toggle button
            toggleModalBtn = NewsFeedOverlay.CreateNewsButton(hudManagerInstance);
            toggleModalBtn.ButtonEvent.MyAction += OnClickNews;
            headlineBtnTooltip = new Tooltip(toggleModalBtn.UIGameObject, "HeadlineButton", "Share a post! Others will\nsee it in the headlines folder\nduring meetings", 0.5f, 2.7f, new Vector3(-0.8f, -0.4f, 0), 1f);
            ActivateNewsButton(false);

            // news modal
            newsModal = NewsFeedOverlay.InitInputOverlay(hudManagerInstance);
            headlinePopupModalTooltip = new Tooltip(newsModal.UIGameObject, "HeadlinePopup", "Choose whether to protect or frame, then choose a target.\nThis will generate a headline about your target.", 0.5f, 9.5f, new Vector3(0, -1.8f, 0), 1.75f);


            // frame and protect buttons
            frameOrProtect = new CustomSelect<bool>(newsModal.GetSize());
            frameOrProtect.AddSelectOption(true, NewsFeedOverlay.CreateRadioButtons(newsModal, radioBtnSprites, "Protect"));
            frameOrProtect.AddSelectOption(false, NewsFeedOverlay.CreateRadioButtons(newsModal, radioBtnSprites, "Frame"));
            frameOrProtect.ArrangeButtons(0.25f, 2, newsModal.GetSize().x / 2 -0.25f, 0.9f);

            // player buttons
            playerButtons = new CustomSelect<byte>(newsModal.GetSize());

            // pagination
            CustomModal parent = FolderManager.Instance.GetFolderArea();
            newsPageHolder = new Pageable(parent, new List<CustomUI>(), maxNewsItemsPerPage); // sets up an empty pageable 
        }

        public void OnClickNews()
        {
            ToggleNewsForm();
        }

        public void ToggleNewsForm()
        {
            if (!newsModal.UIGameObject) { return; }
            if (IsInputpanelOpen)
            {
                frameOrProtect.ClearSelection();
                playerButtons.ClearSelection();
                newsModal.ActivateCustomUI(false);
                IsInputpanelOpen = false;
            }
            else
            {
                if (ScreenshotManager.Instance.IsCameraOpen) { ScreenshotManager.Instance.ToggleCamera(); } // close camera if oopen
                CreatePlayerButtons();
                newsModal.ActivateCustomUI(true);
                IsInputpanelOpen = true;
            }
        }

        private void CreatePlayerButtons()
        {
            playerButtons.RemoveButtons();
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (!playerInfo.IsDead && !playerInfo.Disconnected)
                {
                    CustomButton btn = new CustomButton(newsModal.UIGameObject, playerInfo.PlayerName, playerButtonSprites);
                    btn.SetDefaultBtnColor(btn.TopIcon, Palette.PlayerColors[playerInfo.DefaultOutfit.ColorId]);
                    btn.Label.SetText(playerInfo.PlayerName);
                    playerButtons.AddSelectOption(playerInfo.PlayerId, btn);                    
                }
            }
            int itemsInARow = playerButtons.GetSelecCount() < 5 ? playerButtons.GetSelecCount() : 5;
            float xOffset = 0.7f;
            float btnsSize = 0.6f;
            newsModal.SetSize((2 * xOffset) + (itemsInARow * (btnsSize + 0.02f)));
            playerButtons.ArrangeButtons(btnsSize, itemsInARow, 0.7f , 0.7f);
        }

        public void OnSelectNewsItem(bool protect, byte targetPlayer)
        {
            NewsItem news = NewsCreator.CreateRandomNews(protect, PlayerControl.AllPlayerControls[targetPlayer]);
            RPCSandNews(news);
            CanPostNews(false);
            ToggleNewsForm();
        }
        public void ActivateNewsButton(bool value)
        {
            toggleModalBtn.ActivateCustomUI(value); ;
        }

        public void CanPostNews(bool value)
        {
            canPostNews = value;
            toggleModalBtn.EnableButton(canPostNews);
        }

        public void CheckButtonClicks()
        {
            if (hudManagerInstance == null || !toggleModalBtn.IsActive || !canPostNews) return;
            
            // Replace sprite on mouse hover for both buttons
            toggleModalBtn.ReplaceImgageOnHover();

            try
            {
                // Invoke methods on mouse click - open news form overlay
                if (toggleModalBtn.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    toggleModalBtn.ButtonEvent.InvokeAction();
                }
                // Invoke methods on mouse click - submit news
                if(toggleModalBtn.IsEnabled && IsInputpanelOpen)
                {
                    frameOrProtect.ListenForSelection();
                    playerButtons.ListenForSelection();
                    if(playerButtons.HasSelected && frameOrProtect.HasSelected)
                    {
                        OnSelectNewsItem(frameOrProtect.Selected.Key, playerButtons.Selected.Key);
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
                    foreach (NewsItem news in AllNewsList)
                    {
                        news.CheckForTrustSelect();
                        news.PostButton.ReplaceImgageOnHover();
                        if (news.PostButton.IsEnabled && news.PostButton.IsActive && news.PostButton.IsHovered() && Input.GetKey(KeyCode.Mouse0))
                        {
                            news.PostButton.ButtonEvent.InvokeAction(); 
                        }
                    }
                }
                catch (Exception e)
                {
                    DoomScroll._log.LogError("Error invoking share post button method: " + e);
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

            // game log
            GameLogger.Write(GameLogger.GetTime() + " - " + player.name + " can create a headline.");
        }

        public void RPCSandNews(NewsItem news)
        {
            // set locally
            NewsPostedByLocalPLayer++;
            AddNews(news);
            // share
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDNEWS, (SendOption)1);
            messageWriter.Write(news.NewsID);
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
            AllNewsList.Insert(0, news);

            // game log
            if (AmongUsClient.Instance.AmHost)
            {
                string author = news.AuthorID == 255 ? "System" : news.AuthorName;
                GameLogger.Write(GameLogger.GetTime() + " - " + author + " created a headline: " + news.Title + " [" + news.Source + "]");
            }
        }

        public NewsItem GetNewsByID(int id)
        { 
            foreach(NewsItem news in AllNewsList)
            {
                if(news.NewsID == id)
                {
                    return news;
                }
            }
            return null;
        }

        // lists posts during meetings
        public void DisplayNews()
        {
            CustomModal parent = FolderManager.Instance.GetFolderArea();

            numPages = (int)Math.Ceiling((float)(AllNewsList.Count) / FileText.maxNumTextItems);
            DoomScroll._log.LogInfo("Number of pages of news: " + numPages);

            List<CustomUI> newsCards = new List<CustomUI>();
            for (int displayPageNum = 1; displayPageNum <= numPages; displayPageNum++)
            {
                Vector3 pos = new Vector3(0, parent.GetSize().y / 2 - 0.8f, -10);
                for (int currentNewsIndex = (displayPageNum-1)*maxNewsItemsPerPage; currentNewsIndex < AllNewsList.Count && currentNewsIndex < displayPageNum * maxNewsItemsPerPage; currentNewsIndex++)
                // stops before index out of range and before printing tasks that should be on next page
                {
                    DoomScroll._log.LogInfo($"Current News Index: {currentNewsIndex}, allnewsList Count: {AllNewsList.Count}");
                    NewsItem news = AllNewsList[currentNewsIndex];
                    news.DisplayNewsCard();
                    pos.y -= news.Card.GetSize().y + 0.05f;
                    news.Card.SetLocalPosition(pos);
                    news.Card.ActivateCustomUI(true);
                    news.PostButton.ActivateCustomUI(true);

                    newsCards.Add(news.Card);
                    news.Card.ActivateCustomUI(false); // unsure if necessary>
                }
            }

            // to do: list it on a UI modal
            // always show page 1 first
            if (newsPageHolder == null)
            {
                DoomScroll._log.LogInfo($"Creating new pageable");
                newsPageHolder = new Pageable(parent, newsCards, maxNewsItemsPerPage); // sets up an empty pageable 
            }
            else
            {
                DoomScroll._log.LogInfo($"Updating pageable");
                newsPageHolder.UpdatePages(newsCards);
            }
            newsPageHolder.DisplayPage(1);
            
        }


        public void HideNews()
        {
            if (newsPageHolder != null)
            {
                newsPageHolder.HidePage();
            }
            /*
            if (numPages > 1)
            int currentNewsIndex = (currentPage - 1) * FileText.maxNumTextItems;
            while (currentNewsIndex < AllNewsList.Count && currentNewsIndex < currentPage * FileText.maxNumTextItems)
            {
                AllNewsList[currentNewsIndex].Card.ActivateCustomUI(false);
                AllNewsList[currentNewsIndex].PostButton.ActivateCustomUI(false);
                currentNewsIndex++;
            }
            */
        }

        public string CalculateEndorsementScores(byte playerID)
        {
            int numCorrect = 0;
            int numIncorrect = 0;
            foreach (NewsItem newsPost in AllNewsList)
            {
                /*if (newsPost.EndorsementList.ContainsKey(playerID))
                {
                    if (newsPost.EndorsementList[playerID] == true) numCorrect++;
                    else numIncorrect++;
                }*/
            }
            string score = "\n\t[" + numCorrect + " correct and " + numIncorrect + " incorrect votes out of" + AllNewsList.Count + "]\n";
            PlayerScores[playerID] = score;
            return score;
        }

        public void CheckForDisplayedNewsPageButtonClicks()
        {
            if (newsPageHolder != null)
            {
                newsPageHolder.CheckForDisplayedPageButtonClicks();
            }
        }
        public override string ToString()
        {
            string allnews = "\nNEWS POSTED\n";
            foreach (NewsItem news in AllNewsList)
            {
                allnews += news.ToString() + "\n";
            }
            return allnews;
        }

        public void Reset()
        {
            hudManagerInstance = HudManager.Instance;
            IsInputpanelOpen = false;
            canPostNews = false;
            NewsPostedByLocalPLayer = 0;
            AllNewsList.Clear();
            PlayerScores.Clear();
            InitializeInputPanel();
            DoomScroll._log.LogInfo("NEWS MANAGER RESET");
        }
    }
}
