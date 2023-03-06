using UnityEngine;
using System.Reflection;
using Doom_Scroll.Common;

namespace Doom_Scroll.UI
{   
    //a static container for a set of methods operating on input parameters without having to get or set any internal instance fields.
    internal class ScreenshotOverlay
    {
        public static CustomButton CreateCameraButton(HudManager hud)
        {
            GameObject m_UIParent = hud.gameObject;
            Vector3 mapBtnPos = hud.MapButton.gameObject.transform.position;
            SpriteRenderer mapButtonSr = hud.MapButton.gameObject.GetComponent<SpriteRenderer>();
            Vector3 position = new Vector3(mapBtnPos.x, mapBtnPos.y - mapButtonSr.size.y * hud.MapButton.transform.localScale.y, mapBtnPos.z);
            Vector2 scaledSize = mapButtonSr.size * hud.MapButton.transform.localScale;
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] cameraBtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.cameraFlash.png", slices);

            return new CustomButton(m_UIParent, "Camera Toggle Button", cameraBtnSprites, position, scaledSize.x);
        }

        public static CustomButton CreateCaptureButton(GameObject parent)
        {
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] captureSprite = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.captureScreenNew.png", slices);
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector3 pos = new Vector3(sr.size.x / 2 - 0.7f, 0, -10);

            return new CustomButton(parent, "Screenshot Button", captureSprite, pos, 0.6f);
        }

        public static CustomModal InitCameraOverlay(HudManager hud)
        {
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.cameraOverlay.png");
            CustomModal cameraOverlay = new CustomModal(hud.gameObject, "ScreenshotOverlay", spr);  
            cameraOverlay.SetLocalPosition(new Vector3(0f, 0f, -5));

            // deactivate by default
            cameraOverlay.ActivateCustomUI(false);
            return cameraOverlay;
        }
    }
}
