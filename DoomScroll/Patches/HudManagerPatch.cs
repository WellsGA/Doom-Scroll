﻿using Doom_Scroll.Common;
using Doom_Scroll.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

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
            DoomScroll._log.LogInfo("HUDMANAGER RESETS HEREEEEE.");
            Tooltip.ResetCurrentTooltips();
            ScreenshotManager.Instance.Reset();
            FolderManager.Instance.Reset();
            TaskAssigner.Instance.Reset();
            HeadlineManager.Instance.Reset();
            HeadlineDisplay.Instance.Reset();
            SecondaryWinConditionManager.Reset();

            // Tutorial Mode toggle button
            m_tutorialModeToggleBtn = Tooltip.CreateTutorialModeToggleBtn(__instance.SettingsButton, new Vector3(-3f, 0, 0));
            m_tutorialModeToggleBtn.ButtonEvent.MyAction += ToggleTutorialButtonSelected;
            m_tutorialModeToggleBtn.ButtonEvent.MyAction += FolderManager.RectifyFolderTooltips;
            m_tutorialModeToggleBtn.SelectButton(Tooltip.TutorialModeOn);
            DoomScroll._log.LogInfo("Button event added to button.");
            m_tutorialModeToggleBtn.EnableButton(true);
            m_tutorialModeToggleBtn.ActivateCustomUI(true);

            // Tooltip
            taskPanelSWCTooltip = new Tooltip(__instance.TaskPanel.gameObject, "TaskPanelSWC", "This is your secondary objective. You must succeed as\ncrew/imposter AND complete this objective to win.\nProtect = Keep target player alive.\nFrame = Make sure target player is eliminated.", 0.65f, 4f, new Vector3(1.38f, -1.6f, 0), 1f);
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void PostfixUpdate()
        {
            ScreenshotManager.Instance.CheckButtonClicks();
            HeadlineManager.Instance.CheckButtonClicks();
            HudManagerCheckForButtonClicks();

            if (Minigame.Instance != null && TaskAssigner.Instance.isAssignerPanelActive)
            {
                TaskAssigner.Instance.CheckForPlayerButtonClick();
            }
            else if (Minigame.Instance == null && TaskAssigner.Instance.isAssignerPanelActive)
            {
                // close the panel if no player was selected but the game is closed
                TaskAssigner.Instance.ActivatePanel(false);
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
            DoomScroll._log.LogInfo("MEETING OPENED");
            if (ScreenshotManager.Instance.IsCameraOpen)
            {
                ScreenshotManager.Instance.ToggleCamera();
                DoomScroll._log.LogInfo("HudManager.OpenMeetingRoom ---- CAMERA CLOSED");
            }
            if (HeadlineManager.Instance.NewsModal.IsModalOpen)
            {
                HeadlineManager.Instance.NewsModal.CloseButton.ButtonEvent.InvokeAction();
                DoomScroll._log.LogInfo("HudManager.OpenMeetingRoom ---- NEWS FORM CLOSED");
            }

            HeadlineManager.Instance.CanPostNews(false); // cannot create news
            if (PlayerControl.LocalPlayer.AmOwner)
            {
                // create a random news
                if (Random.Range(0, 2) == 0)
                {
                    HeadlineManager.Instance.RPCSandNews(HeadlineCreator.CreateRandomTrueNews());
                }
                else
                {
                    HeadlineManager.Instance.RPCSandNews(HeadlineCreator.CreateRandomFakeNews());
                }
                // selects new players to post news
                //HeadlineManager.Instance.SelectPLayersWhoCanPostNews();

                // game log 
                GameLogger.Write(GameLogger.GetTime() + " - " + ((reporter != null) ? reporter.ToString() + " called for a meeting." : " Meeting was called"));
            }
            DoomScroll._log.LogInfo(HeadlineManager.Instance.ToString()); // debug

        }

        [HarmonyPostfix]
        [HarmonyPatch("ShowEmblem")]
        public static void PostfixShowEmblem(HudManager __instance, bool shhh)
        {
            if (shhh)
            {
                LobbyBehaviourPatch.gameBegun = true;
            }

            /*
            if (!shhh)
            {
                DoomScroll._log.LogInfo("Discussion Behavior emblem animating!");
                GameObject uiParent = __instance.discussEmblem.Text.gameObject;
                CustomText toolTipText = new CustomText(uiParent, "DiscussionTimeTooltip", "Use this time to look through the files in the folder!\n<size=50%>Open the chat, and click the folder button with a paperclip on it.</size>");
                toolTipText.SetColor(Color.red);
                toolTipText.SetSize(3f);
                Vector3 textPos = new Vector3(0, 0, -10);
                toolTipText.SetLocalPosition(textPos);
                toolTipText.ActivateCustomUI(true);
                DoomScroll._log.LogInfo("Text should be activated!");
            }
            */
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
