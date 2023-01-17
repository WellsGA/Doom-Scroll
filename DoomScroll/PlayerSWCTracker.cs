using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;

namespace Doom_Scroll
{
    public class PlayerSWCTracker
    {
        private byte PlayerID;
        private SecondaryWinCondition SWC;

        public PlayerSWCTracker(byte pID)
        {
            PlayerID = pID;
            SWC = new SecondaryWinCondition(pID);
        }

        public void impostorSWC()
        {
            SWC.assignImpostorValues();
        }

        public byte getPlayerID()
        {
            return PlayerID;
        }

        public SecondaryWinCondition getSWC()
        {
            return SWC;
        }

        public string getPlayerName()
        {
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (playerInfo.PlayerId == PlayerID)
                {
                    return playerInfo.PlayerName;
                }
            }
            return "";
        }

        public string sendableResultsText()
        {
            if (SWC.getPlayerSWCGoal() != SecondaryWinCondition.Goal.None)
            {
                return "";
            }
            else 
            {
                return getPlayerName() + " " + SWC.SWCResultsText() + "\n"; // will create a string in the format of: "PlayerName Goal TargetName: SuccessOrFailure"
            }
        }
    }
}
