using UnityEngine;

namespace Doom_Scroll.UI
{
    public class CustomModal : CustomUI
    {
        // inherits UIGameObject from base
        private CustomImage m_background;

        public CustomModal(GameObject parent, string name, Sprite image) : base(parent, name)
        {
            m_background = new CustomImage(this.UIGameObject, name + " modal", image);
        }

        public override void SetSize(float scaledWidth)
        {
            m_background.SetSize(scaledWidth);
        }

        public void SetSize(Vector3 size)
        {
            m_background.SetSize(size);
        }

        public Vector2 GetSize()
        {
            return m_background.GetSize();
        }

    }
}
