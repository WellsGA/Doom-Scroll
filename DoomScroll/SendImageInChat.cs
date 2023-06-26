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
        public static bool RpcSendChatImage(byte[] image)
        {
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, byte.MaxValue, (SendOption)1);
            DoomScroll._log.LogInfo(string.Concat(new string[]
            {
                "image: ",
                image.Length.ToString(),
                ", buffer: ",
                messageWriter.Buffer.Length.ToString(),
                ", Pos ",
                messageWriter.Position.ToString()
            }));
            int num = messageWriter.Buffer.Length - messageWriter.Position - 3;
            bool flag = Buffer.ByteLength(image) >= num;
            if (flag)
            {
                Sprite sprite = ImageLoader.ReadImageFromByteArray(image);
                int num2 = 75;
                do
                {
                    num2--;
                    image = ImageConversion.EncodeToJPG(sprite.texture, num2);
                }
                while (Buffer.ByteLength(image) >= num);
                DoomScroll._log.LogInfo(string.Concat(new string[]
                {
                    "New image size: ",
                    Buffer.ByteLength(image).ToString(),
                    ", byte array length: ",
                    image.Length.ToString(),
                    ", buffer: ",
                    num.ToString()
                }));
            }
            bool flag2 = AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance;
            if (flag2)
            {
                ChatControllerPatch.screenshot = image;
                string chatText = PlayerControl.LocalPlayer.PlayerId.ToString() + "#" + ScreenshotManager.Instance.Screenshots.ToString();
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, chatText);
            }
            messageWriter.WriteBytesAndSize(image);
            messageWriter.EndMessage();
            return true;
        }

        internal static void SetImage(bool isLocalPlayer, GameObject chatBubble, byte[] imageBytes)
        {
            Sprite sprite = ImageLoader.ReadImageFromByteArray(imageBytes);
            GameObject gameObject = new GameObject("chat image");
            gameObject.layer = LayerMask.NameToLayer("UI");
            gameObject.transform.SetParent(chatBubble.transform);
            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.drawMode = (SpriteDrawMode)1;
            spriteRenderer.sprite = sprite;
            spriteRenderer.size = new Vector2(2f, spriteRenderer.sprite.rect.height / spriteRenderer.sprite.rect.width * 2f);
            spriteRenderer.maskInteraction = (SpriteMaskInteraction)1;
            gameObject.transform.localScale = Vector3.one;
            DoomScroll._log.LogInfo("Image size: " + spriteRenderer.size.ToString());
            DoomScroll._log.LogInfo("chatbubble name: " + chatBubble.name);
            TextMeshPro component = chatBubble.transform.Find("ChatText").gameObject.GetComponent<TextMeshPro>();
            DoomScroll._log.LogInfo("chat text name: " + component.name);
            TextMeshPro component2 = chatBubble.transform.Find("NameText").gameObject.GetComponent<TextMeshPro>();
            DoomScroll._log.LogInfo("Player name: " + component.name);
            SpriteRenderer component3 = chatBubble.transform.Find("Background").gameObject.GetComponent<SpriteRenderer>();
            DoomScroll._log.LogInfo("background name: " + component3.name);
            SpriteRenderer component4 = chatBubble.transform.Find("MaskArea").gameObject.GetComponent<SpriteRenderer>();
            DoomScroll._log.LogInfo("maskArea name: " + component4.name);
            bool flag = component != null;
            if (flag)
            {
                component.text = "Fuck internal classes!";
                component.ForceMeshUpdate(true, true);
                Vector3 localPosition = component.transform.localPosition;
                float num = isLocalPlayer ? (-spriteRenderer.size.x / 2f) : (spriteRenderer.size.x / 2f);
                gameObject.transform.localPosition = new Vector3(localPosition.x + num, localPosition.y - spriteRenderer.size.y / 2f - 0.3f, localPosition.z);
            }
            bool flag2 = component2 != null && component3 != null && component4 != null;
            if (flag2)
            {
                component3.size = new Vector2(5.52f, 0.3f + component2.GetNotDumbRenderedHeight() + component.GetNotDumbRenderedHeight() + spriteRenderer.size.y);
                component4.size = component3.size - new Vector2(0f, 0.03f);
            }
        }

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
