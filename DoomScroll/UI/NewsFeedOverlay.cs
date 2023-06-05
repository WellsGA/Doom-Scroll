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
            //   NEW CODE TO SET UP BUTTON:
            GameObject UIParent = hud.MapButton.gameObject;
            SpriteRenderer mapButtonSr = hud.MapButton.gameObject.GetComponent<SpriteRenderer>();
            Vector2 scaledSize = mapButtonSr.size;
            float yDist = (3 * mapButtonSr.size.y * hud.MapButton.gameObject.transform.localScale.y);
            Vector3 position = new Vector3(0, 0 - yDist, 0);
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] BtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.postNews.png", slices);

            return new CustomButton(UIParent, "News Toggle Button", BtnSprites, position, scaledSize.x);

            //   ORIGINAL CODE TO SET UP BUTTON:
            /*
            GameObject UIParent = hud.gameObject;
            Vector3 mapBtnPos = hud.MapButton.gameObject.transform.position;
            SpriteRenderer mapButtonSr = hud.MapButton.gameObject.GetComponent<SpriteRenderer>();
            float yDist = (2 * mapButtonSr.size.y * hud.MapButton.gameObject.transform.localScale.y) + 0.05f;
            Vector3 position = new Vector3(mapBtnPos.x, mapBtnPos.y - yDist, mapBtnPos.z);
            Vector2 scaledSize = mapButtonSr.size * hud.MapButton.gameObject.transform.localScale;
            */
        }

        public static CustomModal InitInputOverlay(HudManager hud)
        {
            Vector2 bounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");
            CustomModal inputForm = new CustomModal(hud.gameObject, "News Form Overlay", spr);
            inputForm.SetSize(4.2f);
            Vector2 size = inputForm.GetSize();
            inputForm.SetLocalPosition(new Vector3(bounds.x-size.x/2-1f, size.y/2, -5));
            // deactivate by default
            inputForm.ActivateCustomUI(false);
            return inputForm;
        }

        public static CustomInputField AddInputField(CustomModal parent) 
        {
            Vector2 parentSize = parent.GetSize();
            CustomInputField field = new CustomInputField(parent.UIGameObject, "Headline", "Headline", parentSize.x);
            field.SetLocalPosition(new Vector3(0, parentSize.y/2 - 0.5f, 0));
            return field;
        }
        public static List<CustomButton> AddNewsSelect(CustomModal parent, int listItems)
        {
            List<CustomButton> list = new List<CustomButton>();
            Vector2 parentSize = parent.GetSize();
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] spr = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.input.png", slices);
            float inputHeight = 0.5f;
            for(int i = 0; i < listItems; i++)
            {
                Vector3 pos = new Vector3(0, parentSize.y / 2 - inputHeight, -10);
                CustomButton btn = new CustomButton(parent.UIGameObject, "newsItem", spr, pos, parentSize.x - 0.05f);          
                list.Add(btn);
                inputHeight += btn.GetSize().y + 0.02f;
            }
            return list;
        }
        public static CustomButton CreateSubmitButton(GameObject parent)
        {
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] submitSprite = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.submitButton.png", slices);
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector3 pos = new Vector3(0, -sr.size.y / 2 + 0.4f, -10);

            return new CustomButton(parent, "Submit News Button", submitSprite, pos, 1f);
        }
    }
}
