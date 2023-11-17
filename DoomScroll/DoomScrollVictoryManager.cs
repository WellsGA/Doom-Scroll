using AmongUs.GameOptions;
using Doom_Scroll.Common;
using Doom_Scroll.Patches;
using Hazel;
using System.Collections.Generic;

namespace Doom_Scroll
{
    public static class DoomScrollVictoryManager
    {
        public static int LastMeetingNewsItemsCount = 0;
        public static void Reset()
        {
            LastMeetingNewsItemsCount = 0;
        }
        public static bool CheckVictory()
        {
            return (CheckVotingSuccess() && SecondaryWinConditionManager.LocalPLayerSWC.CheckSuccess());
        }
        public static bool CheckLocalImpostor()
        {
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                if (PlayerControl.LocalPlayer.PlayerId == player.PlayerId)
                {
                    if (player.RoleType == RoleTypes.Impostor || player.RoleType == RoleTypes.ImpostorGhost)
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
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                if (PlayerControl.LocalPlayer.PlayerId == player.PlayerId)
                {
                    if (player.RoleType == RoleTypes.Impostor || player.RoleType == RoleTypes.ImpostorGhost)
                    {
                        return true;
                    }
                    break;
                }
            }
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                byte pID = player.PlayerId;
                if (HeadlineDisplay.Instance.PlayerScores.ContainsKey(pID))
                {
                    string strippedScoreText = HeadlineDisplay.Instance.PlayerScores[pID].Trim(' ', '\n', '\t', '[', ']'); //new char[' ','\n','\t','[',']']
                    DoomScroll._log.LogInfo("Current strippedScoreText: " + strippedScoreText);
                    string currentScore = strippedScoreText.Substring(0, 2);
                    currentScore.TrimEnd();
                    int numScore = 0;
                    try
                    {
                        numScore = System.Int32.Parse(currentScore);
                    }
                    catch (System.Exception e)
                    {
                        DoomScroll._log.LogError("Couldn't parse number to string: [" + currentScore + "], error message " + e);
                    }
                    DoomScroll._log.LogInfo("Current numScore: " + numScore.ToString());
                    if (numScore < LastMeetingNewsItemsCount)
                    {
                        return false;
                    }
                }
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
