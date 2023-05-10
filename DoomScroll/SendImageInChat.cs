using Hazel;
using UnityEngine;
using System;
using Doom_Scroll.Common;
using TMPro;

namespace Doom_Scroll
{
    public class SendImageInChat : MonoBehaviour
    {
        public static bool RpcSendChatImage(byte[] image)
        {    
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDIMAGE, (SendOption)1);
            DoomScroll._log.LogInfo("image: " + image.Length + ", buffer: " + messageWriter.Buffer.Length + ", Pos "+ messageWriter.Position);
            int buffer = messageWriter.Buffer.Length - messageWriter.Position-3;
            
            if (Buffer.ByteLength(image) >= buffer/2)
            {
                Sprite img = ImageLoader.ReadImageFromByteArray(image);
                int n = 75; // default quality for the jpg
                //reduce quality until it fits the buffer
                do
                {
                    n--;
                    image = img.texture.EncodeToJPG(n);
                }
                while (Buffer.ByteLength(image) >= buffer);
                DoomScroll._log.LogInfo("New image size: " + Buffer.ByteLength(image) + ", byte array length: " + image.Length + ", buffer: " + buffer);   
            }
            
            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
            {
                ChatControllerPatch.screenshot = image;
                string chatMessage = PlayerControl.LocalPlayer.PlayerId + "#" + ScreenshotManager.Instance.Screenshots;
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, chatMessage);
            }
            // messageWriter.WriteBytesAndSize(image);

            // NEW SENDING METHOD: WRITE EACH BYTE INDIVIDUALLY, WITH A STRING NUMBER ON FRONT? THEN SEND COMPILED STRING
            // OVERALL STRING MAY BE TOO BIG; COULD TRY USING .Write() ON EACH LINE SEPARATELY?
            //          messageWriter.Write($"{line_counter}{imgLine}");
            int line_counter = 0;
            string lines = "";
            foreach (byte imgLine in image)
            {
                lines = lines + $"{line_counter}{imgLine} ";
                line_counter = line_counter + 1;
            }
            messageWriter.Write(lines);
            //NEW SECTION ENDS HERE
            messageWriter.EndMessage();
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
