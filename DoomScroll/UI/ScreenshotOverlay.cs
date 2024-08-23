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
            //   NEW CODE TO SET UP BUTTON:
            GameObject m_UIParent = hud.MapButton.gameObject;
           /* Component[] components = hud.MapButton.gameObject.GetComponentsInChildren<Component>();
            foreach (Component component in components)
            {
                DoomScroll._log.LogInfo(component.ToString());
            }*/
            
            Vector3 position = new Vector3(0, 0 - 0.64f, 0);
            //Vector2 scaledSize = mapButtonSr.size * hud.MapButton.gameObject.transform.localScale;
            Vector2 scaledSize = new Vector2(0.67f, 0.67f);
            SpriteRenderer mapButtonSr = m_UIParent.transform.Find("Background").GetComponent<SpriteRenderer>();
            if (mapButtonSr) {
                position = new Vector3(0, 0 - mapButtonSr.size.y * 0.7f, 0);
                scaledSize = mapButtonSr.size * 0.67f;
            }
            Sprite[] cameraBtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.cameraFlash2.png", ImageLoader.slices2);

            return new CustomButton(m_UIParent, "Camera Toggle Button", cameraBtnSprites, position, scaledSize.x);
        }

        public static CustomButton CreateCaptureButton(CustomImage parent)
        {
            Sprite[] captureSprite = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.captureScreenNew.png", ImageLoader.slices2);
            Vector2 size = parent.GetSize();
            Vector3 pos = new Vector3(size.x / 2 - 0.7f, 0, -10);

            return new CustomButton(parent.UIGameObject, "Screenshot Button", captureSprite, pos, 0.6f);
        }

        public static CustomImage InitCameraOverlay(HudManager hud)
        {
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.cameraOverlay.png");
            CustomImage cameraOverlay = new CustomImage(hud.gameObject, "ScreenshotOverlay", spr);  
            cameraOverlay.SetLocalPosition(new Vector3(0f, 0f, -15));

            // deactivate by default
            cameraOverlay.ActivateCustomUI(false);
            return cameraOverlay;
        }
    }
}
