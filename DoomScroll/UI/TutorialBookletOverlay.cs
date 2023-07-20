using System.Reflection;
using UnityEngine;
using Doom_Scroll.Common;

namespace Doom_Scroll.UI
{
    //a static container for a set of methods operating on input parameters without having to get or set any internal instance fields.
    public static class TutorialBookletOverlay
    {
        private static Vector2 buttonSize = new Vector2(1.5f, 1.5f);
        public static CustomButton CreateTutorialBookletBtn(GameObject parent)
        {
            Vector3 pos = parent.transform.localPosition;
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector2 size = sr ? sr.size : new Vector2(1f, 1f);
            Vector3 position = new(pos.x+size.x*3.7f, pos.y + size.y * 5f, pos.z);
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] btnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.tutorialBookletToggle.png", slices);
            CustomButton tutorialBookletBtn = new CustomButton(parent, "TutorialBookletToggleButton", btnSprites, position, size.x);
            tutorialBookletBtn.ActivateCustomUI(false);
            return tutorialBookletBtn;
        }


        public static CustomModal CreateTutorialBookletOverlay(GameObject parent)
        {
            SpriteRenderer gameRoomButtonSR = GameObject.Find("GameRoomButton").GetComponent<SpriteRenderer>();
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.folderOverlay.png");

            // create the overlay background
            CustomModal tutorialBookletOverlay = new CustomModal(parent, "TutorialBookletOverlay", spr);
            tutorialBookletOverlay.SetLocalPosition(new Vector3(0f, 3f, -50f));
            tutorialBookletOverlay.SetScale(parent.transform.localScale * 0.5f);
            // deactivate by default
            tutorialBookletOverlay.ActivateCustomUI(false);
            if (gameRoomButtonSR != null)
            {
                tutorialBookletOverlay.SetSize(gameRoomButtonSR.size * 0.9f);
                tutorialBookletOverlay.SetLocalPosition(new Vector3(-0.65f, 3f, -30));
            }
            else
            {
                tutorialBookletOverlay.SetScale(parent.transform.localScale * 0.3f);
            }


            return tutorialBookletOverlay;
        }

        public static CustomButton AddCloseButton(GameObject parent)
        {
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector3 position = new Vector3(-sr.size.x / 2 - buttonSize.x / 2, sr.size.y / 2 - buttonSize.y / 2, -5f);
            Sprite[] closeBtnImg = { ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.closeButton.png") };
            return new CustomButton(parent, "Close Overlay", closeBtnImg, position, buttonSize.x);
        }
        public static CustomButton AddLeftButton(GameObject parent)
        {
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] backBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.backButton.png", slices);
            Vector3 backPosition = new Vector3(sr.size.x*0.4f, 4.5f, -5f);
            return new CustomButton(parent, "Flip to prior page", backBtnImg, backPosition, buttonSize.x);

        }
        public static CustomButton AddRightButton(GameObject parent)
        {
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] backBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.backButton.png", slices);
            Vector3 forwardPosition = new Vector3(-sr.size.x*0.4f, 4.5f, -5f);
            return new CustomButton(parent, "Flip to next page", backBtnImg, forwardPosition, -buttonSize.x);

        }

    }
}
