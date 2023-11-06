using Doom_Scroll.Common;
using Doom_Scroll.UI;
using UnityEngine;
using System.Reflection;
using Hazel;
using Doom_Scroll.Patches;
using System.Collections.Generic;

namespace Doom_Scroll
{
    public class Headline
    {
        public int HeadlineID { get; private set; }
        public byte AuthorID { get; private set; }
        public GameObject AuthorIcon { get; private set; }
        public string AuthorName { get; private set; }
        public string Title { get; private set; }
        public bool IsTrue { get; private set; }
        public string Source { get; private set; }
        public CustomModal Card { get; private set; }
        public CustomButton PostButton { get; private set; }
        public Dictionary<byte, bool> PlayersTrustSelections { get; private set; }

        private CustomSelect<bool> trustButtons;
        private CustomText titleUI;
        private CustomText sourceUI;
 
        public Headline(int id, byte player, string headline, bool truth, string source)
        {
            HeadlineID = id;
            AuthorID = player; // 255 for the automated news - no player
            AuthorName = "";
            Title = headline;
            IsTrue = truth;
            Source = source;
            PlayersTrustSelections = new Dictionary<byte, bool>();
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
            sourceUI.SetColor(Color.gray);
            titleUI.TextMP.m_width = Card.GetSize().x - 1f;
            sourceUI.TextMP.m_width = Card.GetSize().x - 1f;
            titleUI.SetLocalPosition(new Vector3(-0.2f, 0.05f, -10));
            sourceUI.SetLocalPosition(new Vector3(-0.2f, -0.07f, -10));
            // sourceUI.SetTextAlignment(TMPro.TextAlignmentOptions.BaselineRight);
            AddButtons();
            Card.ActivateCustomUI(false);
        }
        private void AddButtons()
        {
            // trust & not trust
            Sprite[] radioBtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.radioButton.png", ImageLoader.slices3);
            trustButtons = new CustomSelect<bool>(new Vector2(0.4f, 1f));
            trustButtons.AddSelectOption(true, NewsFeedOverlay.CreateRadioButtons(Card, radioBtnSprites, "Trusted"));
            trustButtons.AddSelectOption(false, NewsFeedOverlay.CreateRadioButtons(Card, radioBtnSprites, "Fake"));
            trustButtons.ArrangeButtons(0.22f, 2, Card.GetSize().x / 2 - 0.44f, 0.45f);
            trustButtons.ButtonEvent.MyAction += UpdateTrustSelection;

            // share btn
            float btnSize = 0.4f;
            Vector3 sharBtnPos = new Vector3(Card.GetSize().x / 2 + btnSize /2, 0, -20);
            Sprite[] BtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.postButton.png", ImageLoader.slices2);
            PostButton = new CustomButton(Card.UIGameObject, "Post News", BtnSprites, sharBtnPos, btnSize);
            PostButton.ButtonEvent.MyAction += OnClickShare;
        }

        public void DisplayCardButtons(bool isVoting)
        {
            Card.SetScale(Vector3.one);
            if (trustButtons.HasSelected)
            {
                trustButtons.Selected.Value.SelectButton(true);
            }
            trustButtons.ActivateButtons(isVoting);
            PostButton.ActivateCustomUI(!isVoting);
            Card.ActivateCustomUI(true);
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
                AuthorIcon.transform.localPosition = new Vector3(-Card.GetSize().x / 2 + sr.size.x/1.8f, 0.08f, -10);
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
                // add to chat locally
                ChatControllerPatch.content = ChatContent.HEADLINE;
                string id = ChatControllerPatch.GetChatID();
                string chatText = id + NewsToChatText();
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, chatText, false);
                // send to others
                RpcPostNews(id);
                PostButton.EnableButton(false);
            }         
        }

        public string NewsToChatText()
        {
            string chatText = AuthorName.Length > 0 ? AuthorName + " posted: " : "";
            chatText += "<color=#366999><i>" + Title + "</i>\n\t" + Source + "\n";
            return chatText;
        }

        private void RpcPostNews(string id)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDNEWSTOCHAT, (SendOption)1);
            messageWriter.Write(id);            // chatbubble id
            messageWriter.Write(HeadlineID);    // headlineID
            messageWriter.EndMessage();
        }

        public void CheckForTrustSelect()
        {
            trustButtons.ListenForSelection();
        }

        public void UpdateTrustSelection()
        {
            byte playerId = PlayerControl.LocalPlayer.PlayerId;
            if (trustButtons.HasSelected)
            {
                PlayersTrustSelections[playerId] = trustButtons.Selected.Key;
                RpcTrustSelection(true, trustButtons.Selected.Key);
            }
            else if(!trustButtons.HasSelected && trustButtons.Selected.Value != null) // deselected values
            {
                PlayersTrustSelections.Remove(playerId);
                RpcTrustSelection(false, trustButtons.Selected.Key);
            }
        }

        private void RpcTrustSelection(bool select, bool isTrusted)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDTRUSTSELECTION, (SendOption)1);
            messageWriter.Write(HeadlineID);                            // news id
            messageWriter.Write(select);                                // select or unselect
            messageWriter.Write(isTrusted);                             // trust or not
            messageWriter.EndMessage();
        }

        public void SetParentAndSize(GameObject parent, Vector2 size)
        {
            Card.UIGameObject.transform.SetParent(parent.transform, false);
            Card.SetLocalPosition(parent.transform.position + new Vector3(0, 0, -20));
            Card.SetSize(new Vector3(size.x - 2f, 0.4f, 0));
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
