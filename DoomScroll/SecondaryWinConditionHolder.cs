using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;

namespace Doom_Scroll
{
    public static class SecondaryWinConditionHolder
    {
        private static PlayerSWCTracker playerSWC;
        private static byte playerID;
        private static List<string> playerSWCList = new List<string>();

        public static void assignPlayerID(byte id)
        {
            playerID = id;
        }

        public static byte getPlayerID()
        {
            return playerID;
        }

        public static void assignLocalPlayerSWCTracker(PlayerSWCTracker tracker)
        {
            playerSWC = tracker;
        }

        public static void addToPlayerSWCList(string playerSWCResultsText)
        {
            playerSWCList.Add(playerSWCResultsText);
        }

        public static void clearPlayerSWCList()
        {
            playerSWCList = new List<string>();
        }

        public static void checkTargetVotedOut(GameData.PlayerInfo votedOutPlayer)
        {
            byte playerSWCTargetID = playerSWC.getSWC().getPlayerSWCTarget();
            if (votedOutPlayer.PlayerId == playerSWCTargetID)
            {
                playerSWC.getSWC().targetWasVotedOut();
            }
        }

        public static SecondaryWinCondition getThisPlayerSWC()
        {
            if (playerSWC.getPlayerID() == PlayerControl.LocalPlayer._cachedData.PlayerId)
            {
                return playerSWC.getSWC();
            }
            return null;
        }

        public static PlayerSWCTracker getThisPlayerTracker()
        {
            if (playerSWC.getPlayerID() == PlayerControl.LocalPlayer._cachedData.PlayerId)
            {
                return playerSWC;
            }
            return null;
        }

        public static string overallSWCResultsText() // text to put in to TMP object at end, when vicotory/defeat and success/failure for all players is revealed
        {
            string overallResults = "";
            foreach (string swcPlayerResults in playerSWCList)
            {
                if (swcPlayerResults != "")
                {
                    overallResults += swcPlayerResults; // will add each player's sent string, in the format of: "PlayerName Goal TargetName: SuccessOrFailure"
                }
            }
            return overallResults;
        }
    }
}
