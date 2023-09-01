using UnityEngine;
using Doom_Scroll.Common;
using Rewired.Utils;

namespace Doom_Scroll.UI
{
    public class CustomButton : CustomUI
    {
        public enum ImageType
        {
            DEFAULT,
            HOVER
        }

        // Creates and manages custom buttnos
        public DoomScrollEvent ButtonEvent = new DoomScrollEvent();
        // inherits UIGameObject from base
        
        private SpriteRenderer m_spriteRenderer;
        private Sprite[] m_buttonImage;
       
        private bool isDefaultImg;
        public bool IsEnabled { get; private set; }
        public bool IsActive { get; private set; }
        public CustomText Label { get; private set; }

        private GameObject btnIcon;
        private SpriteRenderer iconSpriterenderer;

        public CustomButton(GameObject parent, string name, Sprite[] images, Vector3 position, float scaledX) : base(parent, name)
        {
            BasicButton(images);
            // size has to be set after setting the image!
            // ensure the images are sized correctly and scaled proportionately 
            SetSize(scaledX);
            SetLocalPosition(position);
        }
        public CustomButton(GameObject parent, string name, Sprite[] images) : base(parent, name)
        {
            BasicButton(images);
        }
        private void BasicButton(Sprite[] images)
        {
            m_buttonImage = images;
            m_spriteRenderer = UIGameObject.AddComponent<SpriteRenderer>();
            m_spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            SetButtonImg(ImageType.DEFAULT);
            SetScale(Vector3.one);
            Label = new CustomText(UIGameObject, "label", ""); //empty button label
            Label.SetScale(Vector3.one);
            Label.SetSize(1f);
            btnIcon = new GameObject("Icon");
            btnIcon.layer = LayerMask.NameToLayer("UI");
            btnIcon.transform.SetParent(UIGameObject.transform.parent.transform);
            iconSpriterenderer = btnIcon.AddComponent<SpriteRenderer>();
            iconSpriterenderer.drawMode = SpriteDrawMode.Sliced;
            ActivateCustomUI(true);
            EnableButton(true);
        }

        public Vector2 GetSize()
        {
            return m_spriteRenderer.size;
        }
        public override void SetSize(float scaledWidth)
        {
            m_spriteRenderer.size = new Vector2(scaledWidth, m_spriteRenderer.sprite.rect.height * scaledWidth / m_spriteRenderer.sprite.rect.width);
        }
        public void SetColor(Color color)
        {
            m_spriteRenderer.color = color;
        }

        public SpriteRenderer AddButtonIcon(Sprite icon, float rim)
        {
            iconSpriterenderer.sprite = icon;
            iconSpriterenderer.size = m_spriteRenderer.size * rim;
            btnIcon.transform.position = UIGameObject.transform.position;
            btnIcon.transform.localPosition = UIGameObject.transform.localPosition + new Vector3(0, 0, -10);
            return iconSpriterenderer;
        }
        public void RemoveButtonIcon()
        {
            iconSpriterenderer.sprite = null;
        }
        public override void ActivateCustomUI(bool value)
        {
            base.ActivateCustomUI(value);
            IsActive = value;
        }
        public bool isHovered()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 btnPos = m_spriteRenderer.transform.position;
            Vector3 btnScale = m_spriteRenderer.bounds.extents;

            bool isInBoundsX = btnPos.x - btnScale.x < mousePos.x && btnPos.x + btnScale.x > mousePos.x;
            bool isInBoundsY = btnPos.y - btnScale.y < mousePos.y && btnPos.y + btnScale.y > mousePos.y;

            return isInBoundsX && isInBoundsY && IsEnabled && IsActive;
        }

        private void SetButtonImg(ImageType type)
        {
            switch (type)
            {
                case ImageType.DEFAULT:
                    m_spriteRenderer.sprite = m_buttonImage[0];
                    isDefaultImg = true;
                    break;
                case ImageType.HOVER:
                    m_spriteRenderer.sprite = m_buttonImage.Length > 1 ? m_buttonImage[1] : m_buttonImage[0];
                    isDefaultImg = false;
                    break;
                default:
                    m_spriteRenderer.sprite = m_buttonImage[0];
                    isDefaultImg = true;
                    break;
            }
        }

        public void ReplaceImgageOnHover()
        {
            if (isDefaultImg && isHovered())
            {
                SetButtonImg(ImageType.HOVER);
            }
            else if (!isDefaultImg && !isHovered())
            {
                SetButtonImg(ImageType.DEFAULT);
            }
        }

        public void ResetButtonImage(Sprite[] newImage)
        {
            m_buttonImage = newImage;
            SetButtonImg(ImageType.DEFAULT);
            SetScale(Vector3.one);
        }

        public void EnableButton(bool value)
        {
            IsEnabled = value;
            if (value)
            {
                m_spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                m_spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);
            }
        }

    }
}
