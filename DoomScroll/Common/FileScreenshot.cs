using System;
using UnityEngine;
using System.Reflection;
using Hazel;
using Doom_Scroll.Patches;
using Doom_Scroll.UI;
using static Il2CppSystem.Uri;

namespace Doom_Scroll.Common
{
    public class FileScreenshot : File
    {
        public static int ImageSize = 15;
        private Sprite m_Screenshot;
        private byte[] m_content;
        private string m_id;
        public string Id { get { return m_id; } } // this was not set and was always null
        private bool m_shareable;

        public FileScreenshot(string parentPath, string name, CustomModal parentPanel, byte[] image) : base(parentPath, name, parentPanel)
        {
            m_id = name;
            m_content = image;
            Sprite[] shareBtn = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.shareButton.png", ImageLoader.slices2);
            m_Screenshot = ImageLoader.ReadImageFromByteArray(image);
            Sprite[] newBUttonSprites = { shareBtn[0], shareBtn[1], shareBtn[0], m_Screenshot };
            Btn.ResetButtonImages(newBUttonSprites);
            Btn.SetButtonScale(Vector3.one);
            Btn.Label.SetText("image Id: " + name);
            Btn.Label.SetLocalPosition(new Vector3(0, 0.6f, 0));
            Btn.EnableButton(false); // now sets false initially; sets true once fully shared
            m_shareable = false;
        }
        public override void DisplayContent() // this shares the image in the chat instead of opening
        {
            if(!m_shareable) return;
            byte[] image = m_Screenshot.texture.EncodeToJPG(ImageSize);  // we could use m_content but the size may be bigger
            DoomScroll._log.LogInfo("Original size: " + m_content.Length + ", New image size: " + image.Length);
            // set locally
            ScreenshotManager.Instance.AddImageToChat(m_id);
            // rpc
            RpcAddImageToChat(m_id);
        }

        public override void HideContent()
        {
            // doesn't need to do anything
        }

        public void SetImageActive()
        {
            Btn.EnableButton(true); // now sets false initially; sets true once fully shared
            m_shareable = true;
        }

        public void RpcAddImageToChat(string imageID)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ADDIMAGETOCHAT, (SendOption)1);
            messageWriter.Write(imageID);
            messageWriter.EndMessage();
        }
    }
}

