using HarmonyLib;
using UnityEngine;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(HudManager))]
    public static class HudManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart()
        {
            ScreenshotManager.Instance.ReSet();
            FolderManager.Instance.Reset();
            TaskAssigner.Instance.Reset();
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void PostfixUpdate()
        {
            ScreenshotManager.Instance.CheckButtonClicks();
            
            if (Minigame.Instance != null && TaskAssigner.Instance.PlayerButtonHolder.UIGameObject.active)
            {
                TaskAssigner.Instance.CheckForPlayerButtonClick();
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
        }
    }
}
