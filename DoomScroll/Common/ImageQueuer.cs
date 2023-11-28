using Doom_Scroll.Patches;
using Hazel;
using System.Collections.Generic;


namespace Doom_Scroll.Common
{
    // the class will exist for all players but only the host will use it
    static internal class ImageQueuer
    {
        public static bool isSharing = false;
        private static Queue<KeyValuePair<byte, string>> imageSharingQueue = new Queue<KeyValuePair<byte, string>>();

        public static void AddToQueue(byte playerId, string itemId)
        {
            imageSharingQueue.Enqueue(new KeyValuePair<byte, string>(playerId, itemId));
        }

        public static bool HasItems()
        {
            return imageSharingQueue.Count > 0;
        }

        public static void RPCNextCanShare()
        {
            isSharing = true;
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CANSENDIMAGE, (SendOption)1);
            messageWriter.Write(imageSharingQueue.Peek().Key);    // id of who can share
            messageWriter.Write(imageSharingQueue.Peek().Value);  // id of what can they share
            messageWriter.EndMessage();
        }

        public static void FinishedSharing(byte id, string itemID)
        {
            if(imageSharingQueue.Peek().Key == id && imageSharingQueue.Peek().Value == itemID)
            {
                imageSharingQueue.Dequeue();
            }
            isSharing = false;
        }

    }
}
