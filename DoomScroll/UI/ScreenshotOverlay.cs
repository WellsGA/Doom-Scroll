using UnityEngine;
using System.Reflection;
using Doom_Scroll.Common;

namespace Doom_Scroll.UI
{
    internal class ScreenshotOverlay
    {
        public static CustomButton CreateCameraButton(HudManager hud)
        {
            GameObject m_UIParent = hud.gameObject;
            Vector3 mapBtnPos = hud.MapButton.transform.position;
            SpriteRenderer mapButtonSr = hud.MapButton.GetComponent<SpriteRenderer>();
            Vector3 position = new Vector3(mapBtnPos.x, mapBtnPos.y - mapButtonSr.size.y * hud.MapButton.transform.localScale.y, mapBtnPos.z);
            Vector2 scaledSize = mapButtonSr.size * hud.MapButton.transform.localScale;
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] cameraBtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.cameraFlash.png", slices);

            return new CustomButton(m_UIParent, cameraBtnSprites, position, scaledSize.x, "Camera Toggle Button");
        }

        public static CustomButton CreateCaptureButton(GameObject parent)
        {
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] captureSprite = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.captureScreenNew.png", slices);
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector3 pos = new Vector3(sr.size.x / 2 - 0.7f, 0, -10);

            return new CustomButton(parent, captureSprite, pos, 0.6f, "Screenshot Button");
        }

        public static GameObject InitCameraOverlay(HudManager hud)
        {
            GameObject cameraOverlay = new GameObject("ScreenshotOverlay");
            cameraOverlay.layer = LayerMask.NameToLayer("UI");
            cameraOverlay.transform.SetParent(hud.transform);
            cameraOverlay.transform.localPosition = new Vector3(0f, 0f, -5);

            SpriteRenderer sr = cameraOverlay.AddComponent<SpriteRenderer>();
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.cameraOverlay.png");
            sr.sprite = spr;

            // deactivate by default
            cameraOverlay.SetActive(false);
            return cameraOverlay;
        }
    }
}
