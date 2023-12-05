using Doom_Scroll.UI;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(ExileController))]
    static class ExileControllerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Begin")]
        public static void PostfixBegin(ExileController __instance)
        {
            List<System.Tuple<int, string, string>> scoresByNumCorrect = new List<System.Tuple<int, string, string>>();
            int currentHighScore = 0;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                byte pID = player.PlayerId;
                if (HeadlineDisplay.Instance.PlayerScores.ContainsKey(pID))
                {
                    int currentScore = HeadlineDisplay.Instance.PlayerScores[pID].Item1;
                    DoomScroll._log.LogInfo("Current numScore: " + currentScore.ToString());
                    DoomScroll._log.LogInfo("LastMeetingNewsItemsCount: " + DoomScrollVictoryManager.LastMeetingNewsItemsCount.ToString());
                    if (scoresByNumCorrect.Count == 0)
                    {
                        scoresByNumCorrect.Add(new System.Tuple<int, string, string>(currentScore, player.name, HeadlineDisplay.Instance.CalculateScoreStrings(player.PlayerId).Trim(' ', '\n', '\t', '[', ']')));
                    }
                    else
                    {
                        for (int i = 0; i < scoresByNumCorrect.Count; i++)
                        {
                            DoomScroll._log.LogInfo("Count is " + scoresByNumCorrect.Count.ToString() + " and current i is " + i.ToString());
                            DoomScroll._log.LogInfo("Loop " + i.ToString() + "of scoresByNumCorrect");
                            if (scoresByNumCorrect[i].Item1 < currentScore)
                            {
                                scoresByNumCorrect.Insert(i, new System.Tuple<int, string, string>(currentScore, player.name, HeadlineDisplay.Instance.CalculateScoreStrings(player.PlayerId).Trim(' ', '\n', '\t', '[', ']')));
                                break;
                            }
                            else if (i == scoresByNumCorrect.Count - 1)
                            {
                                scoresByNumCorrect.Add(new System.Tuple<int, string, string>(currentScore, player.name, HeadlineDisplay.Instance.CalculateScoreStrings(player.PlayerId).Trim(' ', '\n', '\t', '[', ']')));
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
                votingResultsText += thing.Item2 + ": " + thing.Item3;
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
        }
    }
}
