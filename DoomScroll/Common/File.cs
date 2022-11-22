using System.Reflection;
using Doom_Scroll.UI;
using UnityEngine;
using static Doom_Scroll.UI.CustomButton;

namespace Doom_Scroll.Common
{
    public enum FileType
    {
        IMAGE,
        MAPSOURCE
    }
    public class File : IDirectory
    {
        public GameObject Dir { get; private set; }
        public string Path { get; private set; }
        public Sprite Picture { get; private set; }
        public CustomButton Btn { get; private set; }
        public CustomText Label { get; private set; }
        private SpriteRenderer m_spriteRenderer;
        private byte[] m_content;
        private FileType m_type;
        public File(string parentPath, GameObject parentPanel, string name, byte[] image, FileType fileType)
        {
            Path = parentPath + "/" + name;
            m_type = fileType;
            m_content = image;
            Dir = new GameObject("name");
            Dir.layer = LayerMask.NameToLayer("UI");
            Dir.transform.SetParent(parentPanel.transform);
            m_spriteRenderer = Dir.AddComponent<SpriteRenderer>();
            m_spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            Label = new CustomText(name, Dir, name);
            Label.SetlocalPosition(new Vector3(0,-5.2f,0));
            Picture = ImageLoader.ReadImageFromByteArray(image);
            m_spriteRenderer.sprite = Picture;
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] shareBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.shareButton.png", slices);
            Btn = new CustomButton(Dir, shareBtnImg, "Share");
            Btn.SetLocalPosition(Vector3.zero);
            Btn.ActivateButton(false);
            Btn.ButtonEvent.MyAction += DisplayContent;
        }
        
        public void DisplayContent()
        {
            // display the content of the file -- TO DO
            switch (m_type)
            {
                case FileType.IMAGE:
                    DoomScroll._log.LogInfo("Share Button Clicked!");
                    RPCManager.RpcSendChatImage(m_content);
                    return;
                case FileType.MAPSOURCE:
                    // to do
                    return;
            }
        }
        public Vector2 GetSize()
        {
            return m_spriteRenderer.size;
        }
        public void ScaleSize(float scaledWidth)
        {
            m_spriteRenderer.size = new Vector2(scaledWidth, m_spriteRenderer.sprite.rect.height * scaledWidth / m_spriteRenderer.sprite.rect.width);
        }
        
        public string PrintDirectory()
        {
            return " " + Path + " [file]";
        }
    }
}
