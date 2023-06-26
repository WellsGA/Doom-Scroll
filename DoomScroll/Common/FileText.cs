﻿using System.Reflection;
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
        private string m_content;
        private GameObject m_parent;
        private Vector2 m_parentSize;

        public FileText(string parentPath, string name, GameObject parentPanel, FileTextType textType) : base(parentPath, name, parentPanel)
        {
            m_type = textType;
            m_parent = parentPanel;
            m_parentSize = parentPanel.GetComponent<SpriteRenderer>().size;
            
            Btn.Label.SetLocalPosition(new Vector3(0, 0.4f, 0));
            Btn.Label.SetSize(2.5f);
        }

        public override void DisplayContent()
        {
            switch (m_type)
            {
                case FileTextType.TASKS:
                    {
                        TaskAssigner.Instance.DisplayAssignedTasks();
                        m_content = TaskAssigner.Instance.ToString();
                        break;
                    }
                case FileTextType.NEWS:
                    {
                        NewsFeedManager.Instance.DisplayNews();
                        break;
                    }
            }
            DoomScroll._log.LogInfo(m_content);  // debug purposes
        }
        public override void HideContent()
        {
            switch (m_type)
            {
                case FileTextType.TASKS:
                    {
                        TaskAssigner.Instance.HideAssignedTasks();
                        break;
                    }
                case FileTextType.NEWS:
                    {
                        m_content = NewsFeedManager.Instance.DisplayNews();
                        break;
                    }
            }
        }

    }
}

