using System.Reflection;
using UnityEngine;
using Doom_Scroll.UI;

namespace Doom_Scroll.Common
{
    internal class FileText : File
    {

        public FileText(string parentPath, string name, GameObject parentPanel) : base(parentPath, name, parentPanel)
        {

            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] openBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.openButton.png", slices);
            Btn = new CustomButton(Dir, name, openBtnImg);
            Btn.SetLocalPosition(Vector3.zero);
            Btn.ActivateCustomUI(false);
            Btn.ButtonEvent.MyAction += DisplayContent;

            // create display panel and add text and close button

        }

        public override void DisplayContent()
        {
            HudManager.Instance.ShowPopUp("BLA BLA");
            TaskAssigner.Instance.DisplayAssignedTasks();  // debug purposes
        }
    }
}

