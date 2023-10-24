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
    public class HeadlineManager
    {
        private static readonly HeadlineManager _instance = new HeadlineManager(); // readonly: can be assigned only during static initialization
        public static HeadlineManager Instance
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

        public int NewsPostedByLocalPLayer { get; set; }

        private HeadlineManager()
        {
            // init 
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
            int itemsInARow = playerButtons.GetNumberOfOptions() < 5 ? playerButtons.GetNumberOfOptions() : 5;
            float xOffset = 0.7f;
            float btnsSize = 0.6f;
            newsModal.SetSize((2 * xOffset) + (itemsInARow * (btnsSize + 0.02f)));
            playerButtons.ArrangeButtons(btnsSize, itemsInARow, 0.7f , 0.7f);
        }

        public void OnSelectNewsItem(bool protect, byte targetPlayer)
        {
            Headline news = HeadlineCreator.CreateRandomNews(protect, PlayerControl.AllPlayerControls[targetPlayer]);
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

        public void RPCSandNews(Headline news)
        {
            // set locally
            NewsPostedByLocalPLayer++;
            HeadlineDisplay.Instance.AddNews(news);
            // share
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDNEWS, (SendOption)1);
            messageWriter.Write(news.HeadlineID);
            messageWriter.Write(news.AuthorID);
            messageWriter.Write(news.Title);
            messageWriter.Write(news.IsTrue);
            messageWriter.Write(news.Source);
            messageWriter.EndMessage();
        }
       
        public void Reset()
        {
            hudManagerInstance = HudManager.Instance;
            IsInputpanelOpen = false;
            canPostNews = false;
            NewsPostedByLocalPLayer = 0;
            InitializeInputPanel();
            DoomScroll._log.LogInfo("NEWS MANAGER RESET");
        }
    }
}
