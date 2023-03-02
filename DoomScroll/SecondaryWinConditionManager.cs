using Doom_Scroll.UI;
using System.Collections.Generic;

namespace Doom_Scroll
{
    public static class SecondaryWinConditionManager
    {
        public static SecondaryWinCondition LocalPLayerSWC { get; private set; }
        private static List<SecondaryWinCondition> playerSWCList = new List<SecondaryWinCondition>();  // list of SecondaryWinConditions instead of strings

        public static void InitSecondaryWinCondition(bool isImpostor)
        {
            // set local swc
            byte localPlayer = PlayerControl.LocalPlayer.PlayerId;
            Goal localPlayerGoal = isImpostor ? Goal.None : assignGoal();
            byte localPlayerTarget = isImpostor? byte.MaxValue : assignTarget();
            LocalPLayerSWC = new SecondaryWinCondition(localPlayer, localPlayerGoal, localPlayerTarget);
            addToPlayerSWCList(LocalPLayerSWC);

            ///RPC local swc to others
            LocalPLayerSWC.RPCSendSWC();
        }

        public static void gameOver()
        {
            playerSWCList = new List<SecondaryWinCondition>();
            LocalPLayerSWC = null;
        }

        public static void addToPlayerSWCList(SecondaryWinCondition swc)
        {
            playerSWCList.Add(swc);
            DoomScroll._log.LogInfo("SWC added: " + swc.SendableResultsText());  // debug
        }

        public static void UpdateSWCList(byte targetId, DeathReason reason)  // upadtes the dead targets and evaluates sucess
        {
            foreach (SecondaryWinCondition swc in playerSWCList) 
            {
                swc.TargetDead(targetId, reason);
            }
        }

        public static string overallSWCResultsText() // text to put in to TMP object at end, when vicotory/defeat and success/failure for all players is revealed
        {
            string overallResults = "";
            foreach (SecondaryWinCondition swc in playerSWCList)
            {
                overallResults += swc.SWCResultsText() + '\n'; // will add each player's sent string, in the format of: "PlayerName Goal TargetName: SuccessOrFailure"
            }
            return overallResults;
        }

        // set up local player
        private static Goal assignGoal()
        {
            int goalNum = UnityEngine.Random.Range(1, 4); //min inclusive, max exclusive. Will return 1, 2, or 3
            if (goalNum == 1)
            {
                return Goal.Protect;
            }
            else if (goalNum == 2)
            {
                return Goal.Frame;
            }
            else
            {
                return Goal.None;
            }
        }

        private static byte assignTarget()
        {
            int numPlayers = GameData.Instance.AllPlayers.Count;
            byte target = byte.MaxValue;
            int playerindex = -1;
            for (int i = 0; i < numPlayers; i++)
            {
                if (GameData.Instance.AllPlayers[i].PlayerId == PlayerControl.LocalPlayer.PlayerId)
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
                    target = GameData.Instance.AllPlayers[i].PlayerId;
                }
            }
            return target;
        }
    }
}
