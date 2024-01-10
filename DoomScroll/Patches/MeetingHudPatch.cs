using Doom_Scroll.Common;
using Doom_Scroll.UI;
using HarmonyLib;
using Hazel;
using UnityEngine;
using System;
using System.Reflection;

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
                return true;
            }
            if(__instance.CurrentState == MeetingHud.VoteStates.Results)
            {
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
                    __instance.ProceedButton.gameObject.SetActive(false);
                    return false;
                }
                if (HeadlineDisplay.Instance.discussionStartTimer <= 20)
                {
                    HeadlineDisplay.Instance.CheckForTrustClicks();
                    float timeRemaining = 20 - HeadlineDisplay.Instance.discussionStartTimer;
                    if (!__instance.TimerText.isActiveAndEnabled) { __instance.TimerText.gameObject.SetActive(true); }
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
                    __instance.TitleText.text = "Voting is Over";
                    __instance.TimerText.gameObject.SetActive(false);
                    __instance.ProceedButton.gameObject.SetActive(true);
                    return false;
                }
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
            foreach(HeadlineEndorsement headline in HeadlineDisplay.Instance.endorsementList)
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
            // VOTING
            FolderManager.Instance.CloseFolderOverlay();
            string results = "";
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                results += player.name + ": " + HeadlineDisplay.Instance.CalculateScoreStrings(player.PlayerId);
            }
            DoomScroll._log.LogInfo(results); // debug
            // SCREENSHOT
            if (AmongUsClient.Instance.AmHost)
            {
                ScreenshotManager.Instance.SelectAPlayerToTakeScreenshot();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(MeetingHud __instance)
        {
            DoomScroll._log.LogInfo("Meeting Hud starting! Trying to add tooltip.");
            GameObject uiParent = __instance.TitleText.gameObject;
            Vector3 textPos = new Vector3(0, -3f, -10);
            meetingBeginningToolTip = new Tooltip(uiParent, "DiscussionTime", "Use this time to look through the files in the folder!\n<size=50%>Open the chat, and click the folder button with a paperclip on it.\nInvestigate the information to determine the truth!</size>", 0.75f, 9.5f, textPos, 3f);
            DoomScroll._log.LogInfo("ToolTip should be activated if Tutorial Mode is On!");

            playerVoters = __instance.GetComponentsInChildren<PlayerVoteArea>();
            HeadlineDisplay.Instance.ResetHeadlineVotes();

            ScreenshotManager.Instance.EnableCameraButton(false); // disable camera even if no picture was taken
        }

        [HarmonyPostfix]
        [HarmonyPatch("CastVote")]
        public static void PostFixCastVote(byte srcPlayerId, byte suspectPlayerId)
        {
            
            if (AmongUsClient.Instance.AmHost)
            {
                // Log votes
                string voter = GameData.Instance.GetPlayerById(srcPlayerId) == null ? "some one" : GameData.Instance.GetPlayerById(srcPlayerId).PlayerName;
                string suspect = GameData.Instance.GetPlayerById(suspectPlayerId) == null ? "no one" : GameData.Instance.GetPlayerById(suspectPlayerId).PlayerName;
                GameLogger.Write(GameLogger.GetTime() + " - " + voter + " has voted for " + suspect);
                
                // check for vote results if all-1 player had voded
            }
            HeadlineCreator.UpdatePlayerVote(srcPlayerId, suspectPlayerId);
        }


        [HarmonyPostfix]
        [HarmonyPatch("VotingComplete")]
        public static void PrefixVotingComplete(object[] __args)
        {
            foreach(object thing in __args)
            {
                if (thing == null)
                {
                    DoomScroll._log.LogInfo("Current __args thing is NULL.");
                }
                else
                {
                    DoomScroll._log.LogInfo("Current __args thing is NOT null.");
                }

            }
            if (__args[1] != null)
            {
                DoomScroll._log.LogInfo("__args[1] is not null.");
                if (__args[1] != null)
                {
                    DoomScroll._log.LogInfo("__args[1] to String is " + __args[1].ToString());
                    DoomScroll._log.LogInfo("__args[1] as a PlayerInfo is " + (GameData.PlayerInfo)__args[1]);
                    DoomScroll._log.LogInfo("__args[1] is indeed a GameData.PlayerInfo");
                    ExileControllerPatch.OriginalExiledPlayer = (GameData.PlayerInfo)__args[1];
                    DoomScroll._log.LogInfo(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> OriginalExiledPlayer is being set as: " + (GameData.PlayerInfo)__args[1] + ", " + ((GameData.PlayerInfo)__args[1]).PlayerName);
                    __args[1] = null;
                }
            }
            if (__args[2] != null)
            {
                DoomScroll._log.LogInfo("__args[2] is not null.");
            }
            
        }
    }
}
