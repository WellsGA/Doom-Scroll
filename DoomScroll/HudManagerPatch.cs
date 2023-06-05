using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(HudManager))]
    public static class HudManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(HudManager __instance)
        {
            // adding Canvas to the Hud
            /*__instance.gameObject.AddComponent<Canvas>();
            CanvasScaler cs = __instance.gameObject.AddComponent<CanvasScaler>();
            cs.m_UiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            __instance.gameObject.AddComponent<GraphicRaycaster>();*/

            ScreenshotManager.Instance.Reset();
            FolderManager.Instance.Reset();
            TaskAssigner.Instance.Reset();
            NewsFeedManager.Instance.Reset();
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void PostfixUpdate()
        {
            ScreenshotManager.Instance.CheckButtonClicks();
            NewsFeedManager.Instance.CheckButtonClicks();

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
        [HarmonyPatch(nameof(HudManager.SetHudActive),new[] { typeof(bool) })]
        public static void PostfixSetHudActive(bool isActive)
        {
            if (!ScreenshotManager.Instance.IsCameraOpen)
            {
                ScreenshotManager.Instance.ActivateCameraButton(isActive);
                DoomScroll._log.LogInfo("HudManager.SetHudActive ---- CAMERA ACTIVE: " + isActive);
            }
            if (!NewsFeedManager.Instance.IsInputpanelOpen)
            {
                NewsFeedManager.Instance.ActivateNewsButton(isActive);
                DoomScroll._log.LogInfo("HudManager.SetHudActive ---- NEWS BUTTON ACTIVE: " + isActive);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("OpenMeetingRoom")]
        public static void PrefixOpenMeetingRoom()
        {
            DoomScroll._log.LogInfo("MEETING OPENED");
            if (ScreenshotManager.Instance.IsCameraOpen)
            {
                ScreenshotManager.Instance.ToggleCamera();
                DoomScroll._log.LogInfo("HudManager.OpenMeetingRoom ---- CAMERA CLOSED" );
            }
            if (NewsFeedManager.Instance.IsInputpanelOpen)
            {
                NewsFeedManager.Instance.ToggleNewsForm();
                DoomScroll._log.LogInfo("HudManager.OpenMeetingRoom ---- NEWS FORM CLOSED");
            }

            NewsFeedManager.Instance.DisplayNews();

            NewsFeedManager.Instance.CanPostNews(false); // cannot create news
            if (PlayerControl.LocalPlayer.AmOwner)
            {
                // create a random news
                int rand = UnityEngine.Random.Range(0, 10);
                if (rand % 2 == 0)
                {
                    NewsFeedManager.Instance.CreateTrueNews();
                }
                else
                {
                    NewsFeedManager.Instance.CreateFakeNews();
                }
                // selects new players to post news
                NewsFeedManager.Instance.SelectPLayersWhoCanPostNews();
            }
        }
    }
}
