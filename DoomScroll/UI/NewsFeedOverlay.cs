using Doom_Scroll.Common;
using System.Reflection;
using UnityEngine;

namespace Doom_Scroll.UI
{
    internal class NewsFeedOverlay
    {
        public static CustomButton CreateNewsButton(HudManager hud)
        {
            GameObject UIParent = hud.MapButton.gameObject;
            SpriteRenderer mapButtonSr = UIParent.transform.Find("Background").GetComponent<SpriteRenderer>();
            Vector3 position = new Vector3(0 - 0.69f, 0 - 0.64f, 0);
            Vector2 scaledSize = new Vector2(0.67f, 0.67f);
            if (mapButtonSr)
            {
                new Vector3(0 - mapButtonSr.size.y * 0.7f, 0 - mapButtonSr.size.y * 0.7f, 0);
                scaledSize = mapButtonSr.size * 0.67f;
            }
            
            Sprite[] BtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.postNews2.png", ImageLoader.slices2);

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
