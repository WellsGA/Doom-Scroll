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
        private Dictionary<byte, CustomButton> playerButtons;
        private CustomButton protectButton;
        private CustomButton frameButton;
        private Sprite[] radioBtnSprites;
        private Sprite[] emptyButtonSprites;
        private Sprite playerSprite;
        private GameObject playerButtonParent;

        private bool isprotectSelected;
        private bool isFrameSelected;
        private bool isTargetelected;
        private byte targetPlayer;

        public bool IsInputpanelOpen { get; private set; }
        private bool canPostNews;
        private List<NewsItem> allNewsList;
        private Pageable newsPageHolder;
        private int numPages = 1;
        public int NewsPostedByLocalPLayer { get; set; }
        public List<NewsItem> GetAllNewsList()
        {
            return new List<NewsItem>(allNewsList);
        }

        private NewsFeedManager()
        {
            // init
            playerButtons = new Dictionary<byte, CustomButton>();
            allNewsList = new List<NewsItem>();
            Reset();
            DoomScroll._log.LogInfo("NEWS FEED MANAGER CONSTRUCTOR");
        }

        private void InitializeInputPanel()
        {  
            // button sprites
            Vector4[] slices = { new Vector4(0, 0.66f, 1, 1), new Vector4(0, 0.33f, 1, 0.66f), new Vector4(0, 0, 1, 0.33f) };
            radioBtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.radioButton.png", slices);
            Vector4[] slices2 = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            emptyButtonSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.emptyBtn.png", slices2);
            playerSprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.playerIcon.png");
            playerButtons = new Dictionary<byte, CustomButton>();
            allNewsList = new List<NewsItem>();

            // news modal toggle button
            toggleModalBtn = NewsFeedOverlay.CreateNewsButton(hudManagerInstance);
            toggleModalBtn.ButtonEvent.MyAction += OnClickNews;
            ActivateNewsButton(false);

            // news modal
            newsModal = NewsFeedOverlay.InitInputOverlay(hudManagerInstance);

            // frame and protect buttons
            protectButton = NewsFeedOverlay.CreateRadioButtons(newsModal, radioBtnSprites, new Vector3(-1.2f, 0.3f, -10), true);
            protectButton.ButtonEvent.MyAction += OnclickProtect;
            frameButton = NewsFeedOverlay.CreateRadioButtons(newsModal, radioBtnSprites, new Vector3(0.5f, 0.3f, -10), false);
            frameButton.ButtonEvent.MyAction += OnclickFrame;

            // player buttons
            playerButtonParent = new GameObject("Player buttons");
            playerButtonParent.layer = LayerMask.NameToLayer("UI");
            playerButtonParent.transform.SetParent(newsModal.UIGameObject.transform);
            playerButtonParent.transform.localPosition = new Vector3(0, 0, -10);

            // pagination
            CustomModal parent = FolderManager.Instance.GetFolderArea();
            newsPageHolder = new Pageable(parent.UIGameObject, new List<CustomUI>(), maxNewsItemsPerPage); // sets up an empty pageable 
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
                ClearInputSelection();
                newsModal.ActivateCustomUI(false);
                protectButton.EnableButton(false);
                frameButton.EnableButton(false);
                foreach (KeyValuePair<byte, CustomButton> item in playerButtons) { item.Value.EnableButton(false); }
                IsInputpanelOpen = false;
            }
            else
            {
                if (ScreenshotManager.Instance.IsCameraOpen) { ScreenshotManager.Instance.ToggleCamera(); } // close camera if oopen
                CreatePlayerButtons();
                newsModal.ActivateCustomUI(true);
                protectButton.EnableButton(true);
                frameButton.EnableButton(true);
                foreach (KeyValuePair<byte, CustomButton> item in playerButtons) { item.Value.EnableButton(true); }
                IsInputpanelOpen = true;
            }
        }

        private void ClearInputSelection()
        {
            targetPlayer = 255;
            isTargetelected = false;
            isprotectSelected = false;
            isFrameSelected = false;
            protectButton.RemoveButtonIcon();
            frameButton.RemoveButtonIcon();
        }

        private void CreatePlayerButtons()
        {
            Vector3 nextPos = new Vector3(-newsModal.GetSize().x / 2 + 0.5f, -0.5f, -10);

            playerButtons.Clear();
            while (playerButtonParent.transform.childCount > 0)
            {
                UnityEngine.Object.DestroyImmediate(playerButtonParent.transform.GetChild(0).gameObject);
            }
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (!playerInfo.IsDead && !playerInfo.Disconnected)
                {
                    DoomScroll._log.LogInfo("Player name: " + playerInfo.PlayerName);
                    CustomButton btn = new CustomButton(playerButtonParent, playerInfo.PlayerName, emptyButtonSprites, nextPos, 0.45f);
                    CustomText label = new CustomText(btn.UIGameObject, playerInfo.PlayerName + "- label", playerInfo.PlayerName);
                    label.SetLocalPosition(new Vector3(0, -btn.GetSize().x / 2 - 0.05f, -10));
                    label.SetSize(1.2f);
                    SpriteRenderer sr = btn.AddButtonIcon(playerSprite, 0.7f);
                    sr.color = Palette.PlayerColors[playerInfo.DefaultOutfit.ColorId];
                    playerButtons.Add(playerInfo.PlayerId, btn);
                    nextPos.x += 0.7f;
                }
            }
        }

        private void OnclickProtect()
        {
            if(isprotectSelected)
            {
                protectButton.RemoveButtonIcon();
            }
            else
            {
                if(isFrameSelected) OnclickFrame();
                protectButton.AddButtonIcon(radioBtnSprites[2], 0.6f);
            }
            isprotectSelected = !isprotectSelected;
            DoomScroll._log.LogInfo("protect: " + isprotectSelected + ", frame: " + isFrameSelected);
        }

        private void OnclickFrame()
        {
            if (isFrameSelected)
            {
                frameButton.RemoveButtonIcon();
            }
            else
            {
                if (isprotectSelected) OnclickProtect();
                frameButton.AddButtonIcon(radioBtnSprites[2], 0.6f);
            }
            isFrameSelected = !isFrameSelected;
            DoomScroll._log.LogInfo("protect: " + isprotectSelected + ", frame: " + isFrameSelected);

        }
        
        private void OnSelectTargetPlayer(byte id)
        {
            if (isTargetelected)
            {
                playerButtons[targetPlayer].SetColor(Color.white);
                if (targetPlayer == id)
                {
                    isTargetelected = false; // deselect player
                    targetPlayer = 255;
                    return;
                }
            }
            playerButtons[id].SetColor(Color.green);
            targetPlayer = id;
            isTargetelected = true;
            DoomScroll._log.LogInfo("target id: " + targetPlayer);
        }

        public void OnSelectNewsItem()
        {
            bool protect = isprotectSelected ? true : false;
            string name = PlayerControl.AllPlayerControls[targetPlayer] != null ? PlayerControl.AllPlayerControls[targetPlayer].name : "Disconnected Player";
            NewsItem news = CreateRandomNews(protect, name);
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
                if (toggleModalBtn.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    toggleModalBtn.ButtonEvent.InvokeAction();
                }
                // Invoke methods on mouse click - submit news
                if(toggleModalBtn.IsEnabled && IsInputpanelOpen)
                {
                    protectButton.ReplaceImgageOnHover();
                    frameButton.ReplaceImgageOnHover();

                    if (protectButton.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        protectButton.ButtonEvent.InvokeAction();
                    }
                    if (frameButton.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        frameButton.ButtonEvent.InvokeAction();
                    }
                   
                    foreach (KeyValuePair<byte, CustomButton> item in playerButtons)
                    {
                        item.Value.ReplaceImgageOnHover();
                        if (item.Value.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                        {
                            OnSelectTargetPlayer(item.Key);
                        }
                    }

                    if( isTargetelected && (isFrameSelected || isprotectSelected))
                    {
                        OnSelectNewsItem();
                    }
                }
            }
            catch (Exception e)
            {
                DoomScroll._log.LogError("Error invoking method: " + e);
            }
        }

        public void CheckForShareAndEndorseClicks()
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
                        news.EndorseButton.ReplaceImgageOnHover();
                        news.DenounceButton.ReplaceImgageOnHover();
                        if (news.PostButton.IsEnabled && news.PostButton.IsActive && news.PostButton.isHovered() && Input.GetKey(KeyCode.Mouse0))
                        {
                            news.PostButton.ButtonEvent.InvokeAction(); 
                        }
                        if (news.EndorseButton.IsEnabled && news.EndorseButton.IsActive && news.EndorseButton.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                        {
                            news.EndorseButton.ButtonEvent.InvokeAction();
                        }
                        if (news.DenounceButton.IsEnabled && news.DenounceButton.IsActive && news.DenounceButton.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                        {
                            news.DenounceButton.ButtonEvent.InvokeAction();
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

        // Create post by player
        private NewsItem CreateRandomNews(bool protect, string name)
        {
            string headline;
            string source = "";
            if (protect)
            {
                int rand = UnityEngine.Random.Range(0, NewsStrings.headlinesProtect1p.Length);
                headline = NewsStrings.headlinesProtect1p[rand];
                headline = headline.Replace("{0}", name);

            }
            else
            {
                int rand = UnityEngine.Random.Range(0, NewsStrings.headlinesFrame1p.Length);
                headline = NewsStrings.headlinesFrame1p[rand];
                headline = headline.Replace("{0}", name);
            }
            // TO DO: ADD SOURCE!!!
            int id = PlayerControl.LocalPlayer.PlayerId * 10 + NewsPostedByLocalPLayer;
            return new NewsItem(id, PlayerControl.LocalPlayer.PlayerId, headline, false, source);   
        }

        // Automatic News Creation - Only if player is host!
        public NewsItem CreateRandomFakeNews() 
        {
            int rand = UnityEngine.Random.Range(0, NewsStrings.fakeSources.Length);
            string source = NewsStrings.fakeSources[rand];
            string headline = GetRandomHeadline();
            int id = PlayerControl.LocalPlayer.PlayerId * 10 + NewsPostedByLocalPLayer;
            return new NewsItem(id, 255, headline, false, source);
        }

        public NewsItem CreateRandomTrueNews() 
        {
            int type = UnityEngine.Random.Range(0, 2); // random type of news
            int rand = UnityEngine.Random.Range(0, NewsStrings.trustedSources.Length);
            string source = NewsStrings.trustedSources[rand];
            
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
                                int rnd = UnityEngine.Random.Range(0, NewsStrings.headlinesProtect2p.Length);
                                headline = NewsStrings.headlinesProtect2p[rnd];
                                headline = headline.Replace("{0}", swc.GetPlayerName());
                                headline = headline.Replace("{1}", swc.GetTargetName());
                            }
                            else if(swc.GetGoal() == Goal.Frame)
                            {
                                int rnd = UnityEngine.Random.Range(0, NewsStrings.headlinesFrame2p.Length);
                                headline = NewsStrings.headlinesFrame2p[rnd];
                                headline = headline.Replace("{0}", swc.GetPlayerName());
                                headline = headline.Replace("{1}", swc.GetTargetName());
                            }
                        }
                    }
                    break;
            }
            int id = PlayerControl.LocalPlayer.PlayerId * 10 + NewsPostedByLocalPLayer;
            return new NewsItem(id, 255, headline, true, source);
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
            allNewsList.Insert(0, news);

            // game log
            if (AmongUsClient.Instance.AmHost)
            {
                string author = news.AuthorID == 255 ? "System" : news.AuthorName;
                GameLogger.Write(GameLogger.GetTime() + " - " + author + " created a headline: " + news.Title + " [" + news.Source + "]");
            }
        }

        public NewsItem GetNewsByID(int id)
        { 
            foreach(NewsItem news in allNewsList)
            {
                if(news.NewsID == id)
                {
                    return news;
                }
            }
            return null;
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
            int type = UnityEngine.Random.Range(0,5);
            switch (type)
            {
                case 0:
                    rand = UnityEngine.Random.Range(0, NewsStrings.headlinesProtect1p.Length);
                    string player = GetRandomPlayerName();
                    headline = NewsStrings.headlinesProtect1p[rand].Replace("{0}", player);
                    break;
                case 1:
                    rand = UnityEngine.Random.Range(0, NewsStrings.headlinesFrame1p.Length);
                    string player2 = GetRandomPlayerName();
                    headline = NewsStrings.headlinesFrame1p[rand].Replace("{0}", player2);
                    break;
                case 22:
                    rand = UnityEngine.Random.Range(0, NewsStrings.headlinesProtect2p.Length);
                    headline = NewsStrings.headlinesProtect2p[rand];
                    headline = headline.Replace("{0}", GetRandomPlayerName());
                    headline = headline.Replace("{1}", GetRandomPlayerName());
                    break;
                case 3:
                    rand = UnityEngine.Random.Range(0, NewsStrings.headlinesFrame2p.Length);
                    headline = NewsStrings.headlinesFrame2p[rand];
                    headline = headline.Replace("{0}", GetRandomPlayerName());
                    headline = headline.Replace("{1}", GetRandomPlayerName());
                    break;
                case 4:
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

            numPages = (int)Math.Ceiling((float)(allNewsList.Count) / FileText.maxNumTextItems);
            DoomScroll._log.LogInfo("Number of pages of news: " + numPages);

            List<CustomUI> newsCards = new List<CustomUI>();
            for (int displayPageNum = 1; displayPageNum <= numPages; displayPageNum++)
            {
                Vector3 pos = new Vector3(0, parent.GetSize().y / 2 - 0.8f, -10);
                for (int currentNewsIndex = (displayPageNum-1)*maxNewsItemsPerPage; currentNewsIndex < allNewsList.Count && currentNewsIndex < displayPageNum * maxNewsItemsPerPage; currentNewsIndex++)
                // stops before index out of range and before printing tasks that should be on next page
                {
                    DoomScroll._log.LogInfo($"Current News Index: {currentNewsIndex}, allnewsList Count: {allNewsList.Count}");
                    NewsItem news = allNewsList[currentNewsIndex];
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
                newsPageHolder = new Pageable(parent.UIGameObject, newsCards, maxNewsItemsPerPage); // sets up an empty pageable 
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
            while (currentNewsIndex < allNewsList.Count && currentNewsIndex < currentPage * FileText.maxNumTextItems)
            {
                allNewsList[currentNewsIndex].Card.ActivateCustomUI(false);
                allNewsList[currentNewsIndex].PostButton.ActivateCustomUI(false);
                currentNewsIndex++;
            }
            */
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
            hudManagerInstance = HudManager.Instance;
            IsInputpanelOpen = false;
            canPostNews = false;
            NewsPostedByLocalPLayer = 0;
            targetPlayer = 255;
            isTargetelected = false;
            isprotectSelected = false;
            isFrameSelected = false;
            playerButtons.Clear();
            allNewsList.Clear();
           
            InitializeInputPanel();
            DoomScroll._log.LogInfo("NEWS MANAGER RESET");
        }
    }
}
