using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;

namespace DoomScroll
{
    public class SecondaryWinCondition
    {
        private Goal playerSWCGoal;
        private byte playerSWCTarget;
        private bool targetIsVotedOut = false;

        public SecondaryWinCondition(byte pID)
        {
            assignGoal();
            assignTarget(pID);
        }

        public Goal getPlayerSWCGoal()
        {
            return playerSWCGoal;
        }

        public byte getPlayerSWCTarget()
        {
            return playerSWCTarget;
        }

        public string getTargetName()
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

        public enum Goal
        {
            Protect,
            Frame,
            None
        }

        public void targetWasVotedOut()
        {
            targetIsVotedOut = true;
        }

        public void assignGoal()
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

        public void assignImpostorValues()
        {
            playerSWCGoal = Goal.None;
            playerSWCTarget = byte.MaxValue;
        }

        public void assignTarget(byte pID)
        {
            /*List< GameData.PlayerInfo > otherPlayers = GameData.Instance.AllPlayers;
            foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
            {
                
            }
            otherPlayers.Remove*/

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

        public bool CheckSuccess()
        {
            int numPlayers = GameData.Instance.AllPlayers.Count;
            if (playerSWCGoal == Goal.None)
            {
                return true;
            }
            else if (playerSWCGoal == Goal.Protect)
            {
                for (int i = 0; i < numPlayers; i++)
                {
                    if (playerSWCTarget == GameData.Instance.AllPlayers[i].PlayerId)
                    {
                        if (GameData.Instance.AllPlayers[i].IsDead) //player is dead
                        {
                            return false;
                        }
                        else //all other cases, i.e. player is not dead
                        {
                            return true;
                        }
                    }
                }
            }
            else if (playerSWCGoal == Goal.Frame)
            {
                if (targetIsVotedOut)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            //playerSWCGoal is null
            return false;
        }

        public string SWCAssignText() // text to put into TMP object at beginning, when roles are assigned
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

        public string SWCResultsText() // text to put in to TMP object at end, when vicotory/defeat and success/failure is revealed
        {
            if (playerSWCGoal != Goal.None)
                return SWCAssignText() + ": <size=40%>" + SWCSuccessMessage() + "</size>";
            else
                return "";
        }

        public string SWCSuccessMessage()
        {
            bool wasSuccessful = CheckSuccess();
            if (playerSWCGoal == Goal.None)
            {
                return "";
            }
            else if (wasSuccessful)
            {
                return "Success";
            }
            else if (!wasSuccessful)
            {
                return "Failure";
            }
            else
            {
                return "";
            }
        }
    }
}
