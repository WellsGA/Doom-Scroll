using System;
using UnityEngine;
using System.Reflection;
using Hazel;
using TMPro;
using Doom_Scroll.Patches;

namespace Doom_Scroll.Common
{
    public class FileScreenshot : File
    {
        public static int ImageSize = 15;
        public static int ImageSectionLength = 1000;

        private byte[] m_content;
        public Sprite Picture { get; private set; }
        private int m_id;
        public int Id { get; }
        private static int m_idCounter = 0;
        private bool m_shareable = false;
        public bool Shareable { get; }

        public FileScreenshot(string parentPath, string name, GameObject parentPanel, byte[] image) : base(parentPath, name, parentPanel)
        {
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] file = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.shareButton.png", slices);
            m_id = m_idCounter++;
            m_content = image;
            Picture = ImageLoader.ReadImageFromByteArray(image);

            Btn.Label.SetLocalPosition(new Vector3(0, -0.5f, 0));
            Btn.ResetButtonImage(file);
            Btn.AddButtonIcon(Picture, 0.2f);
            Btn.EnableButton(false); // now sets false initially; sets true once fully shared

        }
        public override void DisplayContent()
        {
            //preparing arguments for RPCs
            // int numMessages = (int)Math.Ceiling(m_content.Length / ImageSectionLength * 1f);
            // byte pID = PlayerControl.LocalPlayer.PlayerId;
            byte[] image = Picture.texture.EncodeToJPG(ImageSize);
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
    }
}

