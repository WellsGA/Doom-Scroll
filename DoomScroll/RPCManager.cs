using Hazel;
using UnityEngine;
using System;
using Doom_Scroll.Common;
using HarmonyLib;
using UnityEngine.Rendering;

namespace Doom_Scroll
{
    /*public enum CustomRPC : byte
    {
        SENDIMAGE = 255
    }*/
    public class RPCManager
    {
        public static bool RpcSendChatImage(byte[] image)
        {
            
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, 255, (SendOption)1);

            DoomScroll._log.LogInfo("image: " + image.Length + ", buffer: " + messageWriter.Buffer.Length + ", Pos "+ messageWriter.Position);
            int buffer = messageWriter.Buffer.Length - messageWriter.Position;
            
            if (image.Length >= buffer)
            {
                Sprite img = ImageLoader.ReadImageFromByteArray(image);
                int n = 75; // default quality for the jpg
                //reduce quality until it fits the buffer
                do
                {
                    n--;
                    image = img.texture.EncodeToJPG(n);

                }
                while (image.Length <= 50000);
                DoomScroll._log.LogInfo("New image size: " + image.Length + ", buffer: " + buffer);
                messageWriter.WriteBytesAndSize(image);
            }
            else
            {
                DoomScroll._log.LogInfo("Buffer was large enough");
                messageWriter.WriteBytesAndSize(image);
            }
            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
            {
                AddChat(PlayerControl.LocalPlayer, image);
            }
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
                SetImage(chatBubble, imageBytes);

                chatBubble.AlignChildren();
                chatContorller.AlignAllBubbles();
                Vector3 chatpos = chatBubble.TextArea.transform.position;
                //image.transform.localPosition = new Vector3(chatpos.x, chatpos.y, 0);
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

        internal static void SetImage(ChatBubble chatBubble, byte[] imageBytes)
        {
            Sprite screenshot = ImageLoader.ReadImageFromByteArray(imageBytes);
            GameObject image = new GameObject("chat image");
            image.layer = LayerMask.NameToLayer("UI");
            image.transform.SetParent(chatBubble.transform);
            SpriteRenderer sr = image.AddComponent<SpriteRenderer>();
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.sprite = screenshot;
            sr.size = new Vector2(2f, sr.sprite.rect.height * 2f / sr.sprite.rect.width);
            Vector3 chatpos = chatBubble.TextArea.transform.localPosition;
            image.transform.localPosition = new Vector3(chatpos.x - sr.size.x / 2, chatpos.y - sr.size.y / 2 - 0.3f, chatpos.z);
            chatBubble.TextArea.text = "et voila";
            chatBubble.TextArea.ForceMeshUpdate(true, true);
            chatBubble.Background.size = new Vector2(5.52f, 0.3f + chatBubble.NameText.GetNotDumbRenderedHeight() + chatBubble.TextArea.GetNotDumbRenderedHeight() + sr.size.y);
            chatBubble.MaskArea.size = chatBubble.Background.size - new Vector2(0f, 0.03f);
        }
    }

        [HarmonyPatch(typeof(PlayerControl))]
        public static class PlayerControlPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("HandleRpc")]
            public static void PrefixHandleRpc(byte callId, MessageReader reader)
            {
                switch (callId)
                {
                    case 255:
                    DoomScroll._log.LogInfo("reader buffer: " + reader.Buffer);
                    byte[] imageBytes = reader.ReadBytesAndSize();
                    DoomScroll._log.LogInfo("Image received! Size:" + imageBytes.Length);
                    if (DestroyableSingleton<HudManager>.Instance)
                    {
                        RPCManager.AddChat(PlayerControl.LocalPlayer, imageBytes);
                        return;
                    }
                    break;
            }
            }
        }
    }
