﻿using UnityEngine;
using Doom_Scroll.Common;

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
        private Sprite[] buttonIcons;
        
        private bool isDefaultImg;
        public bool IsEnabled { get; private set; }
        public bool IsActive { get; private set; }

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
            buttonIcons = images;
            m_spriteRenderer = UIGameObject.AddComponent<SpriteRenderer>();
            m_spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            SetButtonImg(ImageType.DEFAULT);
            SetScale(Vector3.one);
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
                    m_spriteRenderer.sprite = buttonIcons[0];
                    isDefaultImg = true;
                    break;
                case ImageType.HOVER:
                    m_spriteRenderer.sprite = buttonIcons.Length > 1 ? buttonIcons[1] : buttonIcons[0];
                    isDefaultImg = false;
                    break;
                default:
                    m_spriteRenderer.sprite = buttonIcons[0];
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