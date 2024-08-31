using Doom_Scroll.Common;
using UnityEngine;
using System.Reflection;

namespace Doom_Scroll.UI
{
    public class CustomModal : CustomUI
    {
        // inherits UIGameObject from base
        private SpriteRenderer modalBackground;
        public CustomButton CloseButton { get; private set; }
        public CustomButton ModalToggler { get; private set; }
        public bool IsModalOpen { get; private set; }
        public CustomModal(GameObject parent, string name, Sprite modal, CustomButton toggler, bool hasCloseButton) : base(parent, name)
        {
            modalBackground = UIGameObject.AddComponent<SpriteRenderer>();
            modalBackground.drawMode = SpriteDrawMode.Sliced;
            modalBackground.sprite = modal;
            SetScale(Vector3.one);
            ModalToggler = toggler;
            ModalToggler.ButtonEvent.MyAction += ToggleModal;
            if (hasCloseButton)
            {
                CloseButton = AddCloseButton(0.45f);
                CloseButton.ButtonEvent.MyAction += DeactivateModal;
            }
            DeactivateModal();
        }

        public override void SetSize(float scaledWidth)
        {
            modalBackground.size = new Vector2(scaledWidth, modalBackground.sprite.rect.height * scaledWidth / modalBackground.sprite.rect.width);
            //reposition close button
            if(CloseButton != null)
            {
                CloseButton.SetLocalPosition(new Vector3(-modalBackground.size.x / 2, modalBackground.size.y / 2, -10));
            }
        }

        public void SetSize(Vector3 size)
        {
            modalBackground.size = size;
            if (CloseButton != null)
            {
                CloseButton.SetLocalPosition(new Vector3(-size.x / 2, size.y / 2, -10));
            }
        }

        public override Vector2 GetSize()
        {
            return modalBackground.size;
        }

        public void ListenForButtonClicks()
        {
            if (ModalToggler.IsHovered() && ModalToggler.IsActive && ModalToggler.IsEnabled && Input.GetKeyUp(KeyCode.Mouse0))
            {
                ModalToggler.ButtonEvent.InvokeAction();
            }
            if (CloseButton == null) return;
            if (CloseButton.IsHovered() && CloseButton.IsActive && IsModalOpen && Input.GetKeyUp(KeyCode.Mouse0))
            {
                CloseButton.ButtonEvent.InvokeAction();
            }
        }

        private void DeactivateModal()
        {
            ActivateCustomUI(false);
            IsModalOpen = false;
        }

        private void ToggleModal()
        {
            IsModalOpen = !IsModalOpen;
            ActivateCustomUI(IsModalOpen);
            DoomScroll._log.LogInfo("MODAL TOGGLE: " + IsModalOpen);
        }
        
        public void SetIsOpen(bool value)
        {
            IsModalOpen = value;
        }

        private CustomButton AddCloseButton(float buttonSize)
        {
            Vector2 size = modalBackground.size;
            Vector3 position = new Vector3(-size.x /2, size.y / 2, -10);

            Sprite[] closeBtnImg = { ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.closeButton.png") };
            return new CustomButton(UIGameObject, "Close modal", closeBtnImg, position, buttonSize);
        }
    }
}
