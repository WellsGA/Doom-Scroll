using UnityEngine;
using Doom_Scroll.UI;

namespace Doom_Scroll.Common
{
    //currently, maximum posts/tasks displayable is 9 (formatting looks good) or 10 (technically doesn't go out of folder window, but maybe cutting it too close)
    public enum FileTextType
    {
        TASKS,
        NEWS,
        SOURCES
    }
    public class FileText : File
    {
        public static int maxNumTextItems = 9; // Same for every folder. Maximum number of test-based evidence items (i.e. posts or tasks).
        private FileTextType m_type;
        private string m_content;


        public FileText(string parentPath, string name, CustomUI parentPanel, FileTextType textType) : base(parentPath, name, parentPanel)
        {
            m_type = textType;     
            Btn.Label.SetLocalPosition(new Vector3(0, 0.3f, 0));
            Btn.Label.SetSize(2.1f);
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
                        HeadlineDisplay.Instance.DisplayHeadlineInFolder();
                        m_content = HeadlineDisplay.Instance.ToString();
                        break;
                    }
                case FileTextType.SOURCES:
                    {
                        FolderManager.Instance.Sources.DispaySources();
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
                        //TaskAssigner.Instance.HidePageButtons();
                        break;
                    }
                case FileTextType.NEWS:
                    {
                        HeadlineDisplay.Instance.HideNews();
                        //HeadlineDisplay.Instance.HidePageButtons();
                        break;
                    }
                case FileTextType.SOURCES:
                    {
                        FolderManager.Instance.Sources.HideSources();
                        break;
                    }
            }
        }

    }
}

