using Doom_Scroll.Common;
using System.Reflection;
using UnityEngine;

namespace Doom_Scroll.UI
{
    internal class NewsFeedOverlay
    {
        public static CustomButton CreateNewsInputButton(HudManager hud)
        {
            GameObject m_UIParent = hud.gameObject;
            Vector3 mapBtnPos = hud.MapButton.gameObject.transform.position;
            SpriteRenderer mapButtonSr = hud.MapButton.gameObject.GetComponent<SpriteRenderer>();
            Vector3 position = new Vector3(mapBtnPos.x, mapBtnPos.y - 2 * mapButtonSr.size.y * hud.MapButton.gameObject.transform.localScale.y, mapBtnPos.z);
            Vector2 scaledSize = mapButtonSr.size * hud.MapButton.gameObject.transform.localScale;
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] BtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.postNews.png", slices);

            return new CustomButton(m_UIParent, "News Toggle Button", BtnSprites, position, scaledSize.x);
        }

        public static CustomModal InitInputOverlay(HudManager hud)
        {
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");
            CustomModal cameraOverlay = new CustomModal(hud.gameObject, "News Form Overlay", spr);
            cameraOverlay.SetLocalPosition(new Vector3(0f, 0f, -5));
            // deactivate by default
            cameraOverlay.ActivateCustomUI(false);
            return cameraOverlay;
        }

        public static CustomButton CreateSubmitButton(GameObject parent)
        {
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] submitSprite = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.submitButton.png", slices);
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector3 pos = new Vector3(0, sr.size.y / 2 + 0.7f, -10);

            return new CustomButton(parent, "Submit News Button", submitSprite, pos, 1f);
        }
    }
}
