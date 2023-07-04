using HarmonyLib;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(MeetingHud))]
    static class MeetingHudPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void PostfixUpdate()
        {
            FolderManager.Instance.CheckForButtonClicks();
        }
        [HarmonyPostfix]
        [HarmonyPatch("Close")]
        public static void PostfixClose()
        {
            FolderManager.Instance.CloseFolderOverlay();
        }

    }
}
