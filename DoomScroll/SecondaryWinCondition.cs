
namespace Doom_Scroll
{
    public enum Goal : byte
    {
        Protect,
        Frame
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
            swcSuccess = goal != Goal.Frame  ? true : false; // if protect, they start on success
        }

        public string GetTargetName() // same as GetPlayerName, would worth to use only GetPlayerName with an id parameter
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
        public bool CheckSuccess()
        {
            return swcSuccess;
        }

        public string GetPlayerName()
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

        public byte GetTargetId()
        {
            return playerSWCTarget;
        }

        public Goal GetGoal()
        {
            return playerSWCGoal;
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
            else // if (playerSWCGoal == Goal.Frame)
            {
                if (targetState != TargetState.ALIVE) // reason of death doesn't matter for now
                {
                    DoomScroll._log.LogInfo("Frame successful.");
                    swcSuccess = true;
                }
            }
        }
        public void TargetDead(DeathReason reason)
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

        public override string ToString()
        // returns the base SWC assignment text, the text to put into TMP object at beginning
        // when roles are assigned
        {
            if (playerSWCGoal == Goal.Protect)
            {
                return "Protect " + GetTargetName();
            }
            else // if frame
            {
                return "Frame " + GetTargetName();
            }
        }

        public string SWCResultsText() // text to put in to TMP object at end, when vicotory/defeat and success/failure is revealed
        {
            if (swcSuccess)
            {
                return ToString() + " <size=40%>Success</size>";
            }
            else // if !swcSuccess
            {
                return ToString() + " <size=40%>Failure</size>";
            }
        }

        public string SendableResultsText()
        {
            if (swcSuccess)
            {
                return "\n" + GetPlayerName() + ": " + ToString() + " Success";
            }
            else
            {
                return "\n" + GetPlayerName() + ": " + ToString() + " Failure";
            }
        }
    }
}
