using System;
using UnityEngine;
using System.Reflection;
using Hazel;
using Doom_Scroll.Patches;
using Doom_Scroll.UI;

namespace Doom_Scroll.Common
{
    public class FileScreenshot : File
    {
        public static int ImageSize = 15;
        public static int ImageSectionLength = 1000;

 
        public CustomImage Screenshot { get; private set; }
        private byte[] m_content;
        private int m_id;
        public int Id { get; }
        private static int m_idCounter = 0;
        // private bool m_shareable = false;
        public bool Shareable { get; }

        public FileScreenshot(string parentPath, string name, CustomModal parentPanel, byte[] image) : base(parentPath, name, parentPanel)
        {
            Sprite[] shareBtn = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.shareButton.png", ImageLoader.slices2);
            m_id = m_idCounter++;
            m_content = image;
            Sprite picture = ImageLoader.ReadImageFromByteArray(image);
            Btn.ResetButtonImages(shareBtn);
            DisplayImageOnButton(picture, 0.9f);  /// set screenshot as a background image
            Btn.Label.SetLocalPosition(new Vector3(0, -0.5f, 0));
            Btn.EnableButton(false); // now sets false initially; sets true once fully shared

        }
        public override void DisplayContent()
        {
            byte[] image = Screenshot.GetSprite().texture.EncodeToJPG(ImageSize);
            DoomScroll._log.LogInfo("file size: " + image.Length);
            // set locally
            if (DestroyableSingleton<HudManager>.Instance && AmongUsClient.Instance.AmClient)
            {
                string chatBubbleID = ChatControllerPatch.GetChatID();
                ChatControllerPatch.content = ChatContent.SCREENSHOT;
                ChatControllerPatch.screenshot = image;
                string chatText = chatBubbleID + "Screenshot #" + ScreenshotManager.Instance.Screenshots.ToString();
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, chatText, false);

                //sending RPCs
                RpcSendChatImage(chatText, m_id);
            }
        }

        public override void HideContent()
        {
            // doesn't need to do anything
        }

        public void RpcSendChatImage(string chatText, int imageID)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDIMAGETOCHAT, (SendOption)1);
            DoomScroll._log.LogInfo("Sending image to chat! Player: " + PlayerControl.LocalPlayer.name + ", image id " + imageID);
            messageWriter.Write(PlayerControl.LocalPlayer.PlayerId);
            messageWriter.Write(imageID);
            messageWriter.Write(chatText);
            messageWriter.EndMessage();
        }

        private void DisplayImageOnButton(Sprite image, float offset)
        {
            Screenshot = new CustomImage(Btn.UIGameObject, "screenshot" + "#" + ScreenshotManager.Instance.Screenshots.ToString(), image);
            Screenshot.SetSize(Btn.GetBtnSize().x * offset);
            Screenshot.SetScale(Vector3.one);
        }
    }
}

