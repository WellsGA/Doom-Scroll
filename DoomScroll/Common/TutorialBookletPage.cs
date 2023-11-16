using System;
using System.Linq;
using System.Reflection;
using Doom_Scroll.UI;
using Hazel;
using UnityEngine;
using static Doom_Scroll.UI.CustomButton;

namespace Doom_Scroll.Common
{
    public class TutorialBookletPage: CustomUI
    {
        private static Vector2 buttonSize = new Vector2(1.5f, 1.5f);
        //public GameObject Dir { get; private set; }
        //public string Path { get; private set; }
        //public CustomButton Btn { get; set; }
        private CustomModal parentOverlay;
        public CustomText titleText { get; private set; }
        public Sprite tutorialPictureOne { get; private set; }
        public Sprite tutorialPictureTwo { get; private set; }
        public Sprite tutorialPictureThree { get; private set; }
        public CustomImage imageOne { get; private set; }
        public CustomImage imageTwo { get; private set; }
        public CustomImage imageThree { get; private set; }
        public CustomText descriptionText { get; private set; }

        public TutorialBookletPage(string title, string description, CustomModal parentOverlay, string imgName = null, string secondImgName = null, Tuple<string, float> iconButton = null) : base(parentOverlay.UIGameObject, title)
        {
            /*Dir = new GameObject(name);
            Dir.layer = LayerMask.NameToLayer("UI");
            Dir.transform.SetParent(parentOverlay.UIGameObject.transform);
            Dir.transform.localScale = Vector3.one;*/
            this.parentOverlay = parentOverlay;

            //set titleUI text
            titleText = new CustomText(parentOverlay.UIGameObject, $"TitleText{title}", $"<b>{title}</b>");
            titleText.SetColor(Color.black);
            titleText.SetSize(5f);
            Vector3 titleTextPos = new Vector3(0, parentOverlay.GetSize().y-8f, -10);
            titleText.SetLocalPosition(titleTextPos);
            titleText.TextMP.m_enableWordWrapping = false;

            //set description text
            descriptionText = new CustomText(parentOverlay.UIGameObject, $"DescriptionText{title}", description);
            descriptionText.SetColor(Color.black);
            descriptionText.SetSize(4f);
            Vector3 descTextPos = new Vector3(0, parentOverlay.GetSize().y-17f, -10);
            descriptionText.SetLocalPosition(descTextPos);
            descriptionText.TextMP.m_enableWordWrapping = false;
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (secondImgName == null)
            {
                if (imgName != null)
                {
                    //set image;
                    tutorialPictureOne = ImageLoader.ReadImageFromAssembly(assembly, imgName); //will appear as default file icon for now
                    imageOne = new CustomImage(parentOverlay.UIGameObject, $"TutorialPictureOne{title}", tutorialPictureOne);
                    imageOne.SetSize(parentOverlay.GetSize().x * 0.28f); // square ratio
                    imageOne.SetLocalPosition(new Vector3(0f, 1.5f, -50f));
                }
                else
                {
                    descriptionText.SetLocalPosition(new Vector3(0, parentOverlay.GetSize().y - 14f, -10));
                }
            }
            else
            {
                //set first image;
                tutorialPictureOne = ImageLoader.ReadImageFromAssembly(assembly, imgName); //will appear as default file icon for now
                imageOne = new CustomImage(parentOverlay.UIGameObject, $"TutorialPictureOne{title}", tutorialPictureOne);
                imageOne.SetSize(new Vector3(parentOverlay.GetSize().x*1.5f, parentOverlay.GetSize().x) * 0.28f); // landscape 2:3 ratio
                imageOne.SetLocalPosition(new Vector3(-3.8f, 0f, -50f));
                //set second image;
                tutorialPictureTwo = ImageLoader.ReadImageFromAssembly(assembly, secondImgName); //will appear as default file icon for now
                imageTwo = new CustomImage(parentOverlay.UIGameObject, $"TutorialPictureTwo{title}", tutorialPictureTwo);
                imageTwo.SetSize(new Vector3(parentOverlay.GetSize().x*1.5f, parentOverlay.GetSize().x) * 0.28f); // landscape 2:3 ratio
                imageTwo.SetLocalPosition(new Vector3(3.8f, 0f, -50f));
                if (iconButton != null)
                {
                    //set third image;
                    tutorialPictureThree = ImageLoader.ReadImageFromAssembly(assembly, iconButton.Item1); //will appear as default file icon for now
                    imageThree = new CustomImage(parentOverlay.UIGameObject, $"TutorialPictureThree{title}", tutorialPictureThree);
                    imageThree.SetSize(new Vector3(parentOverlay.GetSize().x, parentOverlay.GetSize().x * iconButton.Item2)*((2f - iconButton.Item2)/2.5f) * 0.28f); // uses provided ratio
                    imageThree.SetLocalPosition(new Vector3(0f, 3.5f, -50f));
                }
            }
            HidePage();
        }
        public override void SetSize(float size)
        {
            if (parentOverlay != null)
            {
                parentOverlay.SetSize(size);
            }
        }

        public override void ActivateCustomUI(bool value)
        {
            if (value)
            {
                DisplayPage();
            }
            else
            {
                HidePage();
            }

            UIGameObject.SetActive(value);
        }

        public void DisplayPage()
        {
            titleText.ActivateCustomUI(true);
            if (imageOne != null) { imageOne.ActivateCustomUI(true); }
            if (imageTwo != null) { imageTwo.ActivateCustomUI(true); }
            if (imageThree != null) { imageThree.ActivateCustomUI(true); }
            descriptionText.ActivateCustomUI(true);
            DoomScroll._log.LogInfo("Opening page");
        }
        public void HidePage()
        {
            titleText.ActivateCustomUI(false);
            if (imageOne != null) { imageOne.ActivateCustomUI(false); }
            if (imageTwo != null) { imageTwo.ActivateCustomUI(false); }
            if (imageThree != null) { imageThree.ActivateCustomUI(false); }
            //UnityEngine.Object.Destroy(imageOne.UIGameObject);
            //if (imageTwo != null) { UnityEngine.Object.Destroy(imageTwo.UIGameObject); }
            descriptionText.ActivateCustomUI(false);
            DoomScroll._log.LogInfo("Closing page");
        }
       
        /* public static CustomButton AddLeftButton(GameObject parent, bool inFolderSystem)
        {
            float yPos = inFolderSystem ? -0.3f : 4.5f;
            Vector2 customButtonSize = inFolderSystem ? buttonSize * 0.4f : buttonSize;
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Sprite[] backBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.backButton.png", ImageLoader.slices2);
            Vector3 backPosition = new Vector3(-sr.size.x*0.41f, yPos, -5f);
            return new CustomButton(parent, "Flip to prior page", backBtnImg, backPosition, customButtonSize.x);

        }
        public static CustomButton AddRightButton(GameObject parent, bool inFolderSystem)
        {
            float yPos = inFolderSystem ? -0.3f : 4.5f;
            Vector2 customButtonSize = inFolderSystem ? buttonSize * 0.4f : buttonSize;
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Sprite[] backBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.backButton.png", ImageLoader.slices2);
            Vector3 forwardPosition = new Vector3(sr.size.x*0.41f, yPos, -5f);
            return new CustomButton(parent, "Flip to next page", backBtnImg, forwardPosition, -customButtonSize.x);

        }*/
    }
}
