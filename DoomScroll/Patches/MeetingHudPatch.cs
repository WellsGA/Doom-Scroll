using Doom_Scroll.Common;
using Doom_Scroll.UI;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static MeetingHud;
using System;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(MeetingHud))]
    static class MeetingHudPatch
    {
        public static Tooltip meetingBeginningToolTip;
        private static PlayerVoteArea[] playerVoters;
        private static PlayerVoteArea[] unmodifiedPlayerStates;
        public static MeetingHud.VoterState[] unmodifiedVoterStates;
        private static Dictionary<byte, int> DoomCalculateVotes(MeetingHud __instance)
        {
            Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                if (playerVoteArea.VotedFor != 252 && playerVoteArea.VotedFor != 255 && playerVoteArea.VotedFor != 254)
                {
                    int num;
                    if (dictionary.TryGetValue(playerVoteArea.VotedFor, out num))
                    {
                        dictionary[playerVoteArea.VotedFor] = num + 1;
                    }
                    else
                    {
                        dictionary[playerVoteArea.VotedFor] = 1;
                    }
                }
            }
            return dictionary;
        }
        private static void DoomForceSkipAll(MeetingHud __instance)
        {
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                if (!playerVoteArea.DidVote)
                {
                    playerVoteArea.VotedFor = 254;
                    __instance.DirtyBits |= 1U;
                }
            }
        }

        public static KeyValuePair<byte, int> DoomMaxPair(Dictionary<byte, int> self, out bool tie)
        {
            tie = true;
            KeyValuePair<byte, int> result = new KeyValuePair<byte, int>(byte.MaxValue, int.MinValue);
            foreach (KeyValuePair<byte, int> keyValuePair in self)
            {
                if (keyValuePair.Value > result.Value)
                {
                    result = keyValuePair;
                    tie = false;
                }
                else if (keyValuePair.Value == result.Value)
                {
                    tie = true;
                }
            }
            return result;
        }
        public static void DoomCastVote(MeetingHud __instance, byte srcPlayerId, byte suspectPlayerId)
        {
            GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(srcPlayerId);
            GameData.PlayerInfo playerById2 = GameData.Instance.GetPlayerById(suspectPlayerId);
            __instance.logger.Debug(playerById.PlayerName + " has voted for " + ((playerById2 != null) ? playerById2.PlayerName : "No one"), null);
            PlayerVoteArea playerVoteArea = null;
            foreach (PlayerVoteArea pv in __instance.playerStates)
            {
                if (pv.TargetPlayerId == srcPlayerId)
                {
                    playerVoteArea = pv;
                    break;
                }
            }
            if (!playerVoteArea.AmDead)
            {
                if (PlayerControl.LocalPlayer.PlayerId == srcPlayerId || AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame)
                {
                    SoundManager.Instance.PlaySound(__instance.VoteLockinSound, false, 1f, null);
                }
                playerVoteArea.SetVote(suspectPlayerId);
                __instance.SetDirtyBit(1U);
                //PlayerControl.LocalPlayer.RpcSendChatNote(srcPlayerId, ChatNoteTypes.DidVote);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static bool PrefixUpdate(MeetingHud __instance)
        {
            if (__instance.CurrentState == MeetingHud.VoteStates.Animating)
            {
                return true;
            }
            if (__instance.CurrentState == MeetingHud.VoteStates.Results)
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
           /* foreach(HeadlineEndorsement headline in HeadlineDisplay.Instance.endorsementList)
            {
                headline.CheckForEndorseClicks();
            }*/

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

        [HarmonyPrefix]
        [HarmonyPatch("PopulateResults")]
        public static bool PopulateResultsPatch(MeetingHud __instance, MeetingHud.VoterState[] states)
        {
            if (unmodifiedPlayerStates == null || unmodifiedPlayerStates.Length == 0)
            {
                unmodifiedPlayerStates = __instance.playerStates;
            }
            if (unmodifiedVoterStates == null || unmodifiedVoterStates.Length == 0)
            {
                unmodifiedVoterStates = states;
            }

            DoomScroll._log.LogInfo("Entering our PopulateResultsPatch");
            //__instance.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults, (Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>)Array.Empty<object>());
            int num = 0;
            for (int i = 0; i < unmodifiedPlayerStates.Length; i++)
            {
                DoomScroll._log.LogInfo("Outer Loop.");
                DoomScroll._log.LogInfo($"Length of playerStates array: {unmodifiedPlayerStates.Length}");
                PlayerVoteArea playerVoteArea = unmodifiedPlayerStates[i];
                playerVoteArea.ClearForResults();
                int num2 = 0;
                foreach (MeetingHud.VoterState voterState in unmodifiedVoterStates)
                {
                    DoomScroll._log.LogInfo($"Outer loop: {unmodifiedPlayerStates[i].TargetPlayerId}, Inner loop on another vote area! Player id: {voterState.VoterId}");
                    GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
                    if (playerById == null)
                    {
                        __instance.logger.Error(string.Format("Couldn't find player info for voter: {0}", voterState.VoterId), null);
                    }
                    else if (i == 0 && voterState.SkippedVote)
                    {
                        DoomScroll._log.LogInfo("Trying to populate skipped vote with icon!");
                        __instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
                        num++;
                    }
                    else if (voterState.VotedForId == playerVoteArea.TargetPlayerId)
                    {
                        DoomScroll._log.LogInfo($"Trying to populate player vote with icon! Player id {voterState.VoterId} voted for {playerVoteArea.TargetPlayerId}");
                        __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                        num2++;
                    }
                }
            }
            unmodifiedPlayerStates = new PlayerVoteArea[] { };
            unmodifiedVoterStates = new MeetingHud.VoterState[] { };
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch("CastVote")]
        public static bool PrefixCastVote(MeetingHud __instance, byte srcPlayerId, byte suspectPlayerId)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                // Update last votes for the headlines
                HeadlineCreator.UpdatePlayerVote(srcPlayerId, suspectPlayerId);

                // Log votes
                string voter = GameData.Instance.GetPlayerById(srcPlayerId) == null ? "some one" : GameData.Instance.GetPlayerById(srcPlayerId).PlayerName;
                string suspect = GameData.Instance.GetPlayerById(suspectPlayerId) == null ? "no one" : GameData.Instance.GetPlayerById(suspectPlayerId).PlayerName;
                GameLogger.Write(GameLogger.GetTime() + " - " + voter + " has voted for " + suspect);

                // Code for changing vote stuff!

                // check if this is the last vote
                PlayerVoteArea thisVoteArea = null;
                foreach (PlayerVoteArea playerVoteArea in __instance.playerStates)
                {
                    if (playerVoteArea.TargetPlayerId == srcPlayerId)
                    {
                        thisVoteArea = playerVoteArea;
                        DoomScroll._log.LogInfo("Got current vote area");
                    }
                    else if (!playerVoteArea.AmDead && !playerVoteArea.DidVote) // someone else is alive and didn't vote
                    {
                        DoomScroll._log.LogInfo("There are other players who haven't voted; resuming normal functionality");
                        return true; // stops the prefix, continues on with the method as expected
                    }
                }

                // means that this is the only person who hasn't voted!! Time to calculate and replace votes...
                DoomScroll._log.LogInfo("Trying to set this player's vote");
                thisVoteArea.SetVote(suspectPlayerId);
                DoomScroll._log.LogInfo("Vote set!");
                unmodifiedPlayerStates = __instance.playerStates;

                Dictionary<byte, int> dictionary = DoomCalculateVotes(__instance); // Calculates votes after locally setting whatever this current player's vote should be
                DoomScroll._log.LogInfo("DoomCalculateVotes is done");

                //their code in CheckForEndVoting. array2, exiled, and tie are the parameters that go into RpcVotingComplete.
                bool tie;
                KeyValuePair<byte, int> max = DoomMaxPair(dictionary, out tie);
                Logger logger = __instance.logger;
                string format = "Vote counts: {0} Max={1}@{2} Tie={3}";
                object[] array = new object[4];
                array[0] = string.Join(" ", (from t in dictionary
                                             select t.ToString()).ToArray<string>());
                array[1] = max.Key;
                array[2] = max.Value;
                array[3] = tie;
                logger.Debug(string.Format(format, array), null);
                GameData.PlayerInfo exiled = null;
                foreach (GameData.PlayerInfo v in GameData.Instance.AllPlayers)
                {
                    if (!tie && v.PlayerId == max.Key)
                    {
                        exiled = v;
                        break;
                    }
                }
                DoomScroll._log.LogInfo("Found exiled player!");
                MeetingHud.VoterState[] array2 = new MeetingHud.VoterState[__instance.playerStates.Length];
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea != null)
                    {
                        array2[i] = new MeetingHud.VoterState
                        {
                            VoterId = playerVoteArea.TargetPlayerId,
                            VotedForId = playerVoteArea.VotedFor
                        };
                    }
                }
                // OUR VOTER STATE
                unmodifiedVoterStates = array2;

                //their code ends!
                ExileControllerPatch.OriginalArray2 = array2;
                DoomScroll._log.LogInfo("OriginalArray2 is being set as: " + array2.ToString());
                if (exiled != null)
                {
                    ExileControllerPatch.OriginalExiledPlayer = exiled;
                    DoomScroll._log.LogInfo("OriginalExiledPlayer is being set as: " + exiled + ", " + exiled.PlayerName);
                }
                else
                {

                    ExileControllerPatch.OriginalExiledPlayer = null;
                }
                ExileControllerPatch.OriginalTie = tie;
                DoomScroll._log.LogInfo("OriginalExiledTie is being set as: " + tie.ToString());

                //Now we're setting everyone's votes to SKIP
                //Using code from CastVote
                foreach (GameData.PlayerInfo v in GameData.Instance.AllPlayers)
                {
                    if (v != null && !v.IsDead)
                    {
                        DoomScroll._log.LogInfo("About to do a DoomCastVote for skipped");
                        DoomCastVote(__instance, v.PlayerId, PlayerVoteArea.SkippedVote);
                    }
                }

                // Now check for end voting, and cancel actual method.
                DoomScroll._log.LogInfo("Checking for end voting now!");
                __instance.CheckForEndVoting();
                return false;
            }

            return true;
        }


        [HarmonyPostfix]
        [HarmonyPatch("VotingComplete")]
        public static void PrefixVotingComplete(object[] __args)
        {
            foreach (object thing in __args)
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
            if (__args[0] != null)
            {
                if (ExileControllerPatch.OriginalArray2 != null)
                {
                    __args[0] = ExileControllerPatch.OriginalArray2;
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
                    DoomScroll._log.LogInfo(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> OriginalExiledPlayer is being set as: " + (GameData.PlayerInfo)__args[1] + ", " + ((GameData.PlayerInfo)__args[1]).PlayerName);
                    if (ExileControllerPatch.OriginalExiledPlayer != null)
                    {
                        __args[1] = ExileControllerPatch.OriginalExiledPlayer;
                    }
                }
            }
            else
            {
                if (ExileControllerPatch.OriginalExiledPlayer != null)
                {
                    __args[1] = ExileControllerPatch.OriginalExiledPlayer;
                }
            }
            if (__args[2] != null)
            {
                DoomScroll._log.LogInfo("__args[2] is not null.");
                if (ExileControllerPatch.OriginalTie != null)
                {
                    __args[0] = ExileControllerPatch.OriginalTie;
                }
            }

        }
    }
}
