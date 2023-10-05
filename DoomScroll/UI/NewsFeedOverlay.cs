using Doom_Scroll.Common;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Doom_Scroll.UI
{
    internal class NewsFeedOverlay
    {
        public static CustomButton CreateNewsButton(HudManager hud)
        {
            GameObject UIParent = hud.MapButton.gameObject;
            SpriteRenderer mapButtonSr = hud.MapButton.gameObject.GetComponent<SpriteRenderer>();
            Vector2 scaledSize = mapButtonSr.size;
            float yDist = (3 * mapButtonSr.size.y * hud.MapButton.gameObject.transform.localScale.y);
            Vector3 position = new Vector3(0, 0 - yDist, 0);
            Sprite[] BtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.postNews.png", ImageLoader.slices2);

            return new CustomButton(UIParent, "News Toggle Button", BtnSprites, position, scaledSize.x);
        }

        public static CustomModal InitInputOverlay(HudManager hud)
        {
            Vector2 bounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");
            CustomModal newsModal = new CustomModal(hud.gameObject, "News Form Overlay", spr);
            Vector2 size = new Vector2(bounds.x, bounds.y);
            newsModal.SetSize(size);
            newsModal.SetLocalPosition(new Vector3(0, 0, -10));

            CustomText title = new CustomText(newsModal.UIGameObject, "News Modal Title", "Create a Headline");
            title.SetLocalPosition(new Vector3(0, size.y / 2 - 0.3f, -10));
            title.SetSize(1.5f);
            CustomText subtitle = new CustomText(newsModal.UIGameObject, "News Modal SubTitle", "Select 'protect' or 'frame' and a target player.");
            subtitle.SetLocalPosition(new Vector3(0, size.y / 2 - 0.5f, -10));
            subtitle.SetSize(1.2f);

            // deactivate by default
            newsModal.ActivateCustomUI(false);
            return newsModal;
        }

        public static CustomButton CreateRadioButtons(CustomModal parent, Sprite[] radioSprites, Vector3 pos, bool protect)
        {
            string label = protect ? "Protect" : "Frame";
            CustomButton protectButton = new CustomButton(parent.UIGameObject, label + " Radio", radioSprites);
            protectButton.SetSize(0.25f);
            protectButton.SetLocalPosition(pos);
            CustomText protectLable = new CustomText(protectButton.UIGameObject, label + " Lable", label);
            protectLable.SetSize(1.5f);
            protectLable.SetLocalPosition(new Vector3(protectButton.GetBtnSize().x + 0.2f, 0, -10));

            return protectButton;
        }

    }
}
