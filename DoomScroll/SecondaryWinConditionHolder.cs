using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;

namespace DoomScroll
{
    public static class SecondaryWinConditionHolder
    {
        private static List<PlayerSWCTracker> playerSWCList = new List<PlayerSWCTracker>();

        public static void addToPlayerSWCList(PlayerSWCTracker playerSWCInfo)
        {
            playerSWCList.Add(playerSWCInfo);
        }

        public static void clearPlayerSWCList()
        {
            playerSWCList = new List<PlayerSWCTracker>();
        }

        public static void checkTargetVotedOut(GameData.PlayerInfo votedOutPlayer)
        {
            foreach (PlayerSWCTracker playerSWC in playerSWCList)
            {
                byte playerSWCTargetID = playerSWC.getSWC().getPlayerSWCTarget();
                if (votedOutPlayer.PlayerId == playerSWCTargetID)
                {
                    playerSWC.getSWC().targetWasVotedOut();
                }
            }
        }

        public static SecondaryWinCondition getSomePlayerSWC(byte thisPlayerID)
        {
            foreach (PlayerSWCTracker swcAndPlayer in playerSWCList)
            {
                if (swcAndPlayer.getPlayerID() == thisPlayerID)
                {
                    return swcAndPlayer.getSWC();
                }
            }
            return null;
        }

        public static string overallSWCResultsText() // text to put in to TMP object at end, when vicotory/defeat and success/failure for all players is revealed
        {
            string overallResults = "";
            foreach (PlayerSWCTracker swcAndPlayer in playerSWCList)
            {
                if (swcAndPlayer.getSWC().getPlayerSWCGoal() != SecondaryWinCondition.Goal.None)
                {
                    overallResults += swcAndPlayer.getPlayerName() + " " + swcAndPlayer.getSWC().SWCResultsText() + "\n"; // will add a string in the format of: "PlayerName Goal TargetName: SuccessOrFailure"
                }
            }
            return overallResults;
        }
    }
}
