using AmongUs.GameOptions;
using Hazel;
using System.Collections.Generic;

namespace Doom_Scroll
{
    public static class SecondaryWinConditionManager
    {
        public static SecondaryWinCondition LocalPLayerSWC { get; set; }  // referencing local player swc
        private static List<SecondaryWinCondition> playerSWCList = new List<SecondaryWinCondition>();  // list of SecondaryWinConditions instead of strings

        public static void SetSecondaryWinConditions() // only called for the host in ShipStatus Begin()
        {
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                bool isImpostor =  player.RoleType == RoleTypes.Impostor ? true : false;
                Goal playerGoal = isImpostor ? Goal.None : AssignGoal();
                byte playerTarget = isImpostor ? byte.MaxValue : AssignTarget();
                SecondaryWinCondition swc = new SecondaryWinCondition(player.PlayerId, playerGoal, playerTarget);  
                AddToPlayerSWCList(swc); // add locally - host's list
                RPCSendSWC(swc); // send RPC swc to others
            }
        }

        public static void Reset()
        {
            playerSWCList = new List<SecondaryWinCondition>();
            LocalPLayerSWC = null;
        }

        public static void AddToPlayerSWCList(SecondaryWinCondition swc)
        {
            if (swc.GetPayerId() == PlayerControl.LocalPlayer.PlayerId)
            {
                LocalPLayerSWC = swc;
            }
            playerSWCList.Add(swc);
            DoomScroll._log.LogInfo("SWC added: " + swc.SendableResultsText());  // debug
        }

        public static void UpdateSWCList(byte targetId, DeathReason reason)  // upadtes the dead targets and evaluates success
        {
            foreach (SecondaryWinCondition swc in playerSWCList) 
            {
                if(swc.GetTargetId() == targetId)
                {
                    swc.TargetDead(reason);
                }
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
            int numPlayers = PlayerControl.AllPlayerControls.Count;
            int playerindex = PlayerControl.AllPlayerControls.IndexOf(PlayerControl.LocalPlayer);
            // select a target different from the local player
            int targetNum = UnityEngine.Random.Range(0, numPlayers);
            while (targetNum == playerindex)
            {
                targetNum = UnityEngine.Random.Range(0, numPlayers);
            }
            byte target = PlayerControl.AllPlayerControls[targetNum].PlayerId;
            return target;
        }

        public static List<SecondaryWinCondition> GetSWCList()
        {
            return playerSWCList;
        }

        //RPCs
        public static bool RPCDeathNote(byte plyaerID, DeathReason reason)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DEATHNOTE, (SendOption)1);
            messageWriter.Write(plyaerID);
            messageWriter.Write((byte)reason);
            messageWriter.EndMessage();
            return true;
        }
        public static bool RPCSendSWC(SecondaryWinCondition swc)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDSWC, (SendOption)1);
            messageWriter.Write(swc.GetPayerId());
            messageWriter.Write((byte)swc.GetGoal());
            messageWriter.Write(swc.GetTargetId());
            messageWriter.EndMessage();
            return true;
        }
    }
}
