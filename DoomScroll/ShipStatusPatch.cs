using HarmonyLib;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(ShipStatus))]
    class ShipStatusPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart()
        {
            ScreenshotManager.Instance.ActivateCameraButton(true);
            DoomScroll._log.LogInfo("ShipStatusPatch.Start ---- CAMERA INIT");

            SecondaryWinCondition.initSecondaryWinCondition(PlayerControl.LocalPlayer._cachedData.PlayerId);
            DoomScroll._log.LogInfo("SecondaryWinCondition initialized: " + SecondaryWinCondition.ToString());
        }
    }
}
