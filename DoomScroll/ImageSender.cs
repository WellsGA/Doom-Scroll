using Doom_Scroll.Common;
using Hazel;
using System.Collections;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Doom_Scroll.Patches;

namespace Doom_Scroll
{
    public class ImageSender
    {
        private static int TimeBetweenPieces = 500;
        public ImageSender() { }

        //this method should be called in FileScreenshot
        public void SendCurrentImageMethod(FileScreenshot imageFile, byte[] content)
        {
            SendCurrentImage(imageFile, content);
        }
        public async void SendCurrentImage(FileScreenshot imageFile, byte[] content)
        {
            //preparing arguments for RPCs
            int imgSectionLength = FileScreenshot.ImageSectionLength;
            byte pID = PlayerControl.LocalPlayer.PlayerId;
            Sprite img = ImageLoader.ReadImageFromByteArray(content);
            byte[] image = img.texture.EncodeToJPG(FileScreenshot.ImageSize);
            int numMessages = (int)Math.Ceiling(image.Length / imgSectionLength * 1f);

            DoomScroll._log.LogInfo("file size: " + image.Length);

            //sending RPCs
            /*string arrayString = "";
            foreach (byte b in image)
            {
                arrayString += " " + b.ToString();
            }
            DoomScroll._log.LogInfo("Byte array: " + arrayString);*/


            RpcSendImageFile(pID, imageFile.Id, image.Length, numMessages);
            foreach (int i in Enumerable.Range(0, numMessages))
            {
                if (i != numMessages - 1)
                {
                    RPCSendImageFilePiece(pID, imageFile.Id, image.Skip(imgSectionLength * i).Take(imgSectionLength).ToArray(), i);
                    DoomScroll._log.LogMessage($"Bytearray # {i} of image bytearray sections sent. Length is {image.Skip(imgSectionLength * i).Take(imgSectionLength).ToArray().Length}");
                    await Task.Delay(TimeBetweenPieces);
                }
                else
                {
                    RPCSendImageFilePiece(pID, imageFile.Id, image.Skip(imgSectionLength * i).ToArray(), i);
                    DoomScroll._log.LogMessage($"Bytearray # {i} of image bytearray sections sent. Length is {image.Skip(imgSectionLength * i).ToArray().Length}");
                    await Task.Delay(TimeBetweenPieces);
                }
            }

            // only after it's done sending out all the pieces, it makes the file shareable.
            imageFile.Btn.EnableButton(true);
        }
        public static bool RpcSendImageFile(byte playerID, int imageID, int imgLength, int numMessages)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDIMAGE, (SendOption)1);
            DoomScroll._log.LogInfo("image length: " + imgLength + ", buffer: " + messageWriter.Buffer.Length + ", Pos " + messageWriter.Position);
            int buffer = messageWriter.Buffer.Length - messageWriter.Position - 3;

            // CURRENT RESIZE OF THE IMAGE IS 15, BUT CAN BE CHANGED AS NEEDED
            int numBytes = imgLength;

            DoomScroll._log.LogInfo("New image size: " + numBytes + ", byte array length: " + imgLength + ", buffer: " + buffer);


            DoomScroll._log.LogMessage($"Image is {imgLength} bytes long, so there will be {numMessages} messages to send.");

            messageWriter.Write(numMessages);
            messageWriter.Write(playerID);
            messageWriter.Write(imageID);
            DoomScroll._log.LogMessage($"Sent image info: \n* {numMessages} messages to send\n* from player ID {playerID}\n* image ID number {imageID}");

            //messageWriter.Write("END OF MESSAGE");
            //DoomScroll._log.LogMessage("All bytearrays sent.");

            messageWriter.EndMessage();
            return true;
        }

        public static bool RPCSendImageFilePiece(byte playerID, int imageID, byte[] section, int sectionID)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDIMAGEPIECE, (SendOption)1);
            messageWriter.Write(playerID);
            messageWriter.Write(imageID);
            messageWriter.Write(sectionID);
            messageWriter.WriteBytesAndSize(section);
            messageWriter.EndMessage();
            return true;
        }

        public override String ToString()
        {
            return "Image sender yay!";
        }
    }
}
