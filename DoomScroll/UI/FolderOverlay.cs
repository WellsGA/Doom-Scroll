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
            PassiveButton[] srs = parent.gameObject.GetComponentsInChildren<PassiveButton>(true);
            foreach (PassiveButton r in srs)
            {
                DoomScroll._log.LogInfo(r.gameObject.transform.parent.name); 
                DoomScroll._log.LogInfo(r.gameObject.transform.parent.transform.parent.name);

            }
            GameObject chatScreen = parent.transform.Find("ChatScreenRoot/ChatScreenContainer").gameObject;
            GameObject banButton = parent.GetComponentInChildren<BanMenu>(true).gameObject;
            Vector3 pos = banButton.transform.localPosition;
            SpriteRenderer sr = banButton.GetComponent<SpriteRenderer>();
            Vector2 size = sr ? sr.size * 2 : new Vector2(.85f, .85f);
            Vector3 position = new(pos.x, pos.y + size.y / 2 + 0.05f, pos.z);
            Sprite[] btnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.folderToggle.png", ImageLoader.slices2);
            CustomButton folderBtn = new CustomButton(chatScreen, "FolderToggleButton", btnSprites, position, size.x);
            folderBtn.ActivateCustomUI(false);
            return folderBtn;
        }

        public static CustomModal CreateFolderOverlay(GameObject parent, CustomButton toggler)
        {
            GameObject cahtScreen = parent.transform.Find("ChatScreenRoot/ChatScreenContainer").gameObject;
            GameObject bg = cahtScreen.transform.Find("Background").gameObject;
            SpriteRenderer backgroundSR = bg.GetComponent<SpriteRenderer>();
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.folderOverlay.png");

            // create the overlay background
            CustomModal folderOverlay = new CustomModal(cahtScreen, "FolderOverlay", spr, toggler, true);  
            folderOverlay.SetLocalPosition(new Vector3(0f, 0f, -10f));       
            if (backgroundSR != null)
            {
               folderOverlay.SetSize(backgroundSR.size * 0.85f);
               folderOverlay.SetLocalPosition(new Vector3(-0.65f,0,-10));
            }
            else
            {
                folderOverlay.SetScale(parent.transform.localScale * 0.5f);
            }
            return folderOverlay;
        }

        public static CustomButton AddHomeButton(CustomModal parent)
        {
            Vector2 size = parent.GetSize();
            Sprite[] homeBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.homeButton.png", ImageLoader.slices2);
            Vector3 homePosition = new Vector3(-size.x / 2 + buttonSize.x + 0.2f, size.y / 2 - buttonSize.y, -5f);
            return new CustomButton(parent.UIGameObject, "Back to Home", homeBtnImg, homePosition, buttonSize.x);

        }
        public static CustomButton AddBackButton(CustomModal parent)
        {
            Vector2 size = parent.GetSize();
            Sprite[] backBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.backButton.png", ImageLoader.slices2);
            Vector3 backPosition = new Vector3(-size.x / 2 + 2 * buttonSize.x + 0.2f, size.y / 2 - buttonSize.y, -5f);
            return new CustomButton(parent.UIGameObject, "Back to Previous", backBtnImg, backPosition, buttonSize.x);

        }
        public static CustomText AddPath(GameObject parent)
        {
            CustomText pathText = new CustomText(parent, "PathName", "Home");
            pathText.SetLocalPosition(new Vector3(0, 2f, -10));
            pathText.SetSize(2.6f);
            return pathText;
        }

    }
}
