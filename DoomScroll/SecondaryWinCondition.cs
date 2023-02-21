using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;
using UnityEngine;
using Doom_Scroll.UI;

namespace Doom_Scroll
{
    public static class SecondaryWinCondition
    {
        private static Goal playerSWCGoal;
        private static byte playerSWCTarget;
        private static bool targetVotedOut;
        private static bool swcSuccess;
        private static bool gameRunning = false;
        public static CustomText m_overallSWCText;

        private static byte playerID;
        private static List<string> playerSWCList;

        //public static CustomButton test_button;

        public enum Goal
        {
            Protect,
            Frame,
            None
        }

        public static void InitSecondaryWinCondition()
        // Called in Start() in ShipStatus.
        {
            //SWCH stuff
            playerID = PlayerControl.LocalPlayer.PlayerId;
            playerSWCList = new List<string>();
            //SWC stuff
            assignGoal();
            targetVotedOut = false;
            swcSuccess = false;
            gameRunning = true;
            assignTarget(playerID); 
        }

        public static void assignImpostorValues()
        // Called in PrefixBeginImpostor in IntroCutscene if player gets impostor role.
        // Overwrites SWC values assigned in initSecondaryWinCondition at game start.
        {
            playerSWCGoal = Goal.None;
            playerSWCTarget = byte.MaxValue;
        }

        public static bool checkGameRunning()
        {
            return gameRunning;
        }

        public static Goal getPlayerSWCGoal()
        {
            return playerSWCGoal;
        }

        public static byte getPlayerSWCTarget()
        {
            return playerSWCTarget;
        }

        public static string getTargetName()
        {
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (playerInfo.PlayerId == playerSWCTarget)
                {
                    return playerInfo.PlayerName;
                }
            }
            return "";
        }

        public static void gameOver()
        // Called in ___
        {
            //SWCH stuff
            playerID = byte.MaxValue;
            playerSWCList = new List<string>();

            //SWC stuff
            playerSWCGoal = Goal.None;
            playerSWCTarget= byte.MaxValue;
            targetVotedOut = false;
            swcSuccess = false;
            gameRunning = false;
        }

        public static void assignGoal()
        {
            int goalNum = UnityEngine.Random.Range(1, 4); //min inclusive, max exclusive. Will return 1, 2, or 3
            if (goalNum == 1)
            {
                playerSWCGoal = Goal.Protect;
            }
            else if (goalNum == 2)
            {
                playerSWCGoal = Goal.Frame;
            }
            else
            {
                playerSWCGoal = Goal.None;
            }
        }

        public static void assignTarget(byte pID)
        {
            /*List< GameData.PlayerInfo > otherPlayers = GameData.Instance.AllPlayers;
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                
            }
            otherPlayers.Remove*/
            //List<GameData.PlayerInfo> shuffledPlayersMinusLocal = Extensions.Shuffle<GameData.PlayerInfo>(GameData.Instance.AllPlayers);
            //IList<GameData.PlayerInfo> playersMinusLocal = new List<GameData.PlayerInfo>(new IList<GameData.PlayerInfo>(GameData.Instance.AllPlayers));
            //playersMinusLocal.Remove(GameData.PlayerInfo.LocalPlayer);
            int numPlayers = GameData.Instance.AllPlayers.Count;
            int playerindex = -1;
            for (int i = 0; i < numPlayers; i++)
            {
                if (GameData.Instance.AllPlayers[i].PlayerId == pID)
                {
                    playerindex = i;
                    break;
                }
            }
            int targetNum = UnityEngine.Random.Range(0, numPlayers - 1);
            while (targetNum == playerindex)
            {
                targetNum = UnityEngine.Random.Range(0, numPlayers - 1);
            }

            for (int i = 0; i < numPlayers; i++)
            {
                if (i == targetNum)
                {
                    playerSWCTarget = GameData.Instance.AllPlayers[i].PlayerId;
                    break;
                }
            }

        }

        public static void Evaluate()
        {
            int numPlayers = GameData.Instance.AllPlayers.Count;
            if (playerSWCGoal == Goal.None)
            {
                swcSuccess = true;
            }
            else if (playerSWCGoal == Goal.Protect)
            {
                for (int i = 0; i < numPlayers; i++)
                {
                    if (playerSWCTarget == GameData.Instance.AllPlayers[i].PlayerId)
                    {
                        if (GameData.Instance.AllPlayers[i].IsDead) //player is dead
                        {
                            swcSuccess = false;
                        }
                        else //all other cases, i.e. player is not dead
                        {
                            swcSuccess = true;
                        }
                    }
                }
            }
            else if (playerSWCGoal == Goal.Frame)
            {
                if (targetVotedOut)
                {
                    swcSuccess = true;
                }
                else
                {
                    swcSuccess = false;
                }
            }
            else
            {
                //playerSWCGoal is null (unassigned)
                swcSuccess = false;
            }
        }

        public static string SWCResultsText() // text to put in to TMP object at end, when vicotory/defeat and success/failure is revealed
        {
            string results = "";
            if (playerSWCGoal != Goal.None)
            {
                if (swcSuccess)
                {
                    results = ToString() + ": <size=40%>Success</size>";
                }
                else if (!swcSuccess)
                {
                    results = ToString() + ": <size=40%>Failure</size>";
                }
            }
            // if swc goal is null (unassigned)
            return results;
        }

        public static string ToString()
        // returns the base SWC assignment text, the text to put into TMP object at beginning
        // when roles are assigned
        {
            if (playerSWCGoal == Goal.Protect)
            {
                return "Protect " + getTargetName();
            }
            else if (playerSWCGoal == Goal.Frame)
            {
                return "Frame " + getTargetName();
            }
            return "";
        }







        ///METHODS ADDED FROM SECONDARYWINCONDITIONHOLDER:
        ///


        

        public static byte getPlayerID()
        {
            return playerID;
        }

        public static void addToPlayerSWCList(string playerSWCResultsText)
        {
            playerSWCList.Add(playerSWCResultsText);
        }

        public static void checkTargetVotedOut(GameData.PlayerInfo votedOutPlayer)
        {
            if (votedOutPlayer.PlayerId == playerSWCTarget)
            {
                targetVotedOut = true;
            }
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







        ///METHODS ADDED FROM PLAYERSWCTRACKER:
        ///




        private static string getPlayerName()
        {
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                if (playerInfo.PlayerId == playerID)
                {
                    return playerInfo.PlayerName;
                }
            }
            return "";
        }

        public static string sendableResultsText()
        {
            if (playerSWCGoal == Goal.None)
            {
                return "";
            }
            else
            {
                return getPlayerName() + " " + SWCResultsText() + "\n"; // will create a string in the format of: "PlayerName Goal TargetName: SuccessOrFailure"
            }
        }
    }
}
