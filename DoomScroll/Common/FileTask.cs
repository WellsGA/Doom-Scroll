using System.Reflection;
using UnityEngine;
using Doom_Scroll.UI;

namespace Doom_Scroll.Common
{
    internal class FileTask : File
    {
        private CustomText m_content;
        private GameObject m_parent;
        private Vector2 m_parentSize;

        public FileTask(string parentPath, string name, GameObject parentPanel) : base(parentPath, name, parentPanel)
        {
            m_parent = parentPanel;
            m_parentSize = parentPanel.GetComponent<SpriteRenderer>().size;
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] openBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.openButton.png", slices);
            Btn = new CustomButton(Dir, name, openBtnImg);
            Btn.SetLocalPosition(Vector3.zero);
            Btn.ActivateCustomUI(false);
            Btn.ButtonEvent.MyAction += DisplayContent;
        }

        public override void DisplayContent()
        {
            m_content = new CustomText(m_parent, "assigned tasks", TaskAssigner.Instance.ToString());
            m_content.SetLocalPosition(new Vector3(0, m_parentSize.y/2 - 1.5f, -10));
            m_content.SetSize(1.6f);
            m_content.ActivateCustomUI(true);

            TaskAssigner.Instance.DisplayAssignedTasks();  // debug purposes
        }
        public override void HideContent()
        {
            m_content.ActivateCustomUI(false);
        }

    }
}

