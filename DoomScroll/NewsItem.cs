using Doom_Scroll.Common;
using Doom_Scroll.UI;
using UnityEngine;
using System.Reflection;
using Hazel;
using Doom_Scroll.Patches;

namespace Doom_Scroll
{
    public class NewsItem
    {
        public int NewsID { get; private set; }
        public byte AuthorID { get; private set; }
        public GameObject AuthorIcon { get; private set; }
        public string AuthorName { get; private set; }
        public string Title { get; private set; }
        public bool IsTrue { get; private set; }
        public string Source { get; private set; }
        public CustomModal Card { get; private set; }
        public CustomButton PostButton { get; private set; }
        private CustomText titleUI;
        private CustomText sourceUI;
        
        // endorsement
        public CustomButton EndorseButton { get; private set; }
        public CustomText EndorseLable { get; private set; }
        public CustomButton DenounceButton { get; private set; }
        public CustomText DenounceLable { get; private set; }
        public int TotalEndorsement { get; set; }
        public int TotalDenouncement { get; set; }
        private bool localPlayerEndorsed;
        private bool localPlayerDenounced;

        public NewsItem(int id, byte player, string headline, bool truth, string source)
        {
            NewsID = id;
            AuthorID = player; // 255 for the automated news - no player
            AuthorName = "";
            Title = headline;
            IsTrue = truth;
            Source = source;
            CreateNewsCard();
            TotalEndorsement = 0;
            TotalDenouncement = 0;
            localPlayerEndorsed = false;
            localPlayerDenounced = false;
        }
        public int EndorsedCorrectly()
        {
            //returns 0 if didn't vote at all. Returns 1 if voted correctly. Returns -1 if voted incorrectly.
            int endorsedCorrectly = 0;
            if (localPlayerEndorsed)
            {
                endorsedCorrectly = IsTrue ? 1 : -1;
            }
            else if (localPlayerDenounced)
            {
                endorsedCorrectly = IsTrue ? -1 : 1;
            }
            return endorsedCorrectly;
        }
        private void CreateNewsCard()
        {
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.card.png");
            CustomModal parent = FolderManager.Instance.GetFolderArea();
            Card = new CustomModal(parent.UIGameObject, "card item", spr);
            Card.SetSize(new Vector3(parent.GetSize().x - 2f, 0.4f, 0));  
            titleUI = new CustomText(Card.UIGameObject, "Headline", Title);
            sourceUI = new CustomText(Card.UIGameObject, "Source", Source);
            titleUI.SetSize(1.2f);
            sourceUI.SetSize(0.9f);
            titleUI.SetLocalPosition(new Vector3(0, 0.05f, -10));
            sourceUI.SetLocalPosition(new Vector3(0, -0.05f, -10));
            sourceUI.SetColor(Color.gray);
            // sourceUI.SetTextAlignment(TMPro.TextAlignmentOptions.BaselineRight);
            AddShareButton();
            AddEndorseButtons();
            Card.ActivateCustomUI(false);
        }
        private void AddShareButton()
        {
            float shareBtnSize = Card.GetSize().y - 0.02f;
            Vector3 position = new Vector3(Card.GetSize().x / 2 - 0.05f, 0, -20);
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] BtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.postButton.png", slices);
            PostButton = new CustomButton(Card.UIGameObject, "Post News", BtnSprites, position, shareBtnSize);
            PostButton.ButtonEvent.MyAction += OnClickShare;
        }

        public void AddEndorseButtons()
        {
            float endorseBtnSize = Card.GetSize().y - 0.15f;
            Vector3 pos = new Vector3(Card.GetSize().x / 2 - endorseBtnSize * 2 - 0.2f, +0.05f, -20);
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            
            Sprite[] endorseSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.endorse.png", slices);
            EndorseButton = new CustomButton(Card.UIGameObject, "Emdorse News", endorseSprites, pos, endorseBtnSize);
            EndorseButton.ButtonEvent.MyAction += OnClickEndorse;
            EndorseLable = new CustomText(EndorseButton.UIGameObject, "Endorse Label", TotalEndorsement.ToString());
            EndorseLable.SetLocalPosition(new Vector3(0, -EndorseButton.GetSize().x / 2 - 0.05f, -10));
            EndorseLable.SetSize(1.2f);
            EndorseLable.SetColor(Color.green);

            Vector3 pos2 = new Vector3(Card.GetSize().x / 2 - endorseBtnSize - 0.1f, +0.05f, -20);
            Sprite[] unEndorseSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.unEndorse.png", slices);
            DenounceButton = new CustomButton(Card.UIGameObject, "UnEndorse News", unEndorseSprites, pos2, endorseBtnSize);
            DenounceLable = new CustomText(DenounceButton.UIGameObject, "Un-Endorse Label", TotalEndorsement.ToString());
            DenounceLable.SetLocalPosition(new Vector3(0, -DenounceButton.GetSize().x / 2 - 0.05f, -10));
            DenounceLable.SetSize(1.2f);
            DenounceLable.SetColor(Color.red);
            DenounceButton.ButtonEvent.MyAction += OnClickUnEndorse;
        }

        public void DisplayNewsCard()
        {
            Card.ActivateCustomUI(true);
        }

        public void SetAuthor(byte id) // set player name and icon if not an automated post
        {
            AuthorID = id;
            CreateAuthorIcon();
        }

        public void CreateAuthorIcon() 
        {
            GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(AuthorID);
            if (playerInfo != null)
            {
                AuthorName = playerInfo.PlayerName;
                // create the author icon displayed on the card
                Sprite playerSprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.playerIcon.png");
                AuthorIcon = new GameObject("Author Icon");
                AuthorIcon.layer = LayerMask.NameToLayer("UI");
                AuthorIcon.transform.SetParent(Card.UIGameObject.transform);
                SpriteRenderer sr = AuthorIcon.AddComponent<SpriteRenderer>();
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.sprite = playerSprite;
                sr.size = new Vector2(Card.GetSize().y, sr.sprite.rect.height * Card.GetSize().y / sr.sprite.rect.width);
                sr.color = Palette.PlayerColors[playerInfo.DefaultOutfit.ColorId];
                AuthorIcon.transform.localPosition = new Vector3(-Card.GetSize().x / 2 + sr.size.x, 0.08f, -10);
                AuthorIcon.transform.localScale = Vector3.one;
                CustomText label = new CustomText(AuthorIcon, playerInfo.PlayerName + "- icon label", playerInfo.PlayerName);
                label.SetLocalPosition(new Vector3(0, -sr.size.y / 2 - 0.05f, -10));
                label.SetSize(0.8f);
                /* titleUI.SetLocalPosition(new Vector3(sr.size.x, 0.05f, -10));
                 sourceUI.SetLocalPosition(new Vector3(sr.size.x, -0.05f, -10));*/
            }
        }
        private void OnClickShare()
        {
            if (DestroyableSingleton<HudManager>.Instance && AmongUsClient.Instance.AmClient)
            {
                ChatControllerPatch.content = ChatContent.TEXT;
                string chatText = NewsToChatText();
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, chatText);
            }
            RpcPostNews();
            PostButton.EnableButton(false);
        }

        public string NewsToChatText()
        {
            string chatText = AuthorName.Length > 0 ? AuthorName + " posted: " : "";
            chatText += "<color=#366999><i>" + Title + "</i>\n\t" + Source + "\n <color=#00ff00> :) " + TotalEndorsement + "<color=#ff0000> :( " + TotalDenouncement;
            return chatText;
        }

        private void RpcPostNews()
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDNEWSTOCHAT, (SendOption)1);
            messageWriter.Write(NewsID);    // id
            messageWriter.EndMessage();
        }

        private void OnClickEndorse()
        {
            if (localPlayerDenounced) // if already denounced, it has to change too
            {
                OnClickUnEndorse();
            }
            TotalEndorsement = localPlayerEndorsed ? TotalEndorsement - 1 : TotalEndorsement + 1;
            localPlayerEndorsed = !localPlayerEndorsed;
            EndorseLable.SetText(TotalEndorsement.ToString());
            DoomScroll._log.LogInfo("Endorsed: " + TotalEndorsement);
            RpcShareEndorsement(true, localPlayerEndorsed);
        }

        private void OnClickUnEndorse()
        {
            if (localPlayerEndorsed) // if already endorsed, it has to change too
            {
                OnClickEndorse();
            }
            TotalDenouncement = localPlayerDenounced ? TotalDenouncement - 1 : TotalDenouncement + 1;
            localPlayerDenounced = !localPlayerDenounced;
            DenounceLable.SetText(TotalDenouncement.ToString());
            DoomScroll._log.LogInfo("Denounced: " + TotalDenouncement);
            RpcShareEndorsement(false, localPlayerDenounced);
        }

        private void RpcShareEndorsement(bool endorse, bool up)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDENDORSEMENT, (SendOption)1);
            messageWriter.Write(NewsID);        // id 
            messageWriter.Write(endorse);       // endorse or denounce
            messageWriter.Write(up);            // plus or minus
            messageWriter.EndMessage();
        }

        public override string ToString()
        {
            if (AuthorName == null)
            {
                return Title + " [" + Source + "].";
            }
            else
            {
                return AuthorName + ": " + Title + " [" + Source + "].";
            }           
        }

    }
}
