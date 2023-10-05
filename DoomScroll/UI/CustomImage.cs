using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Doom_Scroll.UI
{
    public class CustomImage : CustomUI
    {
        private SpriteRenderer m_spriteRenderer;
        public CustomImage(GameObject parent, string name, Sprite image) : base(parent, name)
        {
            m_spriteRenderer = UIGameObject.AddComponent<SpriteRenderer>();
            m_spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            m_spriteRenderer.sprite = image;
            SetScale(Vector3.one);
        }

        public override void SetSize(float scaledWidth)
        {
            m_spriteRenderer.size = new Vector2(scaledWidth, m_spriteRenderer.sprite.rect.height * scaledWidth / m_spriteRenderer.sprite.rect.width);
        }
        public void SetSize(Vector3 size)
        {
            m_spriteRenderer.size = size;
        }

        public void SetColor(Color color)
        {
            m_spriteRenderer.color = color;
        }

        public void SetSprite(Sprite sprite)
        {
            m_spriteRenderer.sprite = sprite;    
        }
        public Sprite GetSprite()
        {
           return m_spriteRenderer.sprite;
        }
        public Vector2 GetSize()
        {
            return m_spriteRenderer.size * UIGameObject.transform.localScale;
        }

        public SpriteRenderer GetSpriteRenderer() { return m_spriteRenderer; }
    }
}
