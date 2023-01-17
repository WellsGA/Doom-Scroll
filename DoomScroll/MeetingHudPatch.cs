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
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    public static class MeetingHudVotingCompletePatch
    {
        public static void Prefix(MeetingHud __instance, ref GameData.PlayerInfo exiled)
        {
            if (exiled != null)
            {
                SecondaryWinConditionHolder.checkTargetVotedOut(exiled);
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.RpcVotingComplete))]
    public static class MeetingHudRpcVotingCompletePatch
    {
        public static void Prefix(MeetingHud __instance, ref GameData.PlayerInfo exiled)
        {
            if (exiled != null)
            {
                SecondaryWinConditionHolder.checkTargetVotedOut(exiled);
            }
        }
    }
}
