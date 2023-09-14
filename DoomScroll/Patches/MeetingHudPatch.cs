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
        public static CustomText currentToolTipText;
        public static MeetingHud meetingHudInstance;
        public static GameObject playerIcon;

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void PostfixUpdate()
        {
            FolderManager.Instance.CheckForButtonClicks();
            NewsFeedManager.Instance.CheckForShareAndEndorseClicks();
            TaskAssigner.Instance.CheckForShareTaskClicks();
            if (currentToolTipText.TextMP.text != "" && meetingHudInstance.CurrentState != MeetingHud.VoteStates.Discussion && meetingHudInstance.CurrentState != MeetingHud.VoteStates.Animating)
            {
                currentToolTipText.TextMP.text = "";
                currentToolTipText.ActivateCustomUI(false);
                DoomScroll._log.LogInfo($"MeetingHud state is {meetingHudInstance.CurrentState}. Text should be deactivated!");
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

        }
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(MeetingHud __instance)
        {
            meetingHudInstance = __instance;
            DoomScroll._log.LogInfo("Meeting Hud starting! Trying to add tooltip.");
            GameObject uiParent = __instance.TitleText.gameObject;
            currentToolTipText = new CustomText(uiParent, "DiscussionTimeTooltip", "Use this time to look through the files in the folder!\n<size=50%>Open the chat, and click the folder button with a paperclip on it.</size>");
            currentToolTipText.SetColor(Color.yellow);
            currentToolTipText.SetSize(3f);
            Vector3 textPos = new Vector3(0, -3.5f, -10);
            currentToolTipText.SetLocalPosition(textPos);
            currentToolTipText.ActivateCustomUI(true);
            DoomScroll._log.LogInfo("Text should be activated!");

            playerIcon = __instance.PlayerVotePrefab.gameObject;
            DoomScroll._log.LogInfo("PLAYER PREFAB ICON: " + playerIcon.name);
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
