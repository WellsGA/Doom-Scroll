using Doom_Scroll.Common;
using Doom_Scroll.UI;
using HarmonyLib;
using Hazel;
using UnityEngine;
using System;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(MeetingHud))]
    static class MeetingHudPatch
    {
        public static Tooltip meetingBeginningToolTip;
        private static PlayerVoteArea[] playerVoters;

        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static bool PrefixUpdate(MeetingHud __instance)
        {
            if (__instance.CurrentState == MeetingHud.VoteStates.Animating)
            {
                return false;
            }
            if (!HeadlineDisplay.Instance.HasFinishedSetup)
            {
                if (DestroyableSingleton<HudManager>.Instance.Chat.gameObject.active)
                {
                    DestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(false); ;
                }
                if (FolderManager.Instance.IsFolderOpen()) FolderManager.Instance.CloseFolderOverlay();
                if (playerVoters.Length > 0)
                {
                    foreach (PlayerVoteArea playerVoteArea in playerVoters)
                    {
                        playerVoteArea.gameObject.SetActive(false);
                    }
                }
                __instance.TitleText.text = "Which headlines do you trust?";
                HeadlineDisplay.Instance.SetUpVoteForHeadlines(__instance.Glass);
                return false;
            }
            if (HeadlineDisplay.Instance.discussionStartTimer <= 20)
            {
                HeadlineDisplay.Instance.CheckForTrustClicks();
                float timeRemaining = 20 - HeadlineDisplay.Instance.discussionStartTimer;
                __instance.TimerText.text = "Sorting headlines ends: " + Mathf.CeilToInt(timeRemaining).ToString();
                HeadlineDisplay.Instance.discussionStartTimer += Time.deltaTime;
                return false;
            }
            else if (!HeadlineDisplay.Instance.HasHeadlineVoteEnded)
            {
                if (playerVoters.Length > 0)
                {
                    foreach (PlayerVoteArea playerVoteArea in playerVoters)
                    {
                        playerVoteArea.gameObject.SetActive(true);
                    }
                }
                HeadlineDisplay.Instance.FinishVoteForHeadlines();
                __instance.TitleText.text = "Who is the impostor?";
                DestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(true);
                __instance.discussionTimer = Time.deltaTime;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void PostfixUpdate(MeetingHud __instance)
        {
            FolderManager.Instance.CheckForButtonClicks();
            HeadlineDisplay.Instance.CheckForShareClicks();
            TaskAssigner.Instance.CheckForShareTaskClicks();
            foreach(HeadlineEndorsement headline in HeadlineDisplay.Instance.endorsemntList)
            {
                headline.CheckForEndorseClicks();
            }

            if (meetingBeginningToolTip.TextObject.TextMP.text != "" && __instance.CurrentState != MeetingHud.VoteStates.Discussion && __instance.CurrentState != MeetingHud.VoteStates.Animating)
            {
                meetingBeginningToolTip.TextObject.TextMP.text = "";
                meetingBeginningToolTip.ActivateToolTip(false);
                Tooltip.currentTooltips.Remove(meetingBeginningToolTip);
                DoomScroll._log.LogInfo($"MeetingHud state is {__instance.CurrentState}. Tooltip should be deactivated!");
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch("Close")]
        public static void PostfixClose()
        {
            // calculate vote
            FolderManager.Instance.CloseFolderOverlay();
            string results = "";
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                results += player.name + ": " + HeadlineDisplay.Instance.CalculateScores(player.PlayerId);
            }
            DoomScroll._log.LogInfo(results); // debug
            
        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(MeetingHud __instance)
        {
            DoomScroll._log.LogInfo("Meeting Hud starting! Trying to add tooltip.");
            GameObject uiParent = __instance.TitleText.gameObject;
            Vector3 textPos = new Vector3(0, -3f, -10);
            meetingBeginningToolTip = new Tooltip(uiParent, "DiscussionTime", "Use this time to look through the files in the folder!\n<size=50%>Open the chat, and click the folder button with a paperclip on it.</size>", 0.75f, 9.5f, textPos, 3f);
            DoomScroll._log.LogInfo("ToolTip should be activated if Tutorial Mode is On!");

            playerVoters = __instance.GetComponentsInChildren<PlayerVoteArea>();
            HeadlineDisplay.Instance.ResetHeadlineVotes();
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
