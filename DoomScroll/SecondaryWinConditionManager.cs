using AmongUs.GameOptions;
using Doom_Scroll.UI;
using System.Collections.Generic;

namespace Doom_Scroll
{
    public static class SecondaryWinConditionManager
    {
        public static SecondaryWinCondition LocalPLayerSWC { get; set; }  // referencing local player swc
        private static List<SecondaryWinCondition> playerSWCList = new List<SecondaryWinCondition>();  // list of SecondaryWinConditions instead of strings


        public static void SetSecondaryWinConditions() // only called for the host
        {
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                InitSecondaryWinCondition(player.PlayerId, player.RoleType == RoleTypes.Impostor);
            }
        }
        public static void InitSecondaryWinCondition(byte id, bool isImpostor)
        {
            // byte localPlayer = PlayerControl.LocalPlayer.PlayerId;
            Goal localPlayerGoal = isImpostor ? Goal.None : AssignGoal();
            byte localPlayerTarget = isImpostor? byte.MaxValue : AssignTarget();
            SecondaryWinCondition swc = new SecondaryWinCondition(id, localPlayerGoal, localPlayerTarget);
            AddToPlayerSWCList(swc);
            // RPC local swc to others
            swc.RPCSendSWC();
            if (id == PlayerControl.LocalPlayer.PlayerId)
            {
                LocalPLayerSWC = swc;
            }
        }

        public static void Reset()
        {
            playerSWCList = new List<SecondaryWinCondition>();
            LocalPLayerSWC = null;
        }

        public static void AddToPlayerSWCList(SecondaryWinCondition swc)
        {
            playerSWCList.Add(swc);
            DoomScroll._log.LogInfo("SWC added: " + swc.SendableResultsText());  // debug
        }

        public static void UpdateSWCList(byte targetId, DeathReason reason)  // upadtes the dead targets and evaluates success
        {
            foreach (SecondaryWinCondition swc in playerSWCList) 
            {
                swc.TargetDead(targetId, reason);
            }
        }

        public static string OverallSWCResultsText() // text to put in to TMP object at end, when vicotory/defeat and success/failure for all players is revealed
        {
            string overallResults = "";
            foreach (SecondaryWinCondition swc in playerSWCList)
            {
                overallResults += swc.SendableResultsText(); // will add each player's sent string, in the format of: "PlayerName Goal TargetName: SuccessOrFailure"
            }
            return overallResults;
        }

        // set up local player
        private static Goal AssignGoal()
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

        private static byte AssignTarget()
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
            int targetNum = UnityEngine.Random.Range(0, numPlayers);
            while (targetNum == playerindex)
            {
                targetNum = UnityEngine.Random.Range(0, numPlayers);
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

        public static List<SecondaryWinCondition> GetSWCList()
        {
            return playerSWCList;
        }
    }
}
