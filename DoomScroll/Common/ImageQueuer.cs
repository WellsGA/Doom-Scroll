﻿using Doom_Scroll.Patches;
using Hazel;
using System.Collections.Generic;

namespace Doom_Scroll.Common
{
    // the class will exist for all players but only the host will use it
    static internal class ImageQueuer
    {
        public static bool isSharing = false;
        private static Queue<KeyValuePair<byte, int>> imageSharingQueue = new Queue<KeyValuePair<byte, int>>();

        public static void AddToQueue(byte playerId, int itemId)
        {
            imageSharingQueue.Enqueue(new KeyValuePair<byte, int>(playerId, itemId));
            DoomScroll._log.LogInfo("image queued, id: " + itemId + ", currently sharing: " + isSharing);
        }

        public static bool HasItems()
        {
            if (imageSharingQueue == null)
            {
                return false;
            }
            else
            {
                return imageSharingQueue.Count > 0;
            }
        }

        public static void NextCanShare()
        {
            KeyValuePair<byte, int> nextInLine = imageSharingQueue.Peek();
            if (nextInLine.Key == PlayerControl.LocalPlayer.PlayerId)
            {
                DoomScroll._log.LogInfo("2) You can send image (LP): " + nextInLine.Value);
                ScreenshotManager.Instance.SendImageInPieces(nextInLine.Value);
            }
            else
            {
                RPCNextCanShare(nextInLine);
            }
            isSharing = true;
        }
        public static void RPCNextCanShare(KeyValuePair<byte, int> next)
        {
            
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CANSENDIMAGE, (SendOption)1);
            messageWriter.Write(next.Key);    // id of who can share
            messageWriter.Write(next.Value);  // id of what can they share
            messageWriter.EndMessage();
        }

        public static void FinishedSharing(byte id, int itemID)
        {
            if(imageSharingQueue.Peek().Key == id && imageSharingQueue.Peek().Value == itemID)
            {
                imageSharingQueue.Dequeue();
            }
            isSharing = false;
        }

    }
}
