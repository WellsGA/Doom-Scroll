using UnityEngine;

namespace Doom_Scroll.UI
{
    public class CustomModal : CustomUI
    {
        // inherits UIGameObject from base
        private SpriteRenderer m_spriteRenderer;

        public CustomModal(GameObject parent, string name, Sprite image) : base(parent, name)
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

        public Vector2 GetSize()
        {
            return m_spriteRenderer.size * UIGameObject.transform.localScale;
        }

    }
}
