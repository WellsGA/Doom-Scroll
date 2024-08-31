using Doom_Scroll.Common;
using Doom_Scroll.UI;
using HarmonyLib;
using UnityEngine;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(HudManager))]
    public static class HudManagerPatch
    {
        public static CustomButton m_tutorialModeToggleBtn;
        private static Tooltip taskPanelSWCTooltip;

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(HudManager __instance)
        {
            // Reset Tooltip List
            Tooltip.ResetCurrentTooltips();
            ScreenshotManager.Instance.Reset();
            FolderManager.Instance.Reset();
            TaskAssigner.Instance.Reset();
            HeadlineManager.Instance.Reset();
            HeadlineDisplay.Instance.Reset();
            SecondaryWinConditionManager.Reset();
            TutorialBookletManager.Instance.Reset();

            // Tutorial Mode toggle button
            m_tutorialModeToggleBtn = Tooltip.CreateTutorialModeToggleBtn(__instance.SettingsButton, new Vector3(-10.6f, -6.6f, 0));
            m_tutorialModeToggleBtn.ButtonEvent.MyAction += ToggleTutorialButtonSelected;
            // m_tutorialModeToggleBtn.ButtonEvent.MyAction += FolderManager.RectifyFolderTooltips;
            m_tutorialModeToggleBtn.SelectButton(Tooltip.TutorialModeOn);
            DoomScroll._log.LogInfo("Button event added to button.");
            m_tutorialModeToggleBtn.EnableButton(true);
            m_tutorialModeToggleBtn.ActivateCustomUI(true);

            // Tooltip
            taskPanelSWCTooltip = new Tooltip(__instance.TaskPanel.gameObject, "TaskPanelSWC", "This is your secondary objective. You must succeed as\ncrew/imposter AND complete this objective to win.\nProtect = Keep target player alive.\nFrame = Make sure target player is eliminated.", 0.65f, 4f, new Vector3(1.38f, -2.6f, 0), 1f);
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void PostfixUpdate()
        {
            ScreenshotManager.Instance.CheckButtonClicks();
            HeadlineManager.Instance.CheckButtonClicks();
            HudManagerCheckForButtonClicks();

            if (TutorialBookletManager.Instance != null)
            {
                TutorialBookletManager.Instance.CheckForButtonClicks();
            }

            if (Minigame.Instance != null && TaskAssigner.Instance.isAssignerPanelActive)
            {
                TaskAssigner.Instance.CheckForPlayerButtonClick();
            }
            else if (Minigame.Instance == null && TaskAssigner.Instance.isAssignerPanelActive)
            {
                // close the panel if no player was selected but the game is closed
                TaskAssigner.Instance.ActivatePanel(false);
            }
            // show the next noticifation if any
            if (NotificationManager.NotificationQueue != null)
            {
                if(!NotificationManager.isBroadcasting && NotificationManager.NotificationQueue.Count > 0)
                {
                    NotificationManager.ShowNextNotification();
                }
            }
            // send the next image if any is queuing
            if (AmongUsClient.Instance.AmHost && !ImageQueuer.isSharing && ImageQueuer.HasItems())
            {
                ImageQueuer.NextCanShare();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(HudManager.SetHudActive), new[] { typeof(bool) })]
        public static void PostfixSetHudActive(bool isActive)
        {
            if (!ScreenshotManager.Instance.IsCameraOpen)
            {
                ScreenshotManager.Instance.ActivateCameraButton(isActive);
                DoomScroll._log.LogInfo("HudManager.SetHudActive ---- CAMERA ACTIVE: " + isActive);
            }
            HeadlineManager.Instance.ActivateNewsButton(isActive);
            DoomScroll._log.LogInfo("HudManager.SetHudActive ---- NEWS BUTTON ACTIVE: " + isActive);


        }

        [HarmonyPrefix]
        [HarmonyPatch("OpenMeetingRoom")]
        public static void PrefixOpenMeetingRoom(PlayerControl reporter)
        {
            if (PlayerControl.LocalPlayer.AmOwner)  // OpenMeetingRoom is called by the host only - but just in case
            {
                // create a random news
                if (Random.value > 0.33f)
                {
                    bool protect = Random.value > 0.5f;
                    PlayerControl pl = PlayerControl.AllPlayerControls[Random.Range(0, PlayerControl.AllPlayerControls.Count)];
                    HeadlineManager.Instance.RPCSandNews(HeadlineCreator.CreateRandomTrueNews(255, pl, protect));
                }
                else
                {
                    HeadlineManager.Instance.RPCSandNews(HeadlineCreator.CreateRandomFakeNews());
                }
                // game log 
                GameLogger.Write(GameLogger.GetTime() + " - " + ((reporter != null) ? reporter.ToString() + " called for a meeting." : " Meeting was called"));
            }        
        }

        [HarmonyPostfix]
        [HarmonyPatch("ShowEmblem")]
        public static void PostfixShowEmblem(HudManager __instance, bool shhh)
        {
            if (shhh)
            {
                LobbyBehaviourPatch.gameBegun = true;
            }
        }

        private static void ToggleTutorialButtonSelected()
        {
            m_tutorialModeToggleBtn.SelectButton(Tooltip.TutorialModeOn);
            DoomScroll._log.LogInfo("Changed SelectMode of toggle tutorial button to match TutorialModeOn!");
        }
        public static void HudManagerCheckForButtonClicks()
        {

            // If the Tutorial Mode toggle button is active invoke toggle on mouse click 
            if (m_tutorialModeToggleBtn != null)
            {
                try
                {
                    m_tutorialModeToggleBtn.ReplaceImgageOnHover();
                    if (m_tutorialModeToggleBtn.IsHovered())
                    {
                    }
                    if (m_tutorialModeToggleBtn.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        m_tutorialModeToggleBtn.ButtonEvent.InvokeAction();
                    }
                }
                catch (System.Exception e)
                {
                    DoomScroll._log.LogError("Error invoking tutorialBookletToggle: " + e);
                }
            }
        }

    }
}
