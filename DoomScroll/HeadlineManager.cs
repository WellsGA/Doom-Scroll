using Doom_Scroll.Common;
using Doom_Scroll.UI;
using Hazel;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Doom_Scroll.Patches;
using static Il2CppMono.Security.X509.X520;
using static Il2CppSystem.Runtime.Remoting.RemotingServices;
using UnityEngine.Networking.Types;
using System.Drawing;

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
        public CustomModal NewsModal { get; private set; }
        private CustomImage modalFrame;
        private CustomText modalTitle;
        private CustomText modalSubTitle;
        private CustomButton toggleModalBtn;
        private Tooltip headlineBtnTooltip;
        private Tooltip headlinePopupModalTooltip;

        private Sprite[] playerButtonSprites;
        private Sprite[] headlineButtonSprites;
        private CustomSelect<byte> playerButtons;
        private CustomSelect<bool> frameOrProtect;
        private CustomSelect<Headline> headlineSelect;
        public Dictionary<byte, int> numTimesPlayersPosted { get; set; } = new Dictionary<byte, int>();

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
            toggleModalBtn.ButtonEvent.MyAction += ToggleFormItems;
            headlineBtnTooltip = new Tooltip(toggleModalBtn.UIGameObject, "HeadlineButton", "Share a post! Others will\nsee it in the headlines folder\nduring meetings", 0.5f, 2.7f, new Vector3(-0.8f, -0.4f, 0), 1f);
            ActivateNewsButton(false);

            // news modal
            Sprite[] spr = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.phone.png", ImageLoader.slices2);
            NewsModal = NewsFeedOverlay.InitInputOverlay(hudManagerInstance, toggleModalBtn, spr[1]);
            modalFrame = new CustomImage(NewsModal.UIGameObject, "Phone overlay", spr[0]);
            modalTitle = new CustomText(NewsModal.UIGameObject, "News Modal Title", "Create a Headline");
            modalTitle.SetSize(1.7f);
            modalSubTitle = new CustomText(NewsModal.UIGameObject, "News Modal SubTitle", "Select 'protect' or 'frame' and a target player.");
            modalSubTitle.SetSize(1.2f);

            headlinePopupModalTooltip = new Tooltip(NewsModal.UIGameObject, "HeadlinePopup", "Choose whether to protect or frame, then choose a target.\nThis will generate a headline about your target.", 0.5f, 9.5f, new Vector3(0, -1.8f, 0), 1.75f);
            
            // frame and protect buttons
            frameOrProtect = new CustomSelect<bool>(NewsModal.GetSize());
            frameOrProtect.AddSelectOption(true, NewsFeedOverlay.CreateRadioButtons(NewsModal.UIGameObject, radioBtnSprites, "Protect"));
            frameOrProtect.AddSelectOption(false, NewsFeedOverlay.CreateRadioButtons(NewsModal.UIGameObject, radioBtnSprites, "Frame"));

            // player buttons
            playerButtons = new CustomSelect<byte>(NewsModal.GetSize());

            // headlines
            headlineButtonSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.input.png", ImageLoader.slices2);
            headlineSelect = new CustomSelect<Headline>(NewsModal.GetSize());
           }

        private void ToggleFormItems()
        {
            if (NewsModal.UIGameObject == null) { return; }
            if (NewsModal.IsModalOpen)
            {
                frameOrProtect.ClearSelection();
                playerButtons.ClearSelection();
                headlineSelect.ClearSelection();
            }
            else
            {
                if (ScreenshotManager.Instance.IsCameraOpen) { ScreenshotManager.Instance.ToggleCamera(); } // close camera if oopen
                CreatePlayerButtons();
            }
        }

        private void CreatePlayerButtons()
        {
            headlineSelect.RemoveButtons();
            playerButtons.RemoveButtons();
            modalFrame.SetColor(Palette.PlayerColors[PlayerControl.LocalPlayer.cosmetics.ColorId]);
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (!playerInfo.IsDead && !playerInfo.Disconnected)
                {
                    CustomButton btn = new CustomButton(NewsModal.UIGameObject, playerInfo.PlayerName, playerButtonSprites);
                    btn.SetDefaultBtnColor(btn.TopIcon, Palette.PlayerColors[playerInfo.DefaultOutfit.ColorId]);
                    btn.Label.SetText(playerInfo.PlayerName);
                    playerButtons.AddSelectOption(playerInfo.PlayerId, btn);                    
                }
            }
            int itemsInARow = playerButtons.GetNumberOfOptions() < 8 ? playerButtons.GetNumberOfOptions() : 8;
            float xOffset = 0.5f;
            float btnsSize = 0.45f;
            float modalWidth = (2 * xOffset) + (itemsInARow * (btnsSize + 0.02f)) + 1.6f;
            NewsModal.SetSize(modalWidth); // margin, buttons and fram/protect select
            modalFrame.SetSize(modalWidth * 1.02f);
            headlineSelect.SetParentSize(NewsModal.GetSize());
            modalTitle.SetLocalPosition(new Vector3(0, NewsModal.GetSize().y / 2 - 0.3f, -10));
            modalSubTitle.SetLocalPosition(new Vector3(0, NewsModal.GetSize().y / 2 - 0.4f, -10));
            modalFrame.SetLocalPosition(new Vector3(0, 0, -20));
            frameOrProtect.ArrangeButtons(0.3f, 2, -NewsModal.GetSize().y / 2 + 0.7f, 1.2f);
            playerButtons.ArrangeButtons(btnsSize, itemsInARow, -NewsModal.GetSize().y / 2 + 1.7f , 1.42f);
        }
         
        public void OnSelectTargetAndGoal(bool protect, byte targetPlayer)
        {
            headlineSelect.ClearSelection();
            headlineSelect.RemoveButtons();
            for (int i = 0; i < 3; i++)
            {
                Headline news = HeadlineCreator.CreateRandomNews(protect, PlayerControl.AllPlayerControls[targetPlayer]);
                CustomButton btn = new CustomButton(NewsModal.UIGameObject, "headline item", headlineButtonSprites);
                btn.SetScale(Vector3.one);
                btn.SetSize(new Vector2(NewsModal.GetSize().x * 0.85f, 0.4f));
                btn.SetLocalPosition(new Vector3(0, -i*0.5f, -10));
                btn.Label.SetText(news.Title + " [" + news.Source + "}");
                btn.Label.SetSize(1f);
                btn.Label.SetLocalPosition(new Vector3(0f, 0, -20));
                headlineSelect.AddSelectOption(news, btn);
            }
            // headlineSelect.ArrangeButtons(NewsModal.GetSize().x *0.85f, 1, 1.5f, 1.5f);
            frameOrProtect.ClearSelection();
            playerButtons.ClearSelection();
        }

        private void OnSelectHeadline()
        {
            if (headlineSelect.GetNumberOfOptions() == 0 || !headlineSelect.HasSelected) return;
            RPCSandNews(headlineSelect.Selected.Key);
            CanPostNews(false);
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
                NewsModal.ListenForButtonClicks();
                // Invoke methods on mouse click - submit news
                if(toggleModalBtn.IsEnabled && NewsModal.IsModalOpen)
                {
                    frameOrProtect.ListenForSelection();
                    playerButtons.ListenForSelection();
                    if(playerButtons.HasSelected && frameOrProtect.HasSelected)
                    {
                        OnSelectTargetAndGoal(frameOrProtect.Selected.Key, playerButtons.Selected.Key);

                    }
                    if(headlineSelect.GetNumberOfOptions() != 0)
                    {
                        headlineSelect.ListenForSelection();
                        if (headlineSelect.HasSelected)
                        {
                            OnSelectHeadline();
                            NewsModal.ModalToggler.ButtonEvent.InvokeAction();
                        }
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
            // ALLOWS ALL TO POST ONCE!
            List<PlayerControl> allPlayer = new List<PlayerControl>();
            double numberWhoCanPost = Math.Ceiling((double)PlayerControl.AllPlayerControls.Count/5);
            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                RPCPLayerCanCreateNews(pc);
            }
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
            canPostNews = false;
            NewsPostedByLocalPLayer = 0;
            numTimesPlayersPosted = new Dictionary<byte, int>();
            InitializeInputPanel();
            DoomScroll._log.LogInfo("NEWS MANAGER RESET");
        }
    }
}
