using Doom_Scroll.Common;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Doom_Scroll.UI
{
    internal class NewsFeedOverlay
    {
        public static CustomButton CreateNewsInputButton(HudManager hud)
        {
            GameObject UIParent = hud.MapButton.gameObject;
            SpriteRenderer mapButtonSr = hud.MapButton.gameObject.GetComponent<SpriteRenderer>();
            Vector2 scaledSize = mapButtonSr.size;
            float yDist = (3 * mapButtonSr.size.y * hud.MapButton.gameObject.transform.localScale.y);
            Vector3 position = new Vector3(0, 0 - yDist, 0);
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] BtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.postNews.png", slices);

            return new CustomButton(UIParent, "News Toggle Button", BtnSprites, position, scaledSize.x);
        }

        public static CustomModal InitInputOverlay(HudManager hud)
        {
            Vector2 bounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");
            CustomModal inputForm = new CustomModal(hud.gameObject, "News Form Overlay", spr);
            inputForm.SetSize(new Vector2(5.2f, 2.5f));
            Vector2 size = inputForm.GetSize();
            inputForm.SetLocalPosition(new Vector3(bounds.x-size.x/2-1f, size.y/2, -5));
            // deactivate by default
            inputForm.ActivateCustomUI(false);
            return inputForm;
        }

        public static CustomButton CreateNewsItemButton(CustomModal parent)
        {
            Vector2 parentSize = parent.GetSize();
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] spr = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.input.png", slices);
            Vector3 pos = new Vector3(0, 0, -10);
            return new CustomButton(parent.UIGameObject, "newsItem", spr, pos, parentSize.x - 0.05f);
        }

    }
}
