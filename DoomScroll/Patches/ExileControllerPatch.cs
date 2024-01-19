using Doom_Scroll.UI;
using HarmonyLib;
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
        private static bool _exiledWasModified;

        [HarmonyPrefix]
        [HarmonyPatch("Begin")]
        public static bool PrefixBegin(ExileController __instance, object[] __args)
        {

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
                __instance.exiled = null;

                if (OriginalExiledPlayer != null)
                {
                    DoomScroll._log.LogInfo(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> OriginalExiledPlayer: " + OriginalExiledPlayer.ToString());
                    //CALLS SAME STUFF AS ACTUAL THING, BUT THEN ADDS MESSAGE ABOUT VOTING TO THE END

                    //^SIKE, NOT ANYMORE. THAT WAS A LIE.


                    if (OriginalExiledPlayer.Role.IsImpostor)
                    {
                        if (num2 > 1)
                        {
                            if (num > 1) // If the amount of living impostors left is > 1; basically, if the game doesn't end from them voting out this impostor, SET THE EXILED PLAYER BACK TO NORMAL AND RUN THE REAL METHOD
                            {
                                __args[0] = OriginalExiledPlayer;
                                __instance.exiled = OriginalExiledPlayer;
                                return true;
                            }
                            _retainedExileString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPP, (OriginalExiledPlayer.PlayerName));
                        }
                        else
                        {
                            _retainedExileString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSP, (OriginalExiledPlayer.PlayerName));
                        }
                    }
                    else // IF THEY VOTED SOMEONE OUT AND THEY AREN'T WINNING FROM IT, SET THE EXILED PLAYER BACK TO NORMAL AND RUN THE REAL METHOD
                    {
                        __args[0] = OriginalExiledPlayer;
                        __instance.exiled = OriginalExiledPlayer;
                        return true;
                    }

                    _exiledWasModified = true;
                    __instance.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(__instance.Animate());


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
            // IF THEY VOTED CORRECTLY, THEY CAN VOTE OUT WHOEVER THEY WANT AND RUN THE REAL METHOD
            __args[0] = OriginalExiledPlayer;
            __instance.exiled = OriginalExiledPlayer;
            __args[1] = OriginalTie;
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch("Begin")]
        public static void PostfixBegin(ExileController __instance)
        {
            //
            //  ADD EDITED VOTING STRING HERE, AND DON'T LET THEM VOTE IMPOSTOR OUT UNTIL THEY GET ALL HEADLINE VOTES CORRECT
            //
            if (_exiledWasModified)
            {
                __instance.completeString = _retainedExileString;
            }
            if (!DoomScrollVictoryManager.CheckVotingSuccess())
            {
                if (OriginalExiledPlayer != null && OriginalExiledPlayer.Role.IsImpostor)
                {
                    __instance.completeString += $"\nHOWEVER, {OriginalExiledPlayer.PlayerName} was not ejected, because Crewmates cannot succeed until\nall crewmates vote correctly for all Headlines.";
                }
                else
                {
                    __instance.completeString += $"\nHOWEVER, Crewmates cannot succeed until\nall crewmates vote correctly for all Headlines.";
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
            if (OriginalExiledPlayer != null && !_exiledWasModified)
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

            if (_exiledWasModified)
            {
                __instance.ReEnableGameplay();
            }

            OriginalExiledPlayer = null;
            _exiledWasModified = false;
        }
    }
}
