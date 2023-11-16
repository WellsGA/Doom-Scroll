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

        public static CustomModal InitInputOverlay(HudManager hud, CustomButton toggler, Sprite spr)
        {
            Vector2 bounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            CustomModal newsModal = new CustomModal(hud.gameObject, "News Form Overlay", spr, toggler, true);
            Vector2 size = bounds /2f;
            newsModal.SetSize(size);
            newsModal.SetLocalPosition(new Vector3(0, 0, -10));
           
            // deactivate by default
            newsModal.ActivateCustomUI(false);
            return newsModal;
        }

        public static CustomButton CreateRadioButtons(GameObject parent, Sprite[] radioSprites, string label)
        {
            CustomButton protectButton = new CustomButton(parent, label + " Radio", radioSprites);
            protectButton.Label.SetText(label);
            return protectButton;
        }

    }
}
