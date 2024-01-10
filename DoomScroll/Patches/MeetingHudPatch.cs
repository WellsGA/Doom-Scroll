﻿using Doom_Scroll.Common;
using Doom_Scroll.UI;
using HarmonyLib;
using Hazel;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(MeetingHud))]
    static class MeetingHudPatch
    {
        public static Tooltip meetingBeginningToolTip;
        private static PlayerVoteArea[] playerVoters;

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


        [HarmonyPrefix]
        [HarmonyPatch("PrefixCastVote")]
        public static bool PrefixCastVote(MeetingHud __instance, byte srcPlayerId, byte suspectPlayerId)
        {
            if (AmongUsClient.Instance.AmHost)
            {
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
                    }
                    else if (!playerVoteArea.AmDead && !playerVoteArea.DidVote) // someone else is alive and didn't vote
                    {
                        return true; // stops the prefix, continues on with the method as expected
                    }
                }

                // means that this is the only person who hasn't voted!! Time to calculate and replace votes...
                thisVoteArea.SetVote(suspectPlayerId);
                Dictionary<byte, int> dictionary = DoomCalculateVotes(__instance); // Calculates votes after locally setting whatever this current player's vote should be

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
                MeetingHud.VoterState[] array2 = new MeetingHud.VoterState[__instance.playerStates.Length];
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    array2[i] = new MeetingHud.VoterState
                    {
                        VoterId = playerVoteArea.TargetPlayerId,
                        VotedForId = playerVoteArea.VotedFor
                    };
                }
                //their code ends!
                ExileControllerPatch.OriginalArray2 = array2;
                ExileControllerPatch.OriginalExiledPlayer = exiled;
                ExileControllerPatch.OriginalTie = tie;

                //Now we're setting everyone's votes to SKIP
                //Using code from CastVote
                foreach (GameData.PlayerInfo v in GameData.Instance.AllPlayers)
                {
                    if (!v.IsDead)
                    {
                        DoomCastVote(__instance, v.PlayerId, PlayerVoteArea.SkippedVote);
                    }
                }

                // Now check for end voting, and cancel actual method.
                __instance.CheckForEndVoting();
            }

            return true;
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
