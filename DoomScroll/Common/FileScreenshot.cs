using System;
using System.Linq;
using UnityEngine;
using Doom_Scroll.UI;
using System.Reflection;

namespace Doom_Scroll.Common
{
    public class FileScreenshot : File
    {
        private static int ImageSize = 15;
        private static int ImageSectionLength = 1000;
        private byte[] m_content;
        public Sprite Picture { get; private set; }
        private int m_id;
        private static int m_idCounter = 0;

        public FileScreenshot(string parentPath, string name, GameObject parentPanel, Sprite fileImg, byte[] image) : base (parentPath, name, parentPanel, fileImg)
        {
            m_id = m_idCounter++;

            Picture = ImageLoader.ReadImageFromByteArray(image);
            m_content = image;
        }

        public override void DisplayContent() 
        {
            //preparing arguments for RPCs
            int numMessages = (int)Math.Ceiling(m_content.Length / ImageSectionLength * 1f);
            byte pID = PlayerControl.LocalPlayer.PlayerId;
            Sprite img = ImageLoader.ReadImageFromByteArray(m_content);
            byte[] image = img.texture.EncodeToJPG(ImageSize);

            DoomScroll._log.LogInfo("Share Button Clicked!");

            //sending RPCs
            SendImageInChat.RpcSendChatImage(pID, m_id, image, numMessages);
            foreach (int i in Enumerable.Range(0, numMessages))
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
            return;
        }
    }
}
