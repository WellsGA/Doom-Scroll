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
        public CustomButton Btn { get; private set; }
        public CustomText Label { get; private set; }
        private SpriteRenderer m_spriteRenderer;

        public File(string parentPath, string name, GameObject parentPanel, Sprite fileImg)
        {
            Path = parentPath + "/" + name;

            Dir = new GameObject(name);
            Dir.layer = LayerMask.NameToLayer("UI");
            Dir.transform.SetParent(parentPanel.transform);

            m_spriteRenderer = Dir.AddComponent<SpriteRenderer>();
            m_spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            m_spriteRenderer.sprite = fileImg;
            Dir.transform.localScale = Vector3.one;

            /*Label = new CustomText(Dir, name, name);
            Label.SetLocalPosition(new Vector3(0,-5.2f,0));*/
            
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] shareBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.shareButton.png", slices);
            Btn = new CustomButton(Dir, name, shareBtnImg);
            Btn.SetLocalPosition(Vector3.zero);
            Btn.ActivateCustomUI(false);
            Btn.ButtonEvent.MyAction += DisplayContent;
        }
    
        public virtual void DisplayContent() { }
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
