using Doom_Scroll.Common;
using Doom_Scroll.UI;
using UnityEngine;
using System.Reflection;

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
            Title = headline;
            IsTrue = truth;
            Source = source;
            CreateNewsCard();
        }
        // to do: display as a share button
        public void CreateNewsCard()
        {
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.card.png");
            CustomModal parent = FolderManager.Instance.GetFolderArea();
            Card = new CustomModal(parent.UIGameObject, "card item", spr);
            Card.SetSize(new Vector3(parent.GetSize().x - 2f, 0.3f, 0));  
            titleUI = new CustomText(Card.UIGameObject, "Headline", Title);
            sourceUI = new CustomText(Card.UIGameObject, "Source", Source);
            titleUI.SetSize(1.2f);
            sourceUI.SetSize(0.9f);
            titleUI.SetLocalPosition(new Vector3(0, 0.05f, -10));
            sourceUI.SetLocalPosition(new Vector3(0, -0.05f, -10));
            sourceUI.SetColor(Color.gray);
            // sourceUI.SetTextAlignment(TMPro.TextAlignmentOptions.BaselineRight);
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
