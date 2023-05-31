using Doom_Scroll.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doom_Scroll
{  
    // Singleton with static initialization: thread safe without explicitly coding for it,
    // relies on the common language runtime to initialize the variable
    public class NewsFeedManager
    {
        private static readonly NewsFeedManager _instance = new NewsFeedManager(); // readonly: can be assigned only during static initialization
        public static NewsFeedManager Instance
        {
            get
            {
                return _instance;
            }
        }

        private HudManager hudManagerInstance;

        private CustomModal m_inputPanel;
        private CustomButton m_togglePanelButton;
        private CustomButton m_submitButton;
        public bool IsInputpanelOpen { get; private set; }
        private bool canCreateNews;
        // list of news created randomly and by the selected players -  will be displayed during meetings
        private static List<AssignedTask> AssignedTasks;

        private NewsFeedManager()
        {
            IsInputpanelOpen = false;
            canCreateNews = false;
            hudManagerInstance = HudManager.Instance;
            InitializeInputPanel();
            DoomScroll._log.LogInfo("NEWS FEED MANAGER CONSTRUCTOR");
        }

        private void InitializeInputPanel()
        {
            m_togglePanelButton = NewsFeedOverlay.CreateNewsInputButton(hudManagerInstance);
            m_inputPanel = NewsFeedOverlay.InitInputOverlay(hudManagerInstance);
            m_submitButton = NewsFeedOverlay.CreateSubmitButton(m_inputPanel.UIGameObject);

        }
    }
}
