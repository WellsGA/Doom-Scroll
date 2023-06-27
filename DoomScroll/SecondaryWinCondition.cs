using Hazel;

namespace Doom_Scroll
{
    public enum Goal : byte
    {
        Protect,
        Frame,
        None
    }
    public enum TargetState : byte
    {
        ALIVE,
        KILLED,
        VOTEDOUT,
        DISCONNECTED
    }
    public class SecondaryWinCondition
    {
        private byte playerID;
        private Goal playerSWCGoal;
        private byte playerSWCTarget;
        private TargetState targetState;

        private bool swcSuccess;

        public SecondaryWinCondition(byte player, Goal goal, byte target)
        {
            playerID = player;
            playerSWCGoal = goal;
            playerSWCTarget = target;
            targetState = TargetState.ALIVE;
            swcSuccess = goal != Goal.Frame  ? true : false; // if protect or none they start on success
        }

        public void Evaluate()
        {
            if (playerSWCGoal == Goal.Protect)
            {
                if (targetState != TargetState.ALIVE) //player is dead
                 {
                     DoomScroll._log.LogInfo("Protect failed.");
                      swcSuccess = false;
                 }
            }
            else if (playerSWCGoal == Goal.Frame)
            {
                if (targetState == TargetState.VOTEDOUT)
                {
                    DoomScroll._log.LogInfo("Frame successful.");
                    swcSuccess = true;
                }
            }
        }
        private string GetTargetName() // same as GetPlayerName, would worth to use only GetPlayerName with an id parameter
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

        private string GetPlayerName()
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

        public byte GetPayerId()
        {
            return playerID;
        }
        ///METHODS ADDED FROM SECONDARYWINCONDITIONHOLDER: 

        // replace voded out function with die patch - death reason
        public void TargetDead(byte id, DeathReason reason)
        {
            if(playerSWCTarget == id)
            {
                switch (reason)
                {
                    case DeathReason.Kill:
                        targetState = TargetState.KILLED;
                        break;
                    case DeathReason.Exile:
                        targetState = TargetState.VOTEDOUT;
                        break;
                    case DeathReason.Disconnect:            // This is not evaluated correctly rn! 
                    default:
                        targetState = TargetState.DISCONNECTED;
                        break;
                }
                Evaluate();
            }
        }

        public override string ToString()
        // returns the base SWC assignment text, the text to put into TMP object at beginning
        // when roles are assigned
        {
            if (playerSWCGoal == Goal.Protect)
            {
                return "Protect " + GetTargetName();
            }
            else if (playerSWCGoal == Goal.Frame)
            {
                return "Frame " + GetTargetName();
            }
            return "No secondary win condition";
        }

        public string SWCResultsText() // text to put in to TMP object at end, when vicotory/defeat and success/failure is revealed
        {
            if (swcSuccess)
            {
                return ToString() + ": <size=40%>Success</size>";
            }
            else if (!swcSuccess)
            {
                return ToString() + ": <size=40%>Failure</size>";
            }
            // if swc goal is null (unassigned)
            return ToString();
        }

        public string SendableResultsText()
        {
            if (playerSWCGoal == Goal.None)
            {
                return GetPlayerName() + ": " + ToString() + "\n";
            }
            else if (swcSuccess)
            {
                return GetPlayerName() + ": " + ToString() + ": Success\n";
            }
            else
            {
                return GetPlayerName() + ": " + ToString() + ": Failure\n";
            }
        }

        // RPCs
        public bool RPCSendSWC()
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDSWC, (SendOption)1);
            messageWriter.Write(playerID);
            messageWriter.Write((byte)playerSWCGoal);
            messageWriter.Write(playerSWCTarget);
            // we assume that the target is alive at this point // can it be disconnected tho?
            messageWriter.EndMessage();
            return true;
        }

        public bool RPCDeathNote(DeathReason reason)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DEATHNOTE, (SendOption)1);
            messageWriter.Write(playerSWCTarget);
            messageWriter.Write((byte)reason);
            messageWriter.EndMessage();
            return true;
        }
    }
}
