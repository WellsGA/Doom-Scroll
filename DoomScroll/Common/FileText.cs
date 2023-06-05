using System.Reflection;
using UnityEngine;
using Doom_Scroll.UI;

namespace Doom_Scroll.Common
{
    enum FileTextType
    {
        TASKS,
        NEWS
    }
    internal class FileText : File
    {
        private FileTextType m_type;
        private CustomText m_content;
        private GameObject m_parent;
        private Vector2 m_parentSize;

        public FileText(string parentPath, string name, GameObject parentPanel, FileTextType textType) : base(parentPath, name, parentPanel)
        {
            m_type = textType;
            m_parent = parentPanel;
            m_parentSize = parentPanel.GetComponent<SpriteRenderer>().size;
            m_content = new CustomText(m_parent, "file content", "");
            m_content.SetLocalPosition(new Vector3(0, m_parentSize.y / 2 - 1.5f, -10));
            m_content.SetSize(1.4f);

            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] openBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.openButton.png", slices);
            Btn = new CustomButton(Dir, name, openBtnImg);
            Btn.SetLocalPosition(Vector3.zero);
            Btn.ActivateCustomUI(false);
            Btn.ButtonEvent.MyAction += DisplayContent;

            Label.SetLocalPosition(new Vector3(0, -8f, 0));
            Label.TextMP.fontSize = 4f;
        }

        public override void DisplayContent()
        {
            switch (m_type)
            {
                case FileTextType.TASKS:
                    {
                        m_content.TextMP.text = TaskAssigner.Instance.ToString();
                        break;
                    }
                case FileTextType.NEWS:
                    {
                        m_content.TextMP.text = NewsFeedManager.Instance.DisplayNews();
                        break;
                    }
            }
            m_content.ActivateCustomUI(true);
            TaskAssigner.Instance.DisplayAssignedTasks();  // debug purposes
        }
        public override void HideContent()
        {
            m_content.ActivateCustomUI(false);
        }

    }
}

