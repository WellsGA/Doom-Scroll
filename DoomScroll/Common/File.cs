using System;
using System.Linq;
using System.Reflection;
using Doom_Scroll.UI;
using Hazel;
using UnityEngine;

namespace Doom_Scroll.Common
{
    public class File : IDirectory
    { 
        public GameObject Dir { get; private set; }
        public string Path { get; private set; }
        public CustomButton Btn { get; set; }

        public File(string parentPath, string name, CustomModal parentPanel)
        {
            // default file icon
            Sprite[] file = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.file.png", ImageLoader.slices2);

            Path = parentPath + "/" + name;

            Dir = new GameObject(name);
            Dir.layer = LayerMask.NameToLayer("UI");
            Dir.transform.SetParent(parentPanel.UIGameObject.transform);
            Dir.transform.localScale = Vector3.one;

            Btn = new CustomButton(Dir, name, file);
            //Btn.SetLocalPosition(Vector3.zero);
            Btn.ActivateCustomUI(false);
            Btn.ButtonEvent.MyAction += DisplayContent;
            Btn.Label.SetText(name);
        }
    
        public virtual void DisplayContent() { }
        public virtual void HideContent() { }
        public string PrintDirectory()
        {
            return " " + Path + " [file]";
        }
    }
}
