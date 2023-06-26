﻿using Doom_Scroll.Common;
using Doom_Scroll.UI;
using UnityEngine;
using System.Reflection;

namespace Doom_Scroll
{
    public class NewsItem
    {
        public byte Author { get; private set; }
        public string Title { get; private set; }
        public bool IsTrue { get; private set; }
        public string Source { get; private set; }
        
        public CustomModal Card { get; private set; }
        private static Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");

        public NewsItem(byte player, string headline, bool truth, string source)
        {
            Author = player; // 255 for the default automated news - no player
            Title = headline;
            IsTrue = truth;
            Source = source;
        }

        public void DisplayNewsCard(CustomModal parent)
        {
            Card = new CustomModal(parent.UIGameObject, "card item", spr);
            Card.SetSize(new Vector3(parent.GetSize().x - 2f, 0.4f, 0));
            CustomText title = new CustomText(Card.UIGameObject, "Headline", Title);
            CustomText source = new CustomText(Card.UIGameObject, "Source", Source);
            title.SetSize(1.5f);
            source.SetSize(1f);
            title.SetLocalPosition(new Vector3(0, 0.1f, -10));
            source.SetLocalPosition(new Vector3(0, -0.1f, -10));
            source.SetColor(Color.gray);
            // source.SetTextAlignment(TMPro.TextAlignmentOptions.BaselineRight);
            Card.ActivateCustomUI(true);

        }

        public override string ToString()
        {
            GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(Author);
            if (playerInfo == null)
            {
                return Title + " [" + Source + "].";
            }
            else
            {
                return playerInfo.PlayerName + ": " + Title + " [" + Source + "].";
            }           
        }

    }
}
