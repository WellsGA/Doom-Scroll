using Doom_Scroll.UI;
using HarmonyLib;
using Il2CppSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(ExileController))]
    static class ExileControllerPatch
    {
        private static string _retainedExileString;
        public static MeetingHud.VoterState[] OriginalArray2;
        public static GameData.PlayerInfo OriginalExiledPlayer;
        public static bool OriginalTie;
        private static bool _exiledWasOverridden;

        [HarmonyPrefix]
        [HarmonyPatch("Begin")]
        public static bool PrefixBegin(ExileController __instance, object[] __args)
        {
            DoomScroll._log.LogInfo($"Length of args: {__args.Length}");

            int num = 0;
            foreach (GameData.PlayerInfo p in GameData.Instance.AllPlayers)
            {
                if (p.Role.IsImpostor && !p.IsDead && !p.Disconnected)
                {
                    num++;
                }
            }

            int num2 = 0;
            foreach (GameData.PlayerInfo p in GameData.Instance.AllPlayers)
            {
                if (p.Role.IsImpostor)
                {
                    num2++;
                }
            }

            int numAlivePlayers = 0;
            foreach (GameData.PlayerInfo p in GameData.Instance.AllPlayers)
            {
                if (!p.Role.IsImpostor && !p.IsDead && !p.Disconnected)
                {
                    numAlivePlayers++;
                }
            }


            if (!DoomScrollVictoryManager.CheckVotingSuccess())
            {
                DoomScroll._log.LogInfo("Voting was not a success!");
                __instance.exiled = null;

                if (OriginalExiledPlayer != null)
                {
                    DoomScroll._log.LogInfo(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> OriginalExiledPlayer: " + OriginalExiledPlayer.ToString());
                    //CALLS SAME STUFF AS ACTUAL THING, THEN CANCELS METHOD IF WE MODIFYIING THE EXILE.


                    if (OriginalExiledPlayer.Role.IsImpostor)
                    {
                        if (num2 > 1) // If the amount of impostors is more than 1
                        {
                            if (num > 1) // If the amount of living impostors left is > 1; basically, if the game doesn't end from them voting out this impostor, SET THE EXILED PLAYER BACK TO NORMAL AND RUN OUR VERSION OF THE REAL METHOD
                            {
                                DoomExileBeginLikeNormal(__instance, num, num2);
                                return false;
                            }
                            __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPP, (OriginalExiledPlayer.PlayerName));
                            _retainedExileString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPP, (OriginalExiledPlayer.PlayerName));
                            DoomScroll._log.LogInfo($"String set to: {__instance.completeString}");
                        }
                        else
                        {
                            __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSP, (OriginalExiledPlayer.PlayerName));
                            _retainedExileString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSP, (OriginalExiledPlayer.PlayerName));
                            DoomScroll._log.LogInfo($"String set to: {__instance.completeString}");
                        }
                    }
                    else // IF THEY VOTED SOMEONE OUT AND THEY AREN'T WINNING FROM IT, SET THE EXILED PLAYER BACK TO NORMAL AND RUN OUR VERSION OF THE REAL METHOD
                    {
                        DoomExileBeginLikeNormal(__instance, num, num2);
                        return false;
                    }

                    //if exiled was overridden
                    _exiledWasOverridden = true;
                    __instance.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(__instance.Animate());

                    __instance.Player.UpdateFromEitherPlayerDataOrCache(OriginalExiledPlayer, PlayerOutfitType.Default, PlayerMaterial.MaskType.Exile, false);
                    // ACTION DELETED HERE, MIGHT NEED???

                    __instance.Player.ToggleName(false);
                    if (!__instance.useIdleAnim)
                    {
                        __instance.Player.SetCustomHatPosition(__instance.exileHatPosition);
                        __instance.Player.SetCustomVisorPosition(__instance.exileVisorPosition);
                    }
                    if (OriginalExiledPlayer.Role.IsImpostor)
                    {
                        num--;
                    }


                    if (num == 1)
                    {
                        __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorsRemainS, num);
                    }
                    else
                    {
                        __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorsRemainP, num);
                    }

                    return false;
                }
            }
            // IF THEY VOTED CORRECTLY, OR IF IMPOSTOR NOT VOTED OUT, THEY CAN VOTE OUT WHOEVER THEY WANT AND RUN THE REAL METHOD

            //ACTUALLY INSTEADDDDD, WE USE THEIR CODE AND GO TO THE NEXT THING. THEN PREVENT THEIR METHOD FROM RUNNING.
            DoomExileBeginLikeNormal(__instance, num, num2);
            return false;
        }

        public static void DoomExileBeginLikeNormal(ExileController __instance, int num, int num2)
        {

            if (__instance.specialInputHandler != null)
            {
                __instance.specialInputHandler.disableVirtualCursor = true;
            }
            ExileController.Instance = __instance;
            ControllerManager.Instance.CloseAndResetAll();
            if (OriginalExiledPlayer != null)
            {
                __instance.exiled = OriginalExiledPlayer;
            }
            __instance.Text.gameObject.SetActive(false);
            __instance.Text.text = string.Empty;

            if (OriginalExiledPlayer != null)
            {
                if (!GameManager.Instance.LogicOptions.GetConfirmImpostor())
                {
                    __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextNonConfirm, OriginalExiledPlayer.PlayerName);
                }
                else if (OriginalExiledPlayer.Role.IsImpostor)
                {
                    if (num2 > 1)
                    {
                        __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPP, OriginalExiledPlayer.PlayerName);
                    }
                    else
                    {
                        __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSP, OriginalExiledPlayer.PlayerName);
                    }
                }
                else if (num2 > 1)
                {
                    __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPN, OriginalExiledPlayer.PlayerName);
                }
                else
                {
                    __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSN, OriginalExiledPlayer.PlayerName);
                }

                __instance.Player.UpdateFromEitherPlayerDataOrCache(OriginalExiledPlayer, PlayerOutfitType.Default, PlayerMaterial.MaskType.Exile, false);
                // ACTION DELETED HERE, MIGHT NEED???

                __instance.Player.ToggleName(false);
                if (!__instance.useIdleAnim)
                {
                    __instance.Player.SetCustomHatPosition(__instance.exileHatPosition);
                    __instance.Player.SetCustomVisorPosition(__instance.exileVisorPosition);
                }
                if (OriginalExiledPlayer.Role.IsImpostor)
                {
                    num--;
                }
            }
            else
            {
                if (OriginalTie)
                {
                    __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoExileTie);
                }
                else
                {
                    __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoExileSkip);
                }
                __instance.Player.gameObject.SetActive(false);
            }
            if (num == 1)
            {
                __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorsRemainS, num);
            }
            else
            {
                __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorsRemainP, num);
            }
            __instance.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(__instance.Animate());
        }

        [HarmonyPostfix]
        [HarmonyPatch("Begin")]
        public static void PostfixBegin(ExileController __instance)
        {
            // Calculate num crewmates
            int numCrewmates = 0;
            foreach (GameData.PlayerInfo p in GameData.Instance.AllPlayers)
            {
                if (!p.Role.IsImpostor && !p.Disconnected)
                {
                    numCrewmates++;
                }
            }
            int numCrewVotesNeeded = (int)System.Math.Ceiling(numCrewmates * DoomScrollVictoryManager.PercentCorrectHeadlinesNeeded);

            //
            //  ADD EDITED VOTING STRING HERE, AND DON'T LET THEM VOTE IMPOSTOR OUT UNTIL THEY GET ALL HEADLINE VOTES CORRECT
            //
            if (_exiledWasOverridden)
            {
                __instance.completeString = _retainedExileString;
            }
            if (!DoomScrollVictoryManager.CheckVotingSuccess())
            {
                if (OriginalExiledPlayer != null && OriginalExiledPlayer.Role.IsImpostor)
                {
                    __instance.completeString += $"\nHOWEVER, {OriginalExiledPlayer.PlayerName} was not ejected, because Crewmates cannot succeed until\n{numCrewVotesNeeded} crewmates vote correctly for all Headlines.";
                }
                else
                {
                    __instance.completeString += $"Crewmates cannot succeed until\n{numCrewVotesNeeded} crewmates vote correctly for all Headlines.";
                }
            }
            else
            {
                __instance.completeString += "\nCrewmates have voted correctly for all current Headlines,\nand can now win the game through voting or tasks.";
            }


            List<System.Tuple<int, string, string>> scoresByNumCorrect = new List<System.Tuple<int, string, string>>();
            int currentHighScore = 0;
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                byte pID = player.PlayerId;
                if (HeadlineDisplay.Instance.PlayerScores.ContainsKey(pID) && !player.Disconnected)
                {
                    int currentScore = HeadlineDisplay.Instance.PlayerScores[pID].Item1;
                    DoomScroll._log.LogInfo("Current numScore: " + currentScore.ToString());
                    DoomScroll._log.LogInfo("LastMeetingNewsItemsCount: " + DoomScrollVictoryManager.LastMeetingNewsItemsCount.ToString());
                    if (scoresByNumCorrect.Count == 0)
                    {
                        scoresByNumCorrect.Add(new System.Tuple<int, string, string>(currentScore, player.PlayerName, HeadlineDisplay.Instance.CalculateScoreStrings(player.PlayerId).Trim(' ', '\n', '\t', '[', ']')));
                    }
                    else
                    {
                        for (int i = 0; i < scoresByNumCorrect.Count; i++)
                        {
                            DoomScroll._log.LogInfo("Count is " + scoresByNumCorrect.Count.ToString() + " and current i is " + i.ToString());
                            DoomScroll._log.LogInfo("Loop " + i.ToString() + "of scoresByNumCorrect");
                            if (scoresByNumCorrect[i].Item1 < currentScore)
                            {
                                scoresByNumCorrect.Insert(i, new System.Tuple<int, string, string>(currentScore, player.PlayerName, HeadlineDisplay.Instance.CalculateScoreStrings(player.PlayerId).Trim(' ', '\n', '\t', '[', ']')));
                                break;
                            }
                            else if (i == scoresByNumCorrect.Count - 1)
                            {
                                scoresByNumCorrect.Add(new System.Tuple<int, string, string>(currentScore, player.PlayerName, HeadlineDisplay.Instance.CalculateScoreStrings(player.PlayerId).Trim(' ', '\n', '\t', '[', ']')));
                                break;
                            }
                        }
                    }
                }
            }


            DoomScroll._log.LogInfo("Things in scoresByNumCorrect: ");
            string votingResultsText = "<b>Headline Voting Scores:</b>\n";
            foreach (System.Tuple<int, string, string> thing in scoresByNumCorrect)
            {
                DoomScroll._log.LogInfo(thing.Item3);
                votingResultsText += thing.Item2 + ": " + thing.Item3 + "\n";
            }


            string scoreboardText = "<b>Rankings:</b>\n";
            foreach (System.Tuple<int, string, string> thing in scoresByNumCorrect)
            {
                scoreboardText += thing.Item2 + "\n";
            }

            CustomText votingResults = new CustomText(__instance.Text.gameObject, "Voting Results CustomText", votingResultsText);
            votingResults.SetLocalPosition(new Vector3(-2f, -1.3f, -10f));
            votingResults.SetSize(1.5f);
            votingResults.SetColor(Color.yellow);

            CustomText scoreBoard = new CustomText(__instance.Text.gameObject, "Scoreboard CustomText", scoreboardText);
            scoreBoard.SetLocalPosition(new Vector3(2f, -1.3f, -10f));
            scoreBoard.SetSize(1.5f);
            scoreBoard.SetColor(Color.yellow);

            GameDataPatch.UpdateDummyVotingTaskCompletion(DoomScrollVictoryManager.CheckVotingSuccess());
            DoomScrollVictoryManager.VotingTaskCompleteAsOfLastMeeting = DoomScrollVictoryManager.CheckVotingSuccess();

        }

        [HarmonyPrefix]
        [HarmonyPatch("WrapUp")]
        public static void PrefixWrapUp(ExileController __instance)
        {
            //Reset exile stuff
            if (OriginalExiledPlayer != null && !_exiledWasOverridden)
            {
                foreach (PlayerControl exiled in PlayerControl.AllPlayerControls)
                {
                    if (exiled.PlayerId == OriginalExiledPlayer.PlayerId)
                    {
                        DoomScroll._log.LogInfo("Calling DoomScroll exiled!!!");
                        PlayerControlPatch.DoomScrollExiled(exiled);
                    }
                }
            }

            if (_exiledWasOverridden)
            {
                __instance.ReEnableGameplay();
            }

            OriginalExiledPlayer = null;
            _exiledWasOverridden = false;
        }
    }
}
