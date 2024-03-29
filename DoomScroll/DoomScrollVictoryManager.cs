﻿using AmongUs.GameOptions;
using Doom_Scroll.Common;
using Doom_Scroll.Patches;
using Hazel;
using System.Collections.Generic;

namespace Doom_Scroll
{
    public static class DoomScrollVictoryManager
    {
        public static readonly float PercentCorrectHeadlinesNeeded = 0.75f;
        public static int LastMeetingNewsItemsCount = 0;
        public static bool VotingTaskCompleteAsOfLastMeeting = false;
        public static void Reset()
        {
            LastMeetingNewsItemsCount = 0;
            VotingTaskCompleteAsOfLastMeeting= false;
        }
        public static bool CheckVictory()
        {
            if (CheckLocalImpostor())
            {
                return SecondaryWinConditionManager.LocalPLayerSWC.CheckSuccess();
            }
            else
            {
                return (CheckVotingSuccess() && SecondaryWinConditionManager.LocalPLayerSWC.CheckSuccess());
            }
        }
        public static bool CheckLocalImpostor()
        {
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                if (PlayerControl.LocalPlayer.PlayerId == player.PlayerId)
                {
                    if (player.Role.IsImpostor)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        public static bool CheckVotingSuccess()
        {
            int potentialVoters = 0;
            int wrongVotes = 0;
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                byte pID = player.PlayerId;
                if (HeadlineDisplay.Instance.PlayerScores.ContainsKey(pID) && !player.Role.IsImpostor && !player.Disconnected)
                {
                    potentialVoters++;

                    int currentScore = HeadlineDisplay.Instance.PlayerScores[pID].Item1;
                    DoomScroll._log.LogInfo("Current numScore: " + currentScore.ToString());
                    DoomScroll._log.LogInfo("LastMeetingNewsItemsCount: " + LastMeetingNewsItemsCount.ToString());
                    if (currentScore < LastMeetingNewsItemsCount)
                    {
                        wrongVotes++;
                    }
                }
            }
            if (((float)wrongVotes) / potentialVoters > (1f-PercentCorrectHeadlinesNeeded))
            {
                return false;
            }
            return true;
        }

        public static bool CalculateSWCVictory()
        {
            if (SecondaryWinConditionManager.LocalPLayerSWC.CheckSuccess())
            {
                return true;
            }
            return false;
        }
    }
}
