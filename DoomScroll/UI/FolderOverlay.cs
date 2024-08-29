﻿using System.Reflection; 
using UnityEngine;
using Doom_Scroll.Common;
// using System.Collections.Generic;
using Il2CppSystem.Collections.Generic;

namespace Doom_Scroll.UI
{
    //a static container for a set of methods operating on input parameters without having to get or set any internal instance fields.
    public static class FolderOverlay
    {
        private static Vector2 buttonSize = new Vector2(0.4f, 0.4f);
        public static CustomButton CreateFolderBtn(GameObject parent)
        { 
             // GameObject chatUI = parent.transform.Find("ChatUI").gameObject;
            GameObject mapButton = HudManager.Instance.MapButton.gameObject;
            GameObject chatParent =  parent;
            Vector2 size = new Vector2(.85f, .85f);
            if (mapButton != null)
            {
                chatParent = mapButton;
                SpriteRenderer sr = mapButton.transform.Find("Background").GetComponent<SpriteRenderer>();
                if(sr != null) size = sr.size * 0.64f;
            }
            Vector3 pos = chatParent.transform.localPosition;
            Vector3 position = new(-2f,0,0);
            Sprite[] btnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.folderToggle.png", ImageLoader.slices2);
            CustomButton folderBtn = new CustomButton(chatParent, "FolderToggleButton", btnSprites, position, size.x);
            folderBtn.ActivateCustomUI(false);

           /* Component[] components = HudManager.Instance.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                DoomScroll._log.LogInfo(component.ToString());
            }*/
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
            folderOverlay.SetLocalPosition(new Vector3(0f, 0f, -30f));       
            if (backgroundSR != null)
            {
               folderOverlay.SetSize(backgroundSR.size.x * 0.9f);
               folderOverlay.SetLocalPosition(new Vector3(-0.65f, backgroundSR.size.y /2 - folderOverlay.GetSize().y /2 - 0.3f, -30));
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
        public static CustomText AddPath(CustomModal parent)
        {
            CustomText pathText = new CustomText(parent.UIGameObject, "PathName", "Home");
            pathText.SetLocalPosition(new Vector3(0, parent.GetSize().y /2 -0.45f, -10));
            pathText.SetSize(1.9f);
            return pathText;
        }

    }
}
