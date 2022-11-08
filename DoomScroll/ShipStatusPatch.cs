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

        }

    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static class PatchMinPlayers
    {
        public static void Prefix(GameStartManager __instance)
        {
            __instance.MinPlayers = 2;
        }
    }
}
