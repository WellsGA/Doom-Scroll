using Doom_Scroll.Common;
using Doom_Scroll.UI;
using UnityEngine;
using System.Reflection;

namespace Doom_Scroll
{
    public class NewsItem
    {
        public byte Author { get; private set; }
        public string Title { get; private set; }
        public string Source { get; private set; }
        
        public CustomModal Card { get; private set; }
        private static Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");

        public NewsItem(byte player, string headline, string outlet)
        {
            Author = player; //255 for the default automated news - no player
            Title = headline;
            Source = outlet;
        }

        public void DisplayNewsCard(CustomModal parent)
        {
            Card = new CustomModal(parent.UIGameObject, "card item", spr);
            Card.SetSize(new Vector3(parent.GetSize().x - 1f, 2f, 0));
            CustomText title = new CustomText(Card.UIGameObject, "Headline", Title);
            CustomText source = new CustomText(Card.UIGameObject, "Source", Source);
            title.SetSize(3f);
            source.SetColor(Color.gray);
            source.SetTextAlignment(TMPro.TextAlignmentOptions.BaselineRight);
            Card.ActivateCustomUI(true);
        }

        public override string ToString()
        {
            return Author + ": " + Title + " [" + Source + "]." ;
        }

    }
}
