using System.Reflection;
using UnityEngine;
using Doom_Scroll.Common;

namespace Doom_Scroll.UI
{
    //a static container for a set of methods operating on input parameters without having to get or set any internal instance fields.
    public static class FolderOverlay
    {
        private static Vector2 buttonSize = new Vector2(0.5f, 0.5f);
        public static CustomButton CreateFolderBtn(GameObject parent)
        {
            GameObject keyboardBtn = HudManager.Instance.Chat.OpenKeyboardButton;
            Vector3 pos = keyboardBtn.transform.localPosition;
            SpriteRenderer sr = keyboardBtn.GetComponent<SpriteRenderer>();
            Vector2 size = sr ? sr.size : new Vector2(0.5f, 0.5f);
            Vector3 position = new(pos.x, pos.y + size.y * 2 + 0.05f, pos.z);
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] btnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.folderToggle.png", slices);
            CustomButton folderBtn = new CustomButton(parent, "FolderToggleButton", btnSprites, position, size.x);
            folderBtn.ActivateCustomUI(false);
            return folderBtn;
        }

        public static CustomModal CreateFolderOverlay(GameObject parent)
        {
            SpriteRenderer backgroundSR = parent.transform.Find("Background").GetComponent<SpriteRenderer>();
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.folderOverlay.png");

            // create the overlay background
            CustomModal folderOverlay = new CustomModal(parent, "FolderOverlay", spr);  
            folderOverlay.SetLocalPosition(new Vector3(0f, 0f, -10f));       
            if (backgroundSR != null)
            {
                folderOverlay.SetSize(backgroundSR.size);
            }
            else
            {
                folderOverlay.SetScale(parent.transform.localScale * 0.4f);
            }
            return folderOverlay;
        }

        public static CustomButton AddCloseButton(GameObject parent)
        {
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector3 position = new Vector3(-sr.size.x / 2 - buttonSize.x / 2, sr.size.y / 2 - buttonSize.y / 2, -5f);
            Sprite[] closeBtnImg = { ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.closeButton.png") };
            return new CustomButton(parent, "Close FolderOverlay", closeBtnImg, position, buttonSize.x);
        }
        public static CustomButton AddHomeButton(GameObject parent)
        {
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] homeBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.homeButton.png", slices);
            Vector3 homePosition = new Vector3(-sr.size.x / 2 + buttonSize.x + 0.2f, sr.size.y / 2 - buttonSize.y, -5f);
            return new CustomButton(parent, "Back to Home", homeBtnImg, homePosition, buttonSize.x);

        }
        public static CustomButton AddBackButton(GameObject parent)
        {
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] backBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.backButton.png", slices);
            Vector3 backPosition = new Vector3(-sr.size.x / 2 + 2 * buttonSize.x + 0.2f, sr.size.y / 2 - buttonSize.y, -5f);
            return new CustomButton(parent, "Back to Previous", backBtnImg, backPosition, buttonSize.x);

        }
        public static CustomText AddPath(GameObject parent)
        {
            CustomText pathText = new CustomText(parent, "PathName", "Home");
            pathText.SetLocalPosition(new Vector3(0, 0.3f, -10));
            pathText.SetSize(1.6f);
            return pathText;
        }
    }
}
