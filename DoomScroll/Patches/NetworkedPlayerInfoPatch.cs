using HarmonyLib;
using Hazel;

namespace Doom_Scroll.Patches
{
    internal class NetworkedPlayerInfoPatch
    {
        // ideally we'd patch the SetTasks() method so we don't need to rpc but that is set to private now..
        [HarmonyPostfix]
        [HarmonyPatch("SetTasks")] 
        public static void PostfixRpcSetTasks() 
        { 
            // After settinjg the tasks locally and rpc the ids to the other players
            // we add two dummy tasks: Headline creation and Headline Sorting as task info only (no actual minigame) 
            DoomScrollTasksManager.AddHeadlineTaskInfos();
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ADDHEADLINETASKINFOS, (SendOption)1);
            messageWriter.EndMessage();
        }
    }
}
