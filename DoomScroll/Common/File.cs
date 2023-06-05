using System;
using System.Linq;
using System.Reflection;
using Doom_Scroll.UI;
using Hazel;
using UnityEngine;
using static Doom_Scroll.UI.CustomButton;

namespace Doom_Scroll.Common
{
    public class File : IDirectory
    { 
        public GameObject Dir { get; private set; }
        public string Path { get; private set; }
        public CustomButton Btn { get; set; }
        public CustomText Label { get; private set; }
        private SpriteRenderer m_spriteRenderer;

        public File(string parentPath, string name, GameObject parentPanel)
        {
            // default file icon
            Sprite file = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.file.png");

            Path = parentPath + "/" + name;

            Dir = new GameObject(name);
            Dir.layer = LayerMask.NameToLayer("UI");
            Dir.transform.SetParent(parentPanel.transform);

            m_spriteRenderer = Dir.AddComponent<SpriteRenderer>();
            m_spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            m_spriteRenderer.sprite = file;
            Dir.transform.localScale = Vector3.one;

            Label = new CustomText(Dir, name, name);
        }
    
        public virtual void DisplayContent() { }
        public virtual void HideContent() { }
        public Vector2 GetSize()
        {
            return m_spriteRenderer.size;
        }
        public void ScaleSize(float scaledWidth)
        {
            m_spriteRenderer.size = new Vector2(scaledWidth, m_spriteRenderer.sprite.rect.height * scaledWidth / m_spriteRenderer.sprite.rect.width);
        }
        public void ChangePreviewImage(Sprite img) 
        {
            m_spriteRenderer.sprite = img;
        }
        public string PrintDirectory()
        {
            return " " + Path + " [file]";
        }
    }
}
