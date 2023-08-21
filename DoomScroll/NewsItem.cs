using Doom_Scroll.Common;
using Doom_Scroll.UI;
using UnityEngine;
using System.Reflection;
using Sentry.Internal;
using Hazel;
using static GameData;

namespace Doom_Scroll
{
    public class NewsItem
    {
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
        public NewsItem(byte player, string headline, bool truth, string source)
        {
            
            AuthorID = player; // 255 for the automated news - no player
            AuthorName = "";
            Title = headline;
            IsTrue = truth;
            Source = source;
            CreateNewsCard();
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
            float shareBtnSize = Card.GetSize().y - 0.02f;
            Vector3 position = new Vector3(Card.GetSize().x /2 - Card.GetSize().y /2, 0, -20);
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] BtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.postButton.png", slices);
            PostButton = new CustomButton(Card.UIGameObject, "Post News", BtnSprites, position, shareBtnSize);
            PostButton.ButtonEvent.MyAction += OnClickShare;
            Card.ActivateCustomUI(false);
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
                string chatText = AuthorName.Length > 0 ?  AuthorName + " posted: " : "";
                ChatControllerPatch.content = ChatContent.TEXT;
                chatText += "<color=#366999><i>" + Title + "</i>\n\t" + Source;
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, chatText);
            }
            RpcPostNews();
            PostButton.EnableButton(false);
        }
        private void RpcPostNews()
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDNEWSTOCHAT, (SendOption)1);
            messageWriter.Write(AuthorName);  // author
            messageWriter.Write(Title);     // post
            messageWriter.Write(Source);    // source
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
