using System;
using UnityEngine;
using Doom_Scroll.UI;

namespace Doom_Scroll
{
    // a manager class for handleing screenshots
    // basic singleton pattern - not thread safe
    public sealed class ScreenshotManager
    {
        private static ScreenshotManager _instance;
        public static ScreenshotManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ScreenshotManager();
                }
                return _instance;
            }
        }
        private HudManager hudManagerInstance;
        
        private CustomModal UIOverlay;
        private CustomButton m_cameraButton;
        private CustomButton m_captureScreenButton;

        private Camera mainCamrea;
        private int m_screenshots;
        private int m_maxPictures;
        public bool IsCameraOpen { get; private set; }

        private ScreenshotManager()
        {
            mainCamrea = Camera.main;
            hudManagerInstance = HudManager.Instance;
            m_screenshots = 0;
            m_maxPictures = 3;
            IsCameraOpen = false;
            InitializeManager();
            DoomScroll._log.LogInfo("SCREENSHOT MANAGER CONSTRUCTOR");
        }
        private void InitializeManager()
        {
            m_cameraButton = ScreenshotOverlay.CreateCameraButton(hudManagerInstance);
            UIOverlay = ScreenshotOverlay.InitCameraOverlay(hudManagerInstance);
            m_captureScreenButton = ScreenshotOverlay.CreateCaptureButton(UIOverlay.UIGameObject);

            m_cameraButton.ButtonEvent.MyAction += OnClickCamera;
            m_captureScreenButton.ButtonEvent.MyAction += OnClickCaptureScreenshot;
            ActivateCameraButton(false);
        }
        private void CaptureScreenshot()
        {
            if (mainCamrea)
            {
                // hide player and overlay
                ShowOverlays(false);

                // use the main camera to render screen into a texture
                RenderTexture screenTexture = new RenderTexture(Screen.width, Screen.height, 20);
                mainCamrea.targetTexture = screenTexture;
                RenderTexture.active = screenTexture;
                mainCamrea.Render();

                Texture2D screeenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
                screeenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                byte[] byteArray = screeenShot.EncodeToJPG();
                // reset camera, show player and overlay
                RenderTexture.active = null;
                screenTexture.Release();
                mainCamrea.targetTexture = null;
                UnityEngine.Object.Destroy(screenTexture);

                ShowOverlays(true);

                // save the image locally -- for testing purposes
                // System.IO.File.WriteAllBytes(Application.dataPath + "/cameracapture_" + m_screenshots + ".png", byteArray);

                // save the image in the inventory folder
                FolderManager.Instance.AddImageToScreenshots("evidence_#" + m_screenshots + ".jpg", byteArray);
                UnityEngine.Object.Destroy(screeenShot);
                m_screenshots++;
                DoomScroll._log.LogInfo("number of screenshots: " + m_screenshots);
            }
        }

        private void ShowOverlays(bool value)
        {
            PlayerControl.LocalPlayer.gameObject.SetActive(value);
            UIOverlay.ActivateCustomUI(value);
        }

        public void ToggleCamera()
        {
            if (!UIOverlay.UIGameObject) { return; }
            if (IsCameraOpen)
            {
                UIOverlay.ActivateCustomUI(false);
                m_captureScreenButton.EnableButton(false);
                IsCameraOpen = false;
                HudManager.Instance.SetHudActive(true);
            }
            else
            {
                UIOverlay.ActivateCustomUI(true);
                m_captureScreenButton.EnableButton(true);
                IsCameraOpen = true;
                HudManager.Instance.SetHudActive(false);
            }
        }

        public void ActivateCameraButton(bool value)
        {
            m_cameraButton.ActivateCustomUI(value);
        }
        public void OnClickCamera()
        {
            ToggleCamera();
        }

        public void OnClickCaptureScreenshot()
        {
            if (!m_cameraButton.IsActive) { DoomScroll._log.LogInfo("NO CAM"); return; }
            CaptureScreenshot();
            ToggleCamera();

            if (m_screenshots == m_maxPictures)
            {
                m_cameraButton.EnableButton(false);
            }
        }

        public void CheckButtonClicks()
        {
            if (hudManagerInstance == null) return;
            // Replace sprite on mouse hover for both buttons
            m_cameraButton.ReplaceImgageOnHover();
            m_captureScreenButton.ReplaceImgageOnHover();

            try
            {
                // Invoke methods on mouse click - open camera overlay
                if (m_cameraButton.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    m_cameraButton.ButtonEvent.InvokeAction();
                }
                // Invoke methods on mouse click - capture screen
                if (m_captureScreenButton.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    m_captureScreenButton.ButtonEvent.InvokeAction();
                }
            }
            catch (Exception e)
            {
                DoomScroll._log.LogError("Error invoking method: " + e);
            }
        }

        public void ReSet()
        {
            m_screenshots = 0;
            IsCameraOpen = false;
            if (hudManagerInstance == null)
            {
                mainCamrea = Camera.main;
                hudManagerInstance = HudManager.Instance;
                InitializeManager();
            }
            DoomScroll._log.LogInfo("SCREENSHOT MANAGER RESET");
        }
    }
}
