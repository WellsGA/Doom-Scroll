using AmongUs.GameOptions;
using Doom_Scroll.Common;
using Doom_Scroll.Patches;
using Hazel;
using System.Collections.Generic;

namespace Doom_Scroll
{
    public static class DoomScrollVictoryManager
    {
        public static readonly float PercentCorrectHeadlinesNeeded = 0.75f;
        public static bool IsHeadlineVoteSuccess { get;  private set; } = false;
        public static void Reset()
        {
            IsHeadlineVoteSuccess= false;
        }
        public static bool CheckVictory()
        {
            bool isSWCSuccess = SecondaryWinConditionManager.LocalPLayerSWC.CheckSuccess();
            if (PlayerControl.LocalPlayer.Data.RoleType == RoleTypes.Impostor)
            {
                return isSWCSuccess;
            }
            else
            {
                return IsHeadlineVoteSuccess && isSWCSuccess;
            }
        }
        
        public static void CheckVotingSuccess()
        {
            int potentialVoters = 0;
            int wrongVotes = 0;
            foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers)
            {
                byte pID = player.PlayerId;
                if (HeadlineDisplay.Instance.PlayerScores.ContainsKey(pID) && !player.Role.IsImpostor && !player.Disconnected)
                {
                    potentialVoters++;

                    int currentScore = HeadlineDisplay.Instance.PlayerScores[pID].Item1;
                    // check if they got it all right
                    if (currentScore < HeadlineDisplay.Instance.AllNewsList.Count)
                    {
                        wrongVotes++;
                    }
                }
            }
            if (((float)wrongVotes) / potentialVoters > (1f-PercentCorrectHeadlinesNeeded))
            {
                IsHeadlineVoteSuccess = false;
            }
            IsHeadlineVoteSuccess = true;
        }

    }
}
