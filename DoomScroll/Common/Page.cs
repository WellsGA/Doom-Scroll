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
        //public GameObject Dir { get; private set; }
        //public string Path { get; private set; }
        //public CustomButton Btn { get; set; }
        public CustomText titleText { get; private set; }
        public Sprite tutorialPicture { get; private set; }
        public CustomModal image { get; private set; }
        public CustomText descriptionText { get; private set; }

        public Page(string title, string imgName, string description, CustomModal parentOverlay)
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

            //set image;
            tutorialPicture = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), imgName); //will appear as default file icon for now
            image = new CustomModal(parentOverlay.UIGameObject, $"TutorialPicture{title}", tutorialPicture);
            image.SetSize(parentOverlay.GetSize()*0.75f);
            image.SetLocalPosition(new Vector3(0f, 0f, -50f));

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
            image.ActivateCustomUI(true);
            descriptionText.ActivateCustomUI(true);
            DoomScroll._log.LogInfo("Opening page");
        }
        public void HidePage()
        {
            titleText.ActivateCustomUI(false);
            image.ActivateCustomUI(false);
            descriptionText.ActivateCustomUI(false);
            DoomScroll._log.LogInfo("Closing page");
        }
    }
}
