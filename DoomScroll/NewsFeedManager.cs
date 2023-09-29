using Doom_Scroll.Common;
using Doom_Scroll.UI;
using Hazel;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Doom_Scroll.Patches;
using AmongUs.GameOptions;

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
        public List<NewsItem> AllNewsList { get; private set; }
        private Pageable newsPageHolder;
        private int numPages = 1;
        public int NewsPostedByLocalPLayer { get; set; }

        public Dictionary<byte, string> PlayerScores;
        private NewsFeedManager()
        {
            // init
            playerButtons = new Dictionary<byte, CustomButton>();
            AllNewsList = new List<NewsItem>();
            PlayerScores = new Dictionary<byte, string>();
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
            AllNewsList = new List<NewsItem>();

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
            NewsItem news = CreateRandomNews(protect, PlayerControl.AllPlayerControls[targetPlayer]);
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
                    foreach (NewsItem news in AllNewsList)
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

        private string ReplaceSymbolsInHeadline(string raw, string name, int count) 
        {
            if(raw.Contains("{X}")) raw = raw.Replace("{X}", name);
            if (raw.Contains("{Y}")) raw = raw.Replace("{Y}", GetRandomPlayerName());
            if (raw.Contains("{#}")) raw = raw.Replace("{#}", count.ToString());
            return raw;
        }

        private string ReplaceSymbolsInSource(string raw, string color, string name)
        {

            if (raw.Contains("{C}")) raw = raw.Replace("{C}", color);
            if (raw.Contains("{N}")) raw = raw.Replace("{N}", name);
            if (raw.Contains("{NR}")) raw = raw.Replace("{NR}", RemoveRandomLetter(name));
            return raw;
        }

        // Create post by player
        private NewsItem CreateRandomNews(bool protect, PlayerControl player)
        {
            string headline;
            string source;
            // bool isTrustworthy = UnityEngine.Random.value > 0.5f;
            if (protect)
            {
                int rand1 = UnityEngine.Random.Range(0, NewsStrings.unTrustProtect.Length);
                headline = NewsStrings.unTrustProtect[rand1];
            }
            else
            {
                int rand2 = UnityEngine.Random.Range(0, NewsStrings.unTrustFrame.Length);
                headline = NewsStrings.unTrustFrame[rand2];
            }
            int randSource1 = UnityEngine.Random.Range(0, NewsStrings.unTrustSource.Length);
            source = NewsStrings.unTrustSource[randSource1];
            headline = ReplaceSymbolsInHeadline(headline, player.name, GetFinishedTaskCount(player.PlayerId));
            GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(player.PlayerId);
            source = ReplaceSymbolsInSource(source, playerInfo.GetPlayerColorString(), player.name);
            int id = PlayerControl.LocalPlayer.PlayerId * 10 + NewsPostedByLocalPLayer;
            return new NewsItem(id, PlayerControl.LocalPlayer.PlayerId, headline, false, source);
        }

        // Automatic News Creation - Only if player is host!
        public NewsItem CreateRandomFakeNews() 
        {
            bool protect = UnityEngine.Random.value > 0.5f;
            string headline;
            int randSource = UnityEngine.Random.Range(0, NewsStrings.autoUnTrustSource.Length);
            string source = NewsStrings.autoUnTrustSource[randSource]; // no string replace
            if (protect)
            {
                int rand = UnityEngine.Random.Range(0, NewsStrings.autoUnTrustProtect.Length);
                headline = NewsStrings.autoUnTrustProtect[rand];
            }
            else
            {
                int rand1 = UnityEngine.Random.Range(0, NewsStrings.autoUnTrustFrame.Length);
                headline = NewsStrings.autoUnTrustFrame[rand1];
            }
            PlayerControl pl = GetRandomPlayer();
            headline = ReplaceSymbolsInHeadline(headline, pl.name, GetFinishedTaskCount(pl.PlayerId));
            int id = PlayerControl.LocalPlayer.PlayerId * 10 + NewsPostedByLocalPLayer;
            return new NewsItem(id, 255, headline, false, source);
        }

        public NewsItem CreateRandomTrueNews() 
        {
            bool protect = UnityEngine.Random.value > 0.5f;
            PlayerControl pl = GetRandomPlayer();
            string headline = "";
            int randSource = UnityEngine.Random.Range(0, NewsStrings.autoTrustSource.Length);
            string source = NewsStrings.autoTrustSource[randSource]; // no string replace
            bool foundNews = false;
            List<string> types = new List<string>{ "task", "sabotage", "sign-in", "role" };
            while (!foundNews)
            {
                int rand = UnityEngine.Random.Range(0, types.Count);
                string type = types[rand];
                switch (type)
                {
                    case "task": 
                        {
                            int completedTasks = GetFinishedTaskCount(pl.PlayerId);
                            if ((completedTasks > 0 && protect) || (completedTasks == 0 && !protect))
                            {
                                headline = protect ? SelectHeadline(NewsStrings.autoTrustProtect[0]) : SelectHeadline(NewsStrings.autoTrustFrame[0]);
                                headline = ReplaceSymbolsInHeadline(headline, pl.name, completedTasks);
                                foundNews = true;
                            }
                            else
                            {
                                types.Remove(type);
                                if (types.Count == 0) goto default;
                            }
                            break;
                        }
                    case "sign-in":
                        {
                            int signedInTasks = GetAssignedTasks(pl.PlayerId);
                            if((signedInTasks > 0 && protect) || (signedInTasks == 0 && !protect))
                            {
                                headline = protect? SelectHeadline(NewsStrings.autoTrustProtect[1]) : SelectHeadline(NewsStrings.autoTrustFrame[1]);
                                headline = ReplaceSymbolsInHeadline(headline, pl.name, signedInTasks);
                                if (headline.Contains("{TN}")) headline = headline.Replace("{TN}", GetSignedInTaskName(pl.PlayerId).ToString());
                                foundNews = true;
                            }
                            else
                            {
                                types.Remove(type);
                                if (types.Count == 0) goto default;
                            }
                                 break;
                        }
                    case "role":
                        {
                            GameData.PlayerInfo inf = GameData.Instance.GetPlayerById(pl.PlayerId);
                            if ((inf.Role.Role != RoleTypes.Impostor && protect) || (inf.Role.Role == RoleTypes.Impostor && !protect))
                            {
                                int news = UnityEngine.Random.Range(0, NewsStrings.autoTrustProtect[2].Length);
                                headline = protect ? SelectHeadline(NewsStrings.autoTrustProtect[2]) : SelectHeadline(NewsStrings.autoTrustFrame[2]);
                                if (headline.Contains("{X}")) headline = headline.Replace("{X}", pl.name);
                                foundNews = true;
                            }
                            else 
                            {
                                types.Remove(type);
                                if (types.Count == 0) goto default;
                            }
                            break;
                        }
                    case "sabotage":
                        {
                            // TO DO
                            types.Remove(type);
                            if (types.Count == 0) goto default;
                            break;
                        }
                    default: // none of the headlines appeared correct ...
                            {
                                headline = "My cat Sir Wigglesworth loves " + pl.name + ", and Big Wigs hates everyone";
                                foundNews = true;
                                break;
                            }
                }
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

        private string SelectHeadline(string[] headlines)
        {
            int news = UnityEngine.Random.Range(0, headlines.Length);
            return headlines[news];
        }

        private string GetRandomPlayerName() 
        {
            int rand = UnityEngine.Random.Range(0, PlayerControl.AllPlayerControls.Count);
            return PlayerControl.AllPlayerControls[rand].name;
        }

        private PlayerControl GetRandomPlayer()
        {
            int rand = UnityEngine.Random.Range(0, PlayerControl.AllPlayerControls.Count);
            return PlayerControl.AllPlayerControls[rand];
        }
        private int GetFinishedTaskCount(byte id)
        {
            int count = 0;
            GameData.PlayerInfo player = GameData.Instance.GetPlayerById(id);
            foreach (GameData.TaskInfo task in player.Tasks)
            {
                if (task.Complete) count++;
            }
            return count;
        }

        private int GetAssignedTasks(byte id)
        {
            int count = 0;
            foreach(AssignedTask task in TaskAssigner.Instance.AssignedTasks)
            {
                if(id == task.AssigneeId) count++;
            }
            return count;
        }

        private TaskTypes GetSignedInTaskName(byte id)
        {
            TaskTypes type = TaskTypes.None;
            foreach (AssignedTask task in TaskAssigner.Instance.AssignedTasks)
            {
                if (id == task.AssigneeId) type = task.Type;
            }
            return type;
        }

        private string RemoveRandomLetter(string name)
        {
            int rand = UnityEngine.Random.Range(0, name.Length);
            name = name.Remove(rand, 1);
            return name;
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
                if (newsPost.EndorsementList.ContainsKey(playerID))
                {
                    if (newsPost.EndorsementList[playerID] == true) numCorrect++;
                    else numIncorrect++;
                }
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
            AllNewsList.Clear();
            PlayerScores.Clear();
            InitializeInputPanel();
            DoomScroll._log.LogInfo("NEWS MANAGER RESET");
        }
    }
}
