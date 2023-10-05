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

        private byte[] m_content;
        private CustomImage screenshot;
        private int m_id;
        public int Id { get; }
        private static int m_idCounter = 0;
        private bool m_shareable = false;
        public bool Shareable { get; }

        public FileScreenshot(string parentPath, string name, CustomModal parentPanel, byte[] image) : base(parentPath, name, parentPanel)
        {
            Sprite[] shareBtn = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.shareButton.png", ImageLoader.slices2);
            m_id = m_idCounter++;
            m_content = image;
            Sprite picture = ImageLoader.ReadImageFromByteArray(image);

            Btn.Label.SetLocalPosition(new Vector3(0, -0.5f, 0));
            Btn.ResetButtonImages(shareBtn);
            DisplayImageOnButton(picture, 0.9f);  /// set screenshot as a background image
            Btn.EnableButton(false); // now sets false initially; sets true once fully shared

        }
        public override void DisplayContent()
        {
            //preparing arguments for RPCs
            // int numMessages = (int)Math.Ceiling(m_content.Length / ImageSectionLength * 1f);
            // byte pID = PlayerControl.LocalPlayer.PlayerId;
            byte[] image = screenshot.GetSprite().texture.EncodeToJPG(ImageSize);
            DoomScroll._log.LogInfo("file size: " + image.Length);

            // set locally
            if (DestroyableSingleton<HudManager>.Instance && AmongUsClient.Instance.AmClient)
            {
                ChatControllerPatch.content = ChatContent.SCREENSHOT;
                ChatControllerPatch.screenshot = image;
                string chatText = PlayerControl.LocalPlayer.PlayerId.ToString() + "#" + ScreenshotManager.Instance.Screenshots.ToString();
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, chatText);
            }
            //sending RPCs
            RpcSendChatImage(image, PlayerControl.LocalPlayer.PlayerId, m_id);
        }

        public override void HideContent()
        {
            // doesn't need to do anything
        }

        public void RpcSendChatImage(byte[] image, byte playerID, int imageID)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDIMAGETOCHAT, (SendOption)1);
            DoomScroll._log.LogInfo("Sending image to chat! Player id " + playerID + " and image id " + imageID);

            messageWriter.Write(playerID);
            messageWriter.Write(imageID);
            messageWriter.EndMessage();
        }

        private void DisplayImageOnButton(Sprite image, float offset)
        {
            screenshot = new CustomImage(Btn.UIGameObject, "screenshot" + "#" + ScreenshotManager.Instance.Screenshots.ToString(), image);
            screenshot.SetSize(Btn.GetBtnSize().x * offset);
            screenshot.SetScale(Vector3.one);
        }
    }
}

