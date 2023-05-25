using System.Reflection;
using UnityEngine;
using Doom_Scroll.UI;
using System;
using static Il2CppMono.Security.X509.X520;

namespace Doom_Scroll.Common
{
    internal class FileTask : File
    {
        //button
        private CustomButton m_close;

        //modal
        private bool m_isTaskOverlayOpen;
        private CustomModal m_taskModal;

        public FileTask(string parentPath, string name, GameObject parentPanel) : base(parentPath, name, parentPanel)
        {
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] openBtnImg = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.openButton.png", slices);
            Btn = new CustomButton(Dir, name, openBtnImg);
            Btn.SetLocalPosition(Vector3.zero);
            Btn.ActivateCustomUI(false);
            Btn.ButtonEvent.MyAction += DisplayContent;

            // create display panel and add text and close button
            Sprite panel = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.panel.png");

            // create the overlay background
            m_taskModal = new CustomModal(parentPanel, "FolderOverlay", panel);
            m_taskModal.SetLocalPosition(new Vector3(0f, 0f, -70f));
            // add close button
            m_close = FolderOverlay.AddCloseButton(m_taskModal.UIGameObject);
            // deactivate (hide) modal
            m_taskModal.ActivateCustomUI(false);
            m_isTaskOverlayOpen = false;
        }

        public override void DisplayContent()
        {
            CustomText taskList = new CustomText(m_taskModal.UIGameObject, "assigned tasks", TaskAssigner.Instance.ToString());
            taskList.SetLocalPosition(new Vector3(0, 0.5f, -10));
            taskList.SetSize(1.6f);
            m_taskModal.ActivateCustomUI(true);
            m_isTaskOverlayOpen = true;
            TaskAssigner.Instance.DisplayAssignedTasks();  // debug purposes
        }
         
        public void CheckForCloseFile()
        {
            if (m_isTaskOverlayOpen) // we can close if open
            {
                if (m_close.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    m_taskModal.ActivateCustomUI(false);
                    m_isTaskOverlayOpen = false;
                    DoomScroll._log.LogInfo("TASK MODAL CLOSED");
                }
            }
        }

    }
}

