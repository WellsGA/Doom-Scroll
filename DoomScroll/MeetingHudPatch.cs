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
        [HarmonyPatch("Start")]
        public static void PostfixStart()
        {
            TaskAssigner.Instance.DisplayAssignedTasks();  // debug purposes
        }

    }
}
