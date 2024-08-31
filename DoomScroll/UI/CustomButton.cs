using Doom_Scroll.Common;
using UnityEngine;

namespace Doom_Scroll.UI
{
    public enum ButtonState
    {
        DEFAULT,
        HOVERED,
        SELECTED
    }
    public class CustomButton : CustomUI
    {
        // Creates and manages custom buttnos
        public DoomScrollEvent ButtonEvent = new DoomScrollEvent();
        // inherits UIGameObject from base         
        private bool isSelected;
        private bool isHovered;
        private bool hasBackgroundIcon;
        public bool IsEnabled { get; private set; }
        public bool IsActive { get; private set; }
        public CustomText Label { get; private set; }
        public CustomImage DefaultIcon { get; private set; }
        public CustomImage TopIcon { get; private set; }
        
        private CustomImage selectIcon;
        private CustomImage hoverIcon;
        private ButtonState state;
        private BoxCollider2D collider;

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
            DefaultIcon = new CustomImage(UIGameObject, "default btn icon", images[0]);

            if (images.Length > 2) selectIcon = new CustomImage(UIGameObject, "Select icon", images[2]);
            if (images.Length > 1) hoverIcon = new CustomImage(UIGameObject, "Hover icon", images[1]);
            if (images.Length > 3)
            {
                hasBackgroundIcon = true;
                TopIcon = new CustomImage(UIGameObject, "Bg icon", images[3]);
                TopIcon.SetLocalPosition(new Vector3(0, 0, -10));
            }
            else
            {
                hasBackgroundIcon = false;
            }
            ChangeButtonState(ButtonState.DEFAULT);
            SetScale(Vector3.one);
            collider = DefaultIcon.UIGameObject.AddComponent<BoxCollider2D>();
            // DoomScroll._log.LogInfo("BUTTON SIZE AND COLLIDER SIZE: " + DefaultIcon.GetSize() + ", " + collider.size);
        }

        public override Vector2 GetSize()
        {
            return DefaultIcon.GetSize();
        }
        public override void SetSize(float scaledWidth)
        {
            DefaultIcon.SetSize(scaledWidth);

            if (hoverIcon != null) { hoverIcon.SetSize(scaledWidth); }
            if (selectIcon != null) { selectIcon.SetSize(scaledWidth); }
            if (TopIcon != null) { TopIcon.SetSize(scaledWidth); }
            collider.size = DefaultIcon.GetSize();
        }
        public void SetSize(Vector2 size)
        {
            DefaultIcon.SetSize(size);
            if (hoverIcon != null) { hoverIcon.SetSize(size); }
            if (selectIcon != null) { selectIcon.SetSize(size); }
            if (TopIcon != null) { TopIcon.SetSize(size); }
            collider.size = DefaultIcon.GetSize();
        }
        public void SetButtonScale(Vector3 scale)
        {
            SetScale(scale);
            DefaultIcon.SetScale(scale);
            if (hoverIcon != null) { hoverIcon.SetScale(scale); }
            if (selectIcon != null) { selectIcon.SetScale(scale); }
            if (TopIcon != null) { TopIcon.SetScale(scale); }
        }
        public void SetDefaultBtnColor(CustomImage image, Color color)
        {
            image.SetColor(color);
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
            int layerObject = 1 << LayerMask.NameToLayer("UI");
            //  layerObject = ~layerObject;
            Vector2 ray = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, layerObject);
            return collider == hit.collider ? true : false;
        }

        private void ChangeButtonState(ButtonState type)
        {
            state = type;
            switch (state)
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
                    if (hoverIcon != null) hoverIcon.ActivateCustomUI(false);
                    if ( selectIcon !=  null)
                    {
                        isSelected = true;
                        selectIcon.ActivateCustomUI(true);
                    }
                    break;
            }
        }

        public void ReplaceImgageOnHover()
        {
            if (IsHovered() && !isHovered && IsEnabled)
            {
                isHovered = true;
                ChangeButtonState(ButtonState.HOVERED);
            }
            else if (!IsHovered() && isHovered)
            {
                isHovered = false;
                ChangeButtonState(isSelected ? ButtonState.SELECTED : ButtonState.DEFAULT);
            }
        }

        public void SelectButton(bool value)
        {
            ChangeButtonState(value ? ButtonState.SELECTED : ButtonState.DEFAULT);
        }

        public void ResetButtonImages(Sprite[] newImages)
        {
            DefaultIcon.SetSprite(newImages[0]);
            if (newImages.Length > 2)
            {
                if(selectIcon == null)
                {
                    selectIcon = new CustomImage(UIGameObject, "Select icon", newImages[2]);
                }
                else
                {
                    selectIcon.SetSprite(newImages[2]);
                }
            }
            if (newImages.Length > 1)
            {
                
                if (hoverIcon != null)
                {
                    Object.Destroy(hoverIcon.UIGameObject);
                }
                hoverIcon = new CustomImage(UIGameObject, "Hover icon", newImages[1]);
                hoverIcon.SetLocalPosition(new Vector3(0, 0, -10));
            }
            if (newImages.Length > 3)
            {
                if (TopIcon == null)
                {
                    TopIcon = new CustomImage(UIGameObject, "Top icon", newImages[3]);
                }
                else
                {
                    TopIcon.SetSprite(newImages[3]);
                }
                hasBackgroundIcon = true;          
                TopIcon.SetLocalPosition(new Vector3(0, 0, -10));
            }
            else
            {
                hasBackgroundIcon = false;
            }
            ChangeButtonState(ButtonState.DEFAULT);
        }

        public void SetVisibleInsideMask()
        {
            SpriteRenderer[] SRs = UIGameObject.GetComponentsInChildren<SpriteRenderer>(true);
            //MeshRenderer[] MRs = UIGameObject.GetComponentsInChildren<MeshRenderer>();
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
                DefaultIcon.SetColor(new Color(1f, 1f, 1f, 1f));
                if(hasBackgroundIcon) TopIcon.SetColor(new Color(1f, 1f, 1f, 1f));
            }
            else
            {
                if(isSelected) { ChangeButtonState(ButtonState.DEFAULT); }
                DefaultIcon.SetColor(new Color(1f, 1f, 1f, 0.4f));
                if (hasBackgroundIcon) TopIcon.SetColor(new Color(1f, 1f, 1f, 0.4f));
            }
        }

    }
}
