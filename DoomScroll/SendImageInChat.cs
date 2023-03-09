using Hazel;
using UnityEngine;
using System;
using Doom_Scroll.Common;
using TMPro;

namespace Doom_Scroll
{
    public class SendImageInChat
    {
        public static bool RpcSendChatImage(byte[] image)
        {    
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SENDIMAGE, (SendOption)1);
            DoomScroll._log.LogInfo("image: " + image.Length + ", buffer: " + messageWriter.Buffer.Length + ", Pos "+ messageWriter.Position);
            int buffer = messageWriter.Buffer.Length - messageWriter.Position-3;
            
            if (Buffer.ByteLength(image) >= buffer)
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
            messageWriter.WriteBytesAndSize(image);
            messageWriter.EndMessage();
            return true;
        }       

        // in the 2023.2.28 release ChatBubble is an internal class,
        // so we have to access the Gamobject that has the ChatBubble scrip on it and set the image to be displayed
        internal static void SetImage(bool isLocalPlayer, GameObject chatBubble, byte[] imageBytes)
        {
            Sprite screenshot = ImageLoader.ReadImageFromByteArray(imageBytes);

            GameObject image = new GameObject("chat image");
            image.layer = LayerMask.NameToLayer("UI");
            image.transform.SetParent(chatBubble.transform);
            SpriteRenderer sr = image.AddComponent<SpriteRenderer>();
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.sprite = screenshot;
            sr.size = new Vector2(2f, sr.sprite.rect.height / sr.sprite.rect.width * 2f);
            sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            image.transform.localScale = Vector3.one;
            DoomScroll._log.LogInfo("Image size: " + sr.size);
            DoomScroll._log.LogInfo("chatbubble name: " + chatBubble.name);

            // get the child elements of ChatBubble we need to set the image correctly
            TextMeshPro chatText = chatBubble.transform.Find("ChatText").gameObject.GetComponent<TextMeshPro>();
            DoomScroll._log.LogInfo("chat text name: " + chatText.name);
            TextMeshPro nameText = chatBubble.transform.Find("NameText").gameObject.GetComponent<TextMeshPro>();
            DoomScroll._log.LogInfo("Player name: " + chatText.name);
            SpriteRenderer background = chatBubble.transform.Find("Background").gameObject.GetComponent<SpriteRenderer>();
            DoomScroll._log.LogInfo("background name: " + background.name);
            SpriteRenderer maskArea = chatBubble.transform.Find("MaskArea").gameObject.GetComponent<SpriteRenderer>();
            DoomScroll._log.LogInfo("maskArea name: " + maskArea.name);
            if (chatText != null)
            {
               
                chatText.text = "Fuck internal classes!";
                chatText.ForceMeshUpdate(true, true);
                Vector3 chatpos = chatText.transform.localPosition;
                float xOffset = isLocalPlayer ? -sr.size.x / 2 : sr.size.x / 2;
                image.transform.localPosition = new Vector3(chatpos.x + xOffset, chatpos.y - sr.size.y / 2 - 0.3f, chatpos.z);
            }
            if(nameText != null && background != null && maskArea != null)
            {
               
               

                background.size = new Vector2(5.52f, 0.3f + nameText.GetNotDumbRenderedHeight() + chatText.GetNotDumbRenderedHeight() + sr.size.y);
                maskArea.size = background.size - new Vector2(0f, 0.03f);
            }
           
        }
    }
}
