using System;
using UnityEngine;
using System.Reflection;
using Hazel;
using TMPro;

namespace Doom_Scroll.Common
{
    public class FileScreenshot : File
    {
        private static int ImageSize = 15;
        // private static int ImageSectionLength = 1000;
        private byte[] m_content;
        public Sprite Picture { get; private set; }
        // private int m_id;
        // private static int m_idCounter = 0;

        public FileScreenshot(string parentPath, string name, GameObject parentPanel, byte[] image) : base (parentPath, name, parentPanel)
        {
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] file = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.shareButton.png", slices);
            // m_id = m_idCounter++;
            m_content = image;
            Picture = ImageLoader.ReadImageFromByteArray(image);

            Btn.Label.SetLocalPosition(new Vector3(0, -0.5f, 0));
            Btn.ResetButtonImage(file);
            Btn.AddButtonIcon(Picture, 0.2f);
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
            RpcSendChatImage(image);
        }

        /*\
        //NEWER SCREENSHOT WAY
        public override void DisplayContent() 
        {
            //preparing arguments for RPCs
            numMessages = (int)Math.Ceiling(m_content.Length / ImageSectionLength * 1f);
            pID = PlayerControl.LocalPlayer.PlayerId;
            Sprite img = ImageLoader.ReadImageFromByteArray(m_content);
            byte[] image = img.texture.EncodeToJPG(ImageSize);

            DoomScroll._log.LogInfo("file size: " + image.Length);

            //sending RPCs
            SendImageInChat.RpcSendChatImage(pID, m_id, image, numMessages);
           /* foreach (int i in Enumerable.Range(0, numMessages))
            {
                if (i != numMessages - 1)
                {
                    SendImageInChat.RPCSendChatImagePiece(pID, m_id, image.Skip(1000 * i).Take(1000).ToArray(), numMessages, i);
                    DoomScroll._log.LogMessage($"Bytearray # {i} of image bytearray sections sent. Length is {image.Skip(1000 * i).Take(1000).ToArray()}");
                }
                else
                {
                    SendImageInChat.RPCSendChatImagePiece(pID, m_id, image.Skip(1000 * i).ToArray(), numMessages, i);
                    DoomScroll._log.LogMessage($"Bytearray # {i} of image bytearray sections sent. Length is {image.Skip(1000 * i).ToArray()}");
                }
            }
        }*/

        public override void HideContent()
        {
            // doesn't need to do anything
        }

        public void RpcSendChatImage(byte[] image)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDIMAGE, (SendOption)1);
            DoomScroll._log.LogInfo(string.Concat(new string[]
            {
                "image: ", image.Length.ToString(),
                ", buffer: ", messageWriter.Buffer.Length.ToString(),
                ", Pos ",messageWriter.Position.ToString()
            }));
            int num = messageWriter.Buffer.Length - messageWriter.Position - 3;
            bool flag = Buffer.ByteLength(image) >= num;
            messageWriter.Write(flag);
            if (flag)
            {
                messageWriter.WriteBytesAndSize(image);
            }
            messageWriter.EndMessage();            
        }
    }
}
