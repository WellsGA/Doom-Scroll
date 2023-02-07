using Doom_Scroll;
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
            TaskAssigner.Instance.DisplayAssignedTasks();
        }

        [HarmonyPrefix]
        [HarmonyPatch("VotingComplete")]
        public static void PrefixVotingComplete(MeetingHud __instance, ref GameData.PlayerInfo exiled)
        {
            if (exiled != null)
            {
                SecondaryWinCondition.checkTargetVotedOut(exiled);
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch("RpcVotingComplete")]
        public static void PrefixRpcVotingComplete(MeetingHud __instance, ref GameData.PlayerInfo exiled)
        {
            if (exiled != null)
            {
                SecondaryWinCondition.checkTargetVotedOut(exiled);
            }
        }
    }
}
