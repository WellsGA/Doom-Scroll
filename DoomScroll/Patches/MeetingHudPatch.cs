using Doom_Scroll.Common;
using Doom_Scroll.UI;
using HarmonyLib;
using Hazel;
using UnityEngine;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(MeetingHud))]
    static class MeetingHudPatch
    {
        public static Tooltip meetingBeginningToolTip;
        public static MeetingHud meetingHudInstance;
        public static GameObject playerIcon;

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void PostfixUpdate()
        {
            FolderManager.Instance.CheckForButtonClicks();
            NewsFeedManager.Instance.CheckForShareClicks();
            TaskAssigner.Instance.CheckForShareTaskClicks();
            foreach(PostEndorsement end in ChatControllerPatch.endorsemntList)
            {
                end.CheckForEndorseClicks();
            }
            if (meetingBeginningToolTip.TextObject.TextMP.text != "" && meetingHudInstance.CurrentState != MeetingHud.VoteStates.Discussion && meetingHudInstance.CurrentState != MeetingHud.VoteStates.Animating)
            {
                meetingBeginningToolTip.TextObject.TextMP.text = "";
                meetingBeginningToolTip.ActivateToolTip(false);
                Tooltip.currentTooltips.Remove(meetingBeginningToolTip);
                DoomScroll._log.LogInfo($"MeetingHud state is {meetingHudInstance.CurrentState}. Tooltip should be deactivated!");
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch("Close")]
        public static void PostfixClose()
        {
            FolderManager.Instance.CloseFolderOverlay();
            
            string results = "";
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                results += player.name + ": " + NewsFeedManager.Instance.CalculateEndorsementScores(player.PlayerId);
            }
            DoomScroll._log.LogInfo(results); // debug

            //tooltip stuff

        }
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(MeetingHud __instance)
        {
            meetingHudInstance = __instance;
            DoomScroll._log.LogInfo("Meeting Hud starting! Trying to add tooltip.");
            GameObject uiParent = __instance.TitleText.gameObject;
            Vector3 textPos = new Vector3(0, -3f, -10);
            meetingBeginningToolTip = new Tooltip(uiParent, "DiscussionTime", "Use this time to look through the files in the folder!\n<size=50%>Open the chat, and click the folder button with a paperclip on it.</size>", 0.75f, 9.5f, textPos, 3f);
            DoomScroll._log.LogInfo("ToolTip should be activated if Tutorial Mode is On!");

            playerIcon = __instance.PlayerVotePrefab.gameObject;
            DoomScroll._log.LogInfo("PLAYER PREFAB ICON: " + playerIcon.name);

            //tooltip stuff
        }

        [HarmonyPostfix]
        [HarmonyPatch("CastVote")]
        public static void PostFixCastVote(byte srcPlayerId, byte suspectPlayerId)
        {
            string voter = GameData.Instance.GetPlayerById(srcPlayerId) == null ? "no one" : GameData.Instance.GetPlayerById(srcPlayerId).PlayerName;
            string suspect = GameData.Instance.GetPlayerById(suspectPlayerId) == null ? "no one" : GameData.Instance.GetPlayerById(suspectPlayerId).PlayerName;

            if (AmongUsClient.Instance.AmHost)
            {
                 GameLogger.Write(GameLogger.GetTime() + " - " + voter + " has voted for " + suspect);
            }
            else
            {
                RPCVote(voter, suspect);
            }
        }

        public static void RPCVote(string voter, string suspect)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDVOTE, (SendOption)1);
            messageWriter.Write(voter);
            messageWriter.Write(suspect);
            messageWriter.EndMessage();
        }
    }
}
