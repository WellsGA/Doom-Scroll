using System;
using UnityEngine;
using Doom_Scroll.UI;
using Doom_Scroll.Patches;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using System.Threading.Tasks;
using Doom_Scroll.Common;

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
        
        private CustomImage UIOverlay;
        private CustomButton m_cameraButton;
        private CustomButton m_captureScreenButton;
        private Tooltip m_cameraButtonTooltip;

        private Camera mainCamrea;
        public int Screenshots { get; private set; }
        private int m_maxPictures;
        public bool IsCameraOpen { get; private set; }

        private Dictionary<string, byte[]> AllScreenshots;
        private ScreenshotManager()
        {
            mainCamrea = Camera.main;
            hudManagerInstance = HudManager.Instance;
            Screenshots = 0;
            m_maxPictures = 3;
            IsCameraOpen = false;
            AllScreenshots = new Dictionary<string, byte[]>();
            InitializeManager();
            DoomScroll._log.LogInfo("SCREENSHOT MANAGER CONSTRUCTOR");
        }
        private void InitializeManager()
        {
            m_cameraButton = ScreenshotOverlay.CreateCameraButton(hudManagerInstance);
            m_cameraButtonTooltip = new Tooltip(m_cameraButton.UIGameObject, "CameraToggleButton", "Take a photo! Others will\nsee it in the photo folder\nduring meetings", 0.5f, 2.4f, new Vector3(-0.8f, -0.4f, 0), 1f);
            UIOverlay = ScreenshotOverlay.InitCameraOverlay(hudManagerInstance);
            m_captureScreenButton = ScreenshotOverlay.CreateCaptureButton(UIOverlay);

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
                byte[] imageBytes = screeenShot.EncodeToJPG();
                
                // reset camera, show player and overlay
                RenderTexture.active = null;
                screenTexture.Release();
                mainCamrea.targetTexture = null;
                UnityEngine.Object.Destroy(screenTexture);

                ShowOverlays(true);

                // save the image locally -- for testing purposes
                // System.IO.File.WriteAllBytes(Application.dataPath + "/cameracapture_" + Screenshots + ".png", imageBytes);

                // save the image in the inventory folder, add it to the dictionary of all screenshots
                string imageId = (PlayerControl.LocalPlayer.PlayerId * 10 + Screenshots).ToString();
                FolderManager.Instance.AddImageToScreenshots("image Id: " + imageId, imageBytes);
                AddImage(imageId, imageBytes);

                UnityEngine.Object.Destroy(screeenShot);
                Screenshots++;
                DoomScroll._log.LogInfo("number of screenshots: " + Screenshots);

                //Add image to the image sending queue
                EnqueueImage(imageId);
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
                if (HeadlineManager.Instance.NewsModal.IsModalOpen) 
                {
                    HeadlineManager.Instance.NewsModal.CloseButton.ButtonEvent.InvokeAction(); 
                } // close news form if oopen
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

            if (Screenshots == m_maxPictures)
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
                if (m_cameraButton.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    m_cameraButton.ButtonEvent.InvokeAction();
                }
                // Invoke methods on mouse click - capture screen
                if (m_captureScreenButton.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    m_captureScreenButton.ButtonEvent.InvokeAction();
                }
            }
            catch (Exception e)
            {
                DoomScroll._log.LogError("Error invoking method: " + e);
            }
        }
       
        // MANAGE SHARING AND POSTING SCREENSHOTS
        public void AddImage(string id, byte[] image)
        {
            AllScreenshots[id] = image; // it will overwrite if the same id already exists.
        }

        public byte[] GetScreenshotById(string id)
        {
            if (AllScreenshots.ContainsKey(id))
            {
                return AllScreenshots[id];
            }
            return null;
        }

        public void AddImageToChat(string id)
        {
            if (DestroyableSingleton<HudManager>.Instance && AmongUsClient.Instance.AmClient)
            {
                string chatBubbleID = ChatControllerPatch.GetChatID();  
                ChatControllerPatch.screenshot = GetScreenshotById(id);
                if(ChatControllerPatch.screenshot != null)
                {
                    ChatControllerPatch.content = ChatContent.SCREENSHOT;
                    string chatText = chatBubbleID + "Evidence #" + id;
                    DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, chatText, false);
                }
                else
                {
                    DoomScroll._log.LogInfo("Couldn't find and share the requested image, ID: " + id);
                }
                    
            }
        }
        
        private void EnqueueImage(string id)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                DoomScroll._log.LogInfo("1) Adding image to the queue (host)");
                ImageQueuer.AddToQueue(PlayerControl.LocalPlayer.PlayerId, id);
            }
            else
            {
                RPCEnqueueImage(id);
            }
        }

        private void RPCEnqueueImage(string id)
        {
            DoomScroll._log.LogInfo("1) Sending image to the queue (LP)");
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ENQUEUEIMAGE, (SendOption)1);
            messageWriter.Write(id);
            messageWriter.EndMessage();
        }

        public void SendImageInPieces(string id)
        {
            if (AllScreenshots.ContainsKey(id))
            {
                SendPieces(id, AllScreenshots[id]);
            }
            else  // unlikely but..
            {
                DoomScroll._log.LogInfo("Couldn't find image: " + id + ", next can send...");
                FinishedSending(id);
                // do we want to queue it again??
            }
        }

        public  async void SendPieces(string id, byte[] image)
        {
            int length = 1000;
            // get the first part and send
            for(int i = 0; i<image.Length; i += length)
            {
                byte[] piece = image.Skip(i).Take(length).ToArray();
                RPCImagePiece(id, piece);  // missing: check for success and handle errors
                DoomScroll._log.LogInfo("3) Sending image part: " + i/1000);
                await Task.Delay(2000);
            }
            FinishedSending(id); // done sending, notify host and players
            EnableScreenshotPosting(id);
        }
        
        private void FinishedSending(string id)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                ImageQueuer.FinishedSharing(PlayerControl.LocalPlayer.PlayerId, id);
            }
            else
            {
                RPCFinishedSending(id);
            }
        }
        private void EnableScreenshotPosting(string id)
        {
            foreach (FileScreenshot screenshot in FolderManager.Instance.GetScreenshots())
            {
                if (screenshot.Id == id)
                {
                    screenshot.SetImageActive();
                    DoomScroll._log.LogInfo("5) Image is enabled in the Folder.");
                }
            }
        }
        public void RPCImagePiece(string id, byte[] piece)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDIMAGEPIECE, (SendOption)1);
            messageWriter.Write(id);
            messageWriter.WriteBytesAndSize(piece);
            messageWriter.EndMessage();
        }
        private void RPCFinishedSending(string id)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.IMAGESENDINGCOMPLETE, (SendOption)1);
            messageWriter.Write(id);
            messageWriter.EndMessage();
        }

        public void Reset()
        {
            Screenshots = 0;
            IsCameraOpen = false;
            if (hudManagerInstance == null)
            {
                mainCamrea = Camera.main;
                hudManagerInstance = HudManager.Instance;
                InitializeManager();
            }
            AllScreenshots.Clear();
            DoomScroll._log.LogInfo("SCREENSHOT MANAGER RESET");
        }
    }
}
