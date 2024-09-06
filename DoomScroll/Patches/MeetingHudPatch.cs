using HarmonyLib;
using UnityEngine;
using System;
using Doom_Scroll.UI;
using Doom_Scroll.Common;
using System.Reflection;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(MeetingHud))]
    static class MeetingHudPatch
    {
        public static Tooltip meetingBeginningToolTip;
        private static PlayerVoteArea[] playerVoteAreas;
        private static CustomImage NewsInfoCard;
        private static CustomText NewsInfoText;
        private static bool canVote;

        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static bool PrefixUpdate(MeetingHud __instance)
        {
            if (__instance.CurrentState == MeetingHud.VoteStates.NotVoted)
            {
                if (!HeadlineDisplay.Instance.HasFinishedSetup)
                {
                    if (DestroyableSingleton<HudManager>.Instance.Chat.gameObject.active)
                    {
                        DestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(false); ;
                    }
                    ActivateVoteAreas(false);
                    __instance.TitleText.text = "Which headlines do you trust?";
                    HeadlineDisplay.Instance.SetUpVoteForHeadlines(__instance.Glass);
                    __instance.ProceedButton.gameObject.SetActive(false);
                    return false;
                }
                if (HeadlineDisplay.Instance.discussionStartTimer <= 20)
                {
                    HeadlineDisplay.Instance.CheckForDisplayedVotingPageButtonClicks();
                    HeadlineDisplay.Instance.CheckForTrustClicks();
                    float timeRemaining = 20 - HeadlineDisplay.Instance.discussionStartTimer;
                    if (!__instance.TimerText.isActiveAndEnabled) { __instance.TimerText.gameObject.SetActive(true); }
                    __instance.TimerText.text = "Sorting headlines ends: " + Mathf.CeilToInt(timeRemaining).ToString();
                    HeadlineDisplay.Instance.discussionStartTimer += Time.deltaTime;
                    return false;
                }
                else if (!HeadlineDisplay.Instance.HasHeadlineVoteEnded)
                {
                    // calculate scores
                    Tuple<int, int> score = HeadlineDisplay.Instance.CalculateScores(PlayerControl.LocalPlayer.PlayerId);
                    DoomScroll._log.LogInfo("Correct: " + score.Item1 + " incorrect: " + score.Item2);
                    canVote = score.Item1 >= score.Item2;
                    if (!canVote)
                    { // got less than half of them correctly
                        ActivateVoteAreas(false);
                        __instance.TitleText.text = "You cannot vote in this round.";
                        NewsInfoText.TextMP.text += HeadlineDisplay.Instance.GetWrongAnswers(PlayerControl.LocalPlayer.PlayerId);
                        NewsInfoCard.ActivateCustomUI(true);
                        DoomScroll._log.LogInfo("================= NEWS INFO : " + NewsInfoCard.UIGameObject.transform.parent.name + "=================");
                    }
                    else
                    {
                        ActivateVoteAreas(true);
                        __instance.TitleText.text = "Who is the impostor?";
                    }

                    HeadlineDisplay.Instance.FinishVoteForHeadlines();
                    __instance.TimerText.gameObject.SetActive(false);
                    __instance.ProceedButton.gameObject.SetActive(true);
                    return false;
                }
            }
            if(__instance.CurrentState == MeetingHud.VoteStates.Results && !canVote)
            {
                ActivateVoteAreas(true);
                NewsInfoCard.ActivateCustomUI(false);
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
            /* foreach(HeadlineEndorsement headline in HeadlineDisplay.Instance.endorsementList){ headline.CheckForEndorseClicks(); }*/

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
            // HEADLINE SORTING
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
            NewsInfoText.SetText("You didn't get enough headlines correctly, therefore, you cannot vote this time. \n Take another look at the headlines you missed. Examine both the title and the source.\n");
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

            __instance.discussionTimer = -10; // set a longer discussion time!
            playerVoteAreas = __instance.GetComponentsInChildren<PlayerVoteArea>();

            HeadlineDisplay.Instance.ResetHeadlineVotes();
            // News info panel for incorrect sorting
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.card.png");
            NewsInfoCard = new CustomImage(__instance.Glass.gameObject, "HeadlineSortinginfo", spr);
            NewsInfoCard.SetLocalPosition(new Vector3(0, 0, -20));
            NewsInfoCard.SetSize(__instance.Glass.size.x *0.85f);

            NewsInfoText = new CustomText(NewsInfoCard.UIGameObject, "Headline info", "You didn't get enough headlines correctly, therefore, you cannot vote this time. \n Take another look at the headlines you missed. Examine both the title and the source.\n");
            NewsInfoText.SetSize(1.2f);
            NewsInfoText.SetScale(Vector3.one);
            NewsInfoText.SetLocalPosition(new Vector3(0, 0, -20));
            NewsInfoCard.ActivateCustomUI(false);
            ScreenshotManager.Instance.EnableCameraButton(false); // disable camera even if no picture was taken
        }

        private static void ActivateVoteAreas(bool activate)
        {
            if(playerVoteAreas ==  null) { return;  }
            if (playerVoteAreas.Length > 0)
            {
                foreach (PlayerVoteArea playerVoteArea in playerVoteAreas)
                {
                    playerVoteArea.gameObject.SetActive(activate);
                }
            }
        }
    }
}
