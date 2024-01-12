using UnityEngine;
using System.Reflection;
using Doom_Scroll.UI;

namespace Doom_Scroll.Common
{
    public class FileScreenshot : File
    {
        public static int ImageSize = 15;
        private Sprite m_Screenshot;
        private byte[] m_content;
        private int m_id;
        public int Id { get { return m_id; } }
        private bool m_shareable;

        public FileScreenshot(string parentPath, string name, CustomModal parentPanel, byte[] image, int id) : base(parentPath, name, parentPanel)
        {
            m_id = id;
            m_content = image;
            Sprite[] shareBtn = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.shareButton.png", ImageLoader.slices2);
            m_Screenshot = ImageLoader.ReadImageFromByteArray(image);
            Sprite[] newBUttonSprites = { m_Screenshot, shareBtn[1], shareBtn[0], shareBtn[0] };
            Btn.ResetButtonImages(newBUttonSprites);
            Btn.SetButtonScale(Vector3.one);
            Btn.Label.SetText(name);
            Btn.Label.SetLocalPosition(new Vector3(0, 0.65f, 0));
            Btn.EnableButton(false); // false initially; sets true once fully shared
            m_shareable = false;
        }
        public override void DisplayContent() // this shares the image in the chat instead of opening
        {
            if(!m_shareable) return;
            byte[] image = m_Screenshot.texture.EncodeToJPG(ImageSize);  // we could use m_content but the size may be bigger
            DoomScroll._log.LogInfo("Original size: " + m_content.Length + ", New image size: " + image.Length);
            ScreenshotManager.Instance.SendImageToChat(m_id);
            Btn.EnableButton(false); // disable after sharing
        }

        public override void HideContent()
        {
            // doesn't need to do anything
        }

        public void SetImageActive()
        {
            Btn.Label.SetText("Image #: " + m_id);
            Btn.EnableButton(true); //sets true once fully shared
            m_shareable = true;
        }

    }
}

