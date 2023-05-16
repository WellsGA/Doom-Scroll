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
    public class DoomScrollImage
    {
        private byte sender;
        private byte imgNumber;
        private byte[] image;
        private List<byte[]> imageList;
        public byte Sender { get; }
        public byte ImgNumber { get; }
        public byte[] Image { get; }

        public DoomScrollImage(byte numMessages, byte send, byte num)
        {
            sender = send;
            imgNumber = num;
            imageList = new List<byte[]>(numMessages); //this apparently doesn't let you assign stuff to random positions though
            for (int i = 0; i < imgNumber; i++) imageList.Add(null);
        }

        public bool SameImage(byte send, byte num)
        { 
            if (sender == send && imgNumber == num)
            {
                return true;
            }
            return false;
        }
        public void InsertByteChunk(byte[] byteChunk)
        {
            if (byteChunk.Length > 1)
            {
                imageList.Insert((int)byteChunk[0], byteChunk[1..]);
                // If above slice doesn't work, try this: (byte[])byteChunk.Skip(1)
            }
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

    public class SendImageInChat : MonoBehaviour
    {
        private static int ImageSize = 15;
        private static int ImageSectionLength = 1000;
        public static bool RpcSendChatImage(byte[] image)
        {    
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDIMAGE, (SendOption)1);
            DoomScroll._log.LogInfo("image: " + image.Length + ", buffer: " + messageWriter.Buffer.Length + ", Pos "+ messageWriter.Position);
            int buffer = messageWriter.Buffer.Length - messageWriter.Position-3;
            
            Sprite img = ImageLoader.ReadImageFromByteArray(image);
            image = img.texture.EncodeToJPG(15); // CURRENT RESIZE OF THE IMAGE IS 15, BUT CAN BE CHANGED AS NEEDED
            int numBytes = Buffer.ByteLength(image);
            byte playerID = PlayerControl.LocalPlayer.PlayerId;
            byte imageID = 0; // QUESTION FOR AGNES: HOW AND WHERE CAN I GET THE IMAGE ID?

            DoomScroll._log.LogInfo("New image size: " + numBytes + ", byte array length: " + image.Length + ", buffer: " + buffer);

            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
            {
                ChatControllerPatch.screenshot = image;
                string chatMessage = PlayerControl.LocalPlayer.PlayerId + "#" + ScreenshotManager.Instance.Screenshots;
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, chatMessage);
            }

            int numMessages = (int)Math.Ceiling(numBytes / ImageSectionLength*1f); // CURRENT SIZE OF AN ARRAY WE ARE SENDING IS 1000.0, BUT CAN BE CHANGED AS NEEDED
            DoomScroll._log.LogMessage($"Image is {image.Length} bytes long, so there will be {numMessages} messages to send.");

            messageWriter.Write(numMessages);
            messageWriter.Write(playerID);
            messageWriter.Write(imageID);
            DoomScroll._log.LogMessage($"Sent image info: \n* {numMessages} messages to send\n* from player ID {playerID}\n* image ID number {imageID}");
            foreach (int i in Enumerable.Range(0, numMessages))
            {
                List<byte> currentImageSection;
                if (i != numMessages - 1)
                {
                    currentImageSection = image.Skip(1000 * i).Take(1000).ToList();
                    currentImageSection.Insert(0, imageID);
                    currentImageSection.Insert(0, playerID);
                }
                else
                {
                    currentImageSection = image.Skip(1000 * i).ToList();
                    currentImageSection.Insert(0, imageID);
                    currentImageSection.Insert(0, playerID);
                }
                messageWriter.WriteBytesAndSize(currentImageSection.ToArray());
                DoomScroll._log.LogMessage($"Bytearray # {i} of image bytearray sections sent. Length is {currentImageSection.Count}");
            }
            messageWriter.Write("END OF MESSAGE");
            DoomScroll._log.LogMessage("All bytearrays sent.");
            return true;
        }
    }
    //UNUSED, FOR NOW
    public class Line
    {
        private int lineCounter;
        private byte line;
        public Line(int lCounter, byte l)
        {
            lineCounter = lCounter;
            line = l;
        }
    }
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
