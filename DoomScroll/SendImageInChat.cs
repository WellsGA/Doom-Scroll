using Hazel;
using UnityEngine;
using System;
using Doom_Scroll.Common;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace Doom_Scroll
{
    /*
    public class DoomScrollImage
    {
        private byte sender;
        private int imgNumber;
        private byte[] image;
        private List<byte[]> imageList;
        public byte Sender { get; }
        public byte ImgNumber { get; }
        public byte[] Image { get; }

        public DoomScrollImage(int numMessages, byte send, int num)
        {
            sender = send;
            imgNumber = num;
            imageList = new List<byte[]>(numMessages); //this apparently doesn't let you assign stuff to random positions though
            //for (int i = 0; i < imgNumber; i++) imageList.Add(null);
        }

        public bool SameImage(byte send, byte num)
        { 
            if (sender == send && imgNumber == num)
            {
                return true;
            }
            return false;
        }
        public void InsertByteChunk(int sectionIndex, byte[] byteChunk)
        {
            imageList.Insert((int)sectionIndex, byteChunk);
        }

        public bool CompileImage()
        {
            if (imageList.IndexOf(null) == -1)
            {
                List<byte> imgBytes = new List<byte>();
                foreach (byte[] byteArray in imageList)
                {
                    imgBytes.AddRange(byteArray);
                }
                image = imgBytes.ToArray();
                return true;
            }
            return false;
        }

        public List<int> GetMissingLines()
        {
            List<int> missingLines = new List<int>();
            IEnumerable<Int32> indexes = Enumerable.Range(0, imageList.Count);
            foreach (int i in indexes)
            {
                if (imageList[i] == null)
                {
                    missingLines.Add(i);
                }
            }
            return missingLines;
        }
    }
    */

    public class SendImageInChat : MonoBehaviour
    {
        //REVERTED SENDIMAGE
       

        //NEWER SENDIMAGE
        /*
        public static bool RpcSendChatImage(byte playerID, int imageID, byte[] image, int numMessages)
        {    
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDIMAGE, (SendOption)1);
            DoomScroll._log.LogInfo("image: " + image.Length + ", buffer: " + messageWriter.Buffer.Length + ", Pos "+ messageWriter.Position);
            int buffer = messageWriter.Buffer.Length - messageWriter.Position-3;
            
             // CURRENT RESIZE OF THE IMAGE IS 15, BUT CAN BE CHANGED AS NEEDED
            int numBytes = image.Length;

            DoomScroll._log.LogInfo("New image size: " + numBytes + ", byte array length: " + image.Length + ", buffer: " + buffer);

            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
            {
                ChatControllerPatch.screenshot = image;
                string chatMessage = PlayerControl.LocalPlayer.PlayerId + "#" + ScreenshotManager.Instance.Screenshots;
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, chatMessage);
            }

             // CURRENT SIZE OF AN ARRAY WE ARE SENDING IS 1000.0, BUT CAN BE CHANGED AS NEEDED
            DoomScroll._log.LogMessage($"Image is {image.Length} bytes long, so there will be {numMessages} messages to send.");

            messageWriter.Write(numMessages);
            messageWriter.Write(playerID);
            messageWriter.Write(imageID);
            DoomScroll._log.LogMessage($"Sent image info: \n* {numMessages} messages to send\n* from player ID {playerID}\n* image ID number {imageID}");
            
            //messageWriter.Write("END OF MESSAGE");
            //DoomScroll._log.LogMessage("All bytearrays sent.");
            
            messageWriter.EndMessage();
            return true;
        }
        public static bool RPCSendChatImagePiece(byte playerID, int imageID, byte[] section, int numMessages, int sectionID)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDIMAGEPIECE, (SendOption)1);
            messageWriter.Write(playerID);
            messageWriter.Write(imageID);
            messageWriter.Write(sectionID);
            messageWriter.WriteBytesAndSize(section);
            messageWriter.EndMessage();
            return true;
        }
        */
    }
    /*//UNUSED, FOR NOW
    public class Line
    {
        private int lineCounter;
        private byte line;
        public Line(int lCounter, byte l)
        {
            lineCounter = lCounter;
            line = l;
        }
    }*/
    /*
    public class Photo
    {
        private byte[] image;
        public Photo(byte[] img)
        {
            image = img;
        }
    }
    */
}
