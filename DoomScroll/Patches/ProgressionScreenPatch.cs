using HarmonyLib;
using UnityEngine;
using Doom_Scroll.UI;
using Doom_Scroll.Common;
using System.Reflection;
using System.Collections.Generic;
using Il2CppSystem.Collections;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(ProgressionScreen))]
    class ProgressionScreenPatch
    {
        public static bool progressionScreenOpen = false;
        private static float fontSize = 2f;
        private static string calculateEndorsementScores()
        {
            int numCorrect = 0;
            int numIncorrect = 0;
            List<NewsItem> newsList = NewsFeedManager.Instance.GetAllNewsList();
            foreach (NewsItem newsPost in newsList)
            {
                if (newsPost.EndorsedCorrectly() == 1)
                {
                    numCorrect++;
                }
                else if (newsPost.EndorsedCorrectly() == -1)
                {
                    numIncorrect++;
                }
            }
            return $"<size=120%>News Endorsement Scores:</size>\r\n" +
                $"You correctly voted true or false on {numCorrect}/{newsList.Count} news posts\r\n" +
                $"You incorrectly voted true or false on {numIncorrect}/{newsList.Count} news posts";
        }

        [HarmonyPostfix]
        [HarmonyPatch("Activate")]
        public static void PostfixActivate(ProgressionScreen __instance)
        {
            progressionScreenOpen = true;

            DoomScroll._log.LogInfo("On Progression Screen, playerSWClist = " + SecondaryWinConditionManager.OverallSWCResultsText());
            CustomText overallSWCText = new CustomText(__instance.XpBar.gameObject, "SWCResults", SecondaryWinConditionManager.OverallSWCResultsText());
            overallSWCText.SetColor(Color.white);
            float size = fontSize;
            if (SecondaryWinConditionManager.GetSWCList().Count >= 8)
            {
                size = fontSize / 2f;
            }
            overallSWCText.SetSize(size);
            Vector3 textPos = new Vector3(overallSWCText.UIGameObject.transform.localPosition.x, overallSWCText.UIGameObject.transform.localPosition.y - 0.9f, overallSWCText.UIGameObject.transform.localPosition.z);
            overallSWCText.SetLocalPosition(textPos);

            //Voting info
            CustomText newsVotingResultsText = new CustomText(__instance.XpBar.gameObject, "NewsVotingResults", calculateEndorsementScores());
            newsVotingResultsText.SetColor(Color.white);
            newsVotingResultsText.SetSize(fontSize);
            Vector3 newsTextPos = new Vector3(overallSWCText.UIGameObject.transform.localPosition.x + 2f, overallSWCText.UIGameObject.transform.localPosition.y + 1f, overallSWCText.UIGameObject.transform.localPosition.z);
            newsVotingResultsText.SetLocalPosition(newsTextPos);
            SecondaryWinConditionManager.Reset();
        }
    }
}
