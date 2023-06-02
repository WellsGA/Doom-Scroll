﻿using Doom_Scroll.Common;
using System.Reflection;
using UnityEngine;

namespace Doom_Scroll.UI
{
    internal class NewsFeedOverlay
    {
        public static CustomButton CreateNewsInputButton(HudManager hud)
        {
            GameObject UIParent = hud.gameObject;
            Vector3 mapBtnPos = hud.MapButton.gameObject.transform.position;
            SpriteRenderer mapButtonSr = hud.MapButton.gameObject.GetComponent<SpriteRenderer>();
            float yDist = (2 * mapButtonSr.size.y * hud.MapButton.gameObject.transform.localScale.y) + 0.05f;
            Vector3 position = new Vector3(mapBtnPos.x, mapBtnPos.y - yDist, mapBtnPos.z);
            Vector2 scaledSize = mapButtonSr.size * hud.MapButton.gameObject.transform.localScale;
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] BtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.postNews.png", slices);

            return new CustomButton(UIParent, "News Toggle Button", BtnSprites, position, scaledSize.x);
        }

        public static CustomModal InitInputOverlay(HudManager hud)
        {
            Vector2 bounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");
            CustomModal inputForm = new CustomModal(hud.gameObject, "News Form Overlay", spr);
            inputForm.SetSize(4f);
            Vector2 size = inputForm.GetSize();
            inputForm.SetLocalPosition(new Vector3(bounds.x-size.x/2, size.y/2, -5));
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