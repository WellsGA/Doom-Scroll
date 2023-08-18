using System;
using System.Linq;
using System.Reflection;
using Doom_Scroll.UI;
using Hazel;
using UnityEngine;
using static Doom_Scroll.UI.CustomButton;

namespace Doom_Scroll.Common
{
    public class Page
    {
        private static Vector2 buttonSize = new Vector2(1.5f, 1.5f);
        //public GameObject Dir { get; private set; }
        //public string Path { get; private set; }
        //public CustomButton Btn { get; set; }
        public CustomText titleText { get; private set; }
        public Sprite tutorialPictureOne { get; private set; }
        public Sprite tutorialPictureTwo { get; private set; }
        public CustomModal imageOne { get; private set; }
        public CustomModal imageTwo { get; private set; }
        public CustomText descriptionText { get; private set; }

        public Page(string title, string imgName, string description, CustomModal parentOverlay, string secondImgName = null)
        {
            /*Dir = new GameObject(name);
            Dir.layer = LayerMask.NameToLayer("UI");
            Dir.transform.SetParent(parentOverlay.UIGameObject.transform);
            Dir.transform.localScale = Vector3.one;*/

            //set titleUI text
            titleText = new CustomText(parentOverlay.UIGameObject, $"TitleText{title}", $"<b>{title}</b>");
            titleText.SetColor(Color.black);
            titleText.SetSize(5f);
            Vector3 titleTextPos = new Vector3(0, parentOverlay.GetSize().y+1f, -10);
            titleText.SetLocalPosition(titleTextPos);
            titleText.TextMP.m_enableWordWrapping = false;

            //set description text
            descriptionText = new CustomText(parentOverlay.UIGameObject, $"DescriptionText{title}", description);
            descriptionText.SetColor(Color.black);
            descriptionText.SetSize(4f);
            Vector3 descTextPos = new Vector3(0, parentOverlay.GetSize().y-8f, -10);
            descriptionText.SetLocalPosition(descTextPos);
            descriptionText.TextMP.m_enableWordWrapping = false;
            Assembly assembly = Assembly.GetExecutingAssembly();

            if (secondImgName == null)
            {
                //set image;
                tutorialPictureOne = ImageLoader.ReadImageFromAssembly(assembly, imgName); //will appear as default file icon for now
                imageOne = new CustomModal(parentOverlay.UIGameObject, $"TutorialPictureOne{title}", tutorialPictureOne);
                imageOne.SetSize(parentOverlay.GetSize().x); // square ratio
                imageOne.SetLocalPosition(new Vector3(0f, 0f, -50f));
            }
            else
            {
                //set first image;
                tutorialPictureOne = ImageLoader.ReadImageFromAssembly(assembly, imgName); //will appear as default file icon for now
                imageOne = new CustomModal(parentOverlay.UIGameObject, $"TutorialPictureOne{title}", tutorialPictureOne);
                imageOne.SetSize(new Vector3(parentOverlay.GetSize().x*1.5f, parentOverlay.GetSize().x)); // landscape 2:3 ratio
                imageOne.SetLocalPosition(new Vector3(-3.8f, 0f, -50f));
                //set second image;
                tutorialPictureTwo = ImageLoader.ReadImageFromAssembly(assembly, secondImgName); //will appear as default file icon for now
                imageTwo = new CustomModal(parentOverlay.UIGameObject, $"TutorialPictureTwo{title}", tutorialPictureTwo);
                imageTwo.SetSize(new Vector3(parentOverlay.GetSize().x*1.5f, parentOverlay.GetSize().x)); // landscape 2:3 ratio
                imageTwo.SetLocalPosition(new Vector3(3.8f, 0f, -50f));
            }

            //Btn.SetLocalPosition(Vector3.zero);
            /*
            Btn = new CustomButton(Dir, name, file);
            Btn.ActivateCustomUI(false);
            Btn.ButtonEvent.MyAction += DisplayPage;
            Btn.Label.SetText(name);
            */
            HidePage();
        }

        public void DisplayPage()
        {
            titleText.ActivateCustomUI(true);
            imageOne.ActivateCustomUI(true);
            if (imageTwo != null) { imageTwo.ActivateCustomUI(true); }
            descriptionText.ActivateCustomUI(true);
            DoomScroll._log.LogInfo("Opening page");
        }
        public void HidePage()
        {
            titleText.ActivateCustomUI(false);
            imageOne.ActivateCustomUI(false);
            if (imageTwo != null) { imageTwo.ActivateCustomUI(false); }
            //UnityEngine.Object.Destroy(imageOne.UIGameObject);
            //if (imageTwo != null) { UnityEngine.Object.Destroy(imageTwo.UIGameObject); }
            descriptionText.ActivateCustomUI(false);
            DoomScroll._log.LogInfo("Closing page");
        }
        public static CustomButton AddLeftButton(GameObject parent, bool inFolderSystem)
        {
            float yPos = inFolderSystem ? -0.3f : 4.5f;
            Vector2 customButtonSize = inFolderSystem ? buttonSize * 0.4f : buttonSize;
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] backBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.backButton.png", slices);
            Vector3 backPosition = new Vector3(-sr.size.x*0.41f, yPos, -5f);
            return new CustomButton(parent, "Flip to prior page", backBtnImg, backPosition, customButtonSize.x);

        }
        public static CustomButton AddRightButton(GameObject parent, bool inFolderSystem)
        {
            float yPos = inFolderSystem ? -0.3f : 4.5f;
            Vector2 customButtonSize = inFolderSystem ? buttonSize * 0.4f : buttonSize;
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] backBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.backButton.png", slices);
            Vector3 forwardPosition = new Vector3(sr.size.x*0.41f, yPos, -5f);
            return new CustomButton(parent, "Flip to next page", backBtnImg, forwardPosition, -customButtonSize.x);

        }
    }
}
