﻿using Hazel;
using UnityEngine;
using System;
using Doom_Scroll.Common;
using System.Reflection;

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
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, image.ToString());
                AddChat(PlayerControl.LocalPlayer, image);
            }
            messageWriter.WriteBytesAndSize(image);
            messageWriter.EndMessage();
            return true;
        }

        // based on the AddChat method of ChatController class
        public static void AddChat(PlayerControl sourcePlayer, byte[] imageBytes)
        {
            ChatController chatContorller = DestroyableSingleton<HudManager>.Instance.Chat;
            if (!sourcePlayer || !PlayerControl.LocalPlayer)
            {
                return;
            }
            GameData.PlayerInfo localPlayerData = PlayerControl.LocalPlayer.Data;
            GameData.PlayerInfo sourecPlayerData = sourcePlayer.Data;
            if (sourecPlayerData == null || localPlayerData == null 
                || (sourecPlayerData.IsDead && !localPlayerData.IsDead))
            {
                return;
            }
            if (chatContorller.chatBubPool.NotInUse == 0)
            {
                chatContorller.chatBubPool.ReclaimOldest();
            }
           
            ChatBubble chatBubble = chatContorller.chatBubPool.Get<ChatBubble>();
            try
            {
                chatBubble.transform.SetParent(chatContorller.scroller.Inner);
                chatBubble.transform.localScale = Vector3.one;
                bool flag = sourcePlayer == PlayerControl.LocalPlayer;
                if (flag)
                {
                    chatBubble.SetRight();
                }
                else
                {
                    chatBubble.SetLeft();
                }
                bool didVote = MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);
                chatBubble.SetCosmetics(sourecPlayerData);
                chatContorller.SetChatBubbleName(chatBubble, sourecPlayerData, sourecPlayerData.IsDead, didVote, PlayerNameColor.Get(sourecPlayerData), null);
                // removed chat filter - we are not sending a free text
                SetImage(flag, chatBubble, imageBytes);
                chatBubble.AlignChildren();
                chatContorller.AlignAllBubbles();
                Vector3 chatpos = chatBubble.TextArea.transform.position;
                if (!chatContorller.IsOpen && chatContorller.notificationRoutine == null)
                {
                    chatContorller.notificationRoutine = chatContorller.StartCoroutine(chatContorller.BounceDot());
                }
                if (!flag)
                {
                    SoundManager.Instance.PlaySound(chatContorller.MessageSound, false, 1f, null).pitch = 0.5f + (float)sourcePlayer.PlayerId / 15f;
                }
            }
            catch (Exception message)
            {
                DoomScroll._log.LogError(message);
                chatContorller.chatBubPool.Reclaim(chatBubble);
            }
        }

        internal static void SetImage(bool isLocalPlayer, ChatBubble chatBubble, byte[] imageBytes)
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

            chatBubble.TextArea.text = "et voila ...";
            chatBubble.TextArea.ForceMeshUpdate(true, true);
            Vector3 chatpos = chatBubble.TextArea.transform.localPosition;
            float xOffset = isLocalPlayer ? -sr.size.x / 2 : sr.size.x / 2;
            image.transform.localPosition = new Vector3(chatpos.x + xOffset, chatpos.y - sr.size.y / 2 - 0.3f, chatpos.z);

            chatBubble.Background.size = new Vector2(5.52f, 0.3f + chatBubble.NameText.GetNotDumbRenderedHeight() + chatBubble.TextArea.GetNotDumbRenderedHeight() + sr.size.y);
            chatBubble.MaskArea.size = chatBubble.Background.size - new Vector2(0f, 0.03f);

        }
    }
}
