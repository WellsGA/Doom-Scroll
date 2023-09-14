using Doom_Scroll.Common;
using Doom_Scroll.Patches;
using Hazel;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

namespace Doom_Scroll
{
    public static class SecondaryWinConditionManager
    {
        public static SecondaryWinCondition LocalPLayerSWC { get; set; }  // referencing local player swc
        private static List<SecondaryWinCondition> playerSWCList = new List<SecondaryWinCondition>();  // list of SecondaryWinConditions instead of strings

        public static void SetSecondaryWinConditions() // only called for the host in ShipStatus Begin()
        {
            GameLogger.Write(GameLogger.GetTime() + " - Secondary Win Conditions"); 
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                Goal playerGoal = AssignGoal();
                byte playerTarget = AssignTarget(player.PlayerId);
                SecondaryWinCondition swc = new SecondaryWinCondition(player.PlayerId, playerGoal, playerTarget);  
                AddToPlayerSWCList(swc); // add locally - host's list
                RPCSendSWC(swc); // send RPC swc to others

                // game log
                GameLogger.Write("\t" + player.PlayerName + ": " + playerGoal + " " + swc.GetTargetName());

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

        public static SecondaryWinCondition GetSwcByPlayerID(byte id)
        {
            foreach (SecondaryWinCondition swc in playerSWCList)
            {
                if (swc.GetPayerId() == id)
                {
                    return swc;
                }
            }
            return null;
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

            // game log
            if (AmongUsClient.Instance.AmHost)
            {
                string name = GameData.Instance.GetPlayerById(targetId).PlayerName;
                GameLogger.Write(GameLogger.GetTime() + " - " + name + " is dead. Reason: " + reason.ToString());
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
            int goalNum = UnityEngine.Random.Range(1, 3); //min inclusive, max exclusive. Will return 1 or 2
            if (goalNum == 1)
            {
                return Goal.Protect;
            }
            else
            {
                return Goal.Frame;
            }
        }

        private static byte AssignTarget(byte id)
        {
            // int playerindex = PlayerControl.AllPlayerControls.IndexOf(playerInfo.Object); // accesses a private variable? We'll see if it works.  
            int numPlayers = PlayerControl.AllPlayerControls.Count;
            // select a target different from the current player being assigned
            int targetNum = UnityEngine.Random.Range(0, numPlayers);
            byte target = PlayerControl.AllPlayerControls[targetNum].PlayerId;
            while (target == id)
            {
                targetNum = UnityEngine.Random.Range(0, numPlayers);
                target = PlayerControl.AllPlayerControls[targetNum].PlayerId;
            }
           
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
