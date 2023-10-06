using UnityEngine;
using Doom_Scroll.Common;

namespace Doom_Scroll.UI
{
    public class CustomButton : CustomUI
    {
        public enum ButtonState
        {
            DEFAULT,
            HOVERED,
            SELECTED
        }

        // Creates and manages custom buttnos
        public DoomScrollEvent ButtonEvent = new DoomScrollEvent();
        // inherits UIGameObject from base
               
        private bool isSelected;
        private bool hasBackgroundIcon;
        public bool IsEnabled { get; private set; }
        public bool IsActive { get; private set; }
        public CustomText Label { get; private set; }

        private CustomImage defaultIcon;
        private CustomImage bgIcon;
        private CustomImage selectIcon;
        private CustomImage hoverIcon;
        private Sprite defaultSprite;

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
            CreateBasicButton(images);
            Label = new CustomText(UIGameObject, "label", ""); //empty button label
            Label.SetScale(Vector3.one);
            Label.SetSize(1f);
            ActivateCustomUI(true);
            EnableButton(true);
        }

        private void CreateBasicButton(Sprite[] images)
        {
            if (images.Length > 3)
            {
                hasBackgroundIcon = true;
                bgIcon = new CustomImage(UIGameObject, "Bg icon", images[3]);
            }
            else
            {
                hasBackgroundIcon = false;
            }
            if (images.Length > 2) selectIcon = new CustomImage(UIGameObject, "Select icon", images[2]);
            if (images.Length > 1) hoverIcon = new CustomImage(UIGameObject, "Hover icon", images[1]);

            defaultSprite = images[0];
            defaultIcon = new CustomImage(UIGameObject, "default btn icon", defaultSprite);
            defaultIcon.SetLocalPosition(new Vector3(0, 0, -20));
            SetButtonState(ButtonState.DEFAULT);
            SetScale(Vector3.one);
        }

        public Vector2 GetBtnSize()
        {
            return defaultIcon.GetSize();
        }
        public override void SetSize(float scaledWidth)
        {
            defaultIcon.SetSize(scaledWidth);
            if (hoverIcon != null) { hoverIcon.SetSize(scaledWidth); }
            if (selectIcon != null) { selectIcon.SetSize(scaledWidth); }
            if(bgIcon != null) { bgIcon.SetSize(scaledWidth); }
        }
        public void SetDefaultBtnColor(Color color)
        {
            defaultIcon.SetColor(color);
        }

        public void RemoveButtonIcon(CustomImage image)
        {
            image.SetSprite(null);
        }
        public override void ActivateCustomUI(bool value)
        {
            base.ActivateCustomUI(value);
            IsActive = value;
        }
        public bool IsHovered()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 btnPos = defaultIcon.UIGameObject.transform.position;
            Vector3 btnScale = defaultIcon.GetSpriteRenderer().bounds.extents;

            bool isInBoundsX = btnPos.x - btnScale.x < mousePos.x && btnPos.x + btnScale.x > mousePos.x;
            bool isInBoundsY = btnPos.y - btnScale.y < mousePos.y && btnPos.y + btnScale.y > mousePos.y;

            return isInBoundsX && isInBoundsY && IsEnabled && IsActive;
        }

        private void SetButtonState(ButtonState type)
        {
            switch (type)
            {
                default:
                case ButtonState.DEFAULT:
                    // not selected and not hovered
                    if(hoverIcon != null ) hoverIcon.ActivateCustomUI(false);
                    if(selectIcon != null)
                    {
                        isSelected = false;
                        selectIcon.ActivateCustomUI(false);
                    }     
                    break;
                case ButtonState.HOVERED:
                    // selected or not and hovered
                    if (hoverIcon != null) hoverIcon.ActivateCustomUI(true);
                    break;
                case ButtonState.SELECTED:
                    // selected and not hovered
                    if( selectIcon !=  null)
                    {
                        isSelected = true;
                        selectIcon.ActivateCustomUI(true);
                    }
                    break;
            }
        }

        public void ReplaceImgageOnHover()
        {
            if (IsHovered())
            {
                SetButtonState(ButtonState.HOVERED);
            }
            else if (!IsHovered())
            {
                SetButtonState(isSelected ? ButtonState.SELECTED : ButtonState.DEFAULT);
            }
        }

        public void SetButtonSelect(bool value)
        {
            SetButtonState(value ? ButtonState.SELECTED : ButtonState.DEFAULT);
        }
        public void ResetButtonImages(Sprite[] newImages)
        {
            CreateBasicButton(newImages);
        }

        public void SetVisibleInsideMask()
        {
            SpriteRenderer[] SRs = UIGameObject.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in SRs)
            {
                sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }
        }
        public void EnableButton(bool value)
        {
            IsEnabled = value;
            if (value)
            {
                defaultIcon.SetColor(new Color(1f, 1f, 1f, 1f));
                if(hasBackgroundIcon) bgIcon.SetColor(new Color(1f, 1f, 1f, 1f));
            }
            else
            {
                if(isSelected) { SetButtonState(ButtonState.DEFAULT); }
                defaultIcon.SetColor(new Color(1f, 1f, 1f, 0.4f));
                if (hasBackgroundIcon) bgIcon.SetColor(new Color(1f, 1f, 1f, 0.4f));
            }
        }

    }
}
