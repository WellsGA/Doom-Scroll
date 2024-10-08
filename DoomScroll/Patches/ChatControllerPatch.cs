﻿using Doom_Scroll.Common;
using Doom_Scroll.UI;
using HarmonyLib;
using TMPro;
using UnityEngine;
using System.Collections.Generic;


namespace Doom_Scroll.Patches
{
    public enum ChatContent
    {
        SCREENSHOT,
        DEFAULT
    }

    // in the 2023.2.28 release ChatBubble is an internal class,
    // so we have to access the Gamobject that has the ChatBubble script on it and set the image to be displayed
    [HarmonyPatch(typeof(ChatController))]
    public class ChatControllerPatch
    {
        public static byte[] screenshot = null; // for sharing images
        public static ChatContent content = ChatContent.DEFAULT;
        private static int numberOfMessages = 10;

        public static string GetChatID()
        {
            string playerId = PlayerControl.LocalPlayer.PlayerId < 10 ? "0" + PlayerControl.LocalPlayer.PlayerId.ToString() : PlayerControl.LocalPlayer.PlayerId.ToString();
            string id = playerId + numberOfMessages.ToString();
            numberOfMessages++;
            return id;
        }

        [HarmonyPostfix]
        [HarmonyPatch("AddChat")]
        public static void PostfixAddChat(ChatController __instance, PlayerControl sourcePlayer, string chatText)
        {   
            // logging chat messages
            if (AmongUsClient.Instance.AmHost)
            {
                string message = chatText;
                if(content == ChatContent.SCREENSHOT) { message = "Photo shared, ID: " + chatText; }
                GameLogger.Write(GameLogger.GetTime() + " - " + sourcePlayer.name + " texted: " + message);
            }

            switch (content)
            {
                case ChatContent.SCREENSHOT:
                    {
                        bool isLocalPlayer = sourcePlayer == PlayerControl.LocalPlayer;
                        GameObject scroller = __instance.GetComponentInChildren<Scroller>(true).gameObject;
                        // get the last max 25 chatbubbles // will only work as long as they keep the object hierarchy
                        List<GameObject> lastBubbles = new List<GameObject>();
                        int nrofBubbles = scroller.transform.childCount;
                        int maxbubbles = nrofBubbles >= 25 ? 25 : nrofBubbles;
                        for (int i = 1; i <= maxbubbles; i++)
                        {
                            lastBubbles.Add(scroller.transform.GetChild(nrofBubbles - i).gameObject);
                        }

                        foreach (GameObject bub in lastBubbles)
                        {
                            TextMeshPro[] texts = bub.GetComponentsInChildren<TextMeshPro>();
                            foreach (TextMeshPro text in texts)
                            { 
                                if (text.text == chatText)
                                {
                                    DoomScroll._log.LogInfo("ChatBubble was found, id: " + chatText);
                                    text.text = chatText;
                                    Transform chatbubble = text.transform.parent;
                                    // child elements of ChatBubble needed to set the content correctly
                                    SpriteRenderer background = chatbubble.transform.Find("Background").gameObject.GetComponent<SpriteRenderer>();
                                    TextMeshPro nameText = chatbubble.Find("NameText (TMP)").gameObject.GetComponent<TextMeshPro>();
                                    SpriteRenderer maskArea = chatbubble.Find("MaskArea").gameObject.GetComponent<SpriteRenderer>();

                                    if (chatbubble != null && screenshot != null)
                                    {
                                        Sprite imgSprite = ImageLoader.ReadImageFromByteArray(screenshot);
                                        GameObject image = new GameObject("chat image");
                                        image.layer = LayerMask.NameToLayer("UI");
                                        image.transform.SetParent(chatbubble);
                                        SpriteRenderer sr = image.AddComponent<SpriteRenderer>();
                                        sr.drawMode = SpriteDrawMode.Sliced;
                                        sr.sprite = imgSprite;
                                        sr.size = new Vector2(2f, sr.sprite.rect.height / sr.sprite.rect.width * 2f);
                                        sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                                        image.transform.localScale = Vector3.one;
                                        DoomScroll._log.LogInfo("Image size: " + sr.size);
                                        DoomScroll._log.LogInfo("chatbubble name: " + chatbubble.name);

                                        // text.text = "";
                                        text.ForceMeshUpdate(true, true);
                                        Vector3 chatpos = text.transform.localPosition;
                                        float xOffset = isLocalPlayer ? -sr.size.x / 2 : sr.size.x / 2;
                                        image.transform.localPosition = new Vector3(chatpos.x + xOffset, chatpos.y - sr.size.y / 2 - 0.3f, chatpos.z);
                                        if (nameText != null && background != null && maskArea != null)
                                        {
                                            text.ForceMeshUpdate(true, true);
                                            background.size = new Vector2(5.52f, 0.3f + nameText.GetNotDumbRenderedHeight() + text.GetNotDumbRenderedHeight() + sr.size.y);
                                            maskArea.size = background.size - new Vector2(0f, 0.03f);
                                            // background.transform.localPosition = new Vector3(background.transform.localPosition.x, background.transform.localPosition.y - background.size.y / 3, background.transform.localPosition.z);
                                            background.transform.localPosition += new Vector3(0, nameText.transform.localPosition.y - background.size.y / 2f + 0.05f, 0);
                                            maskArea.transform.localPosition = background.transform.localPosition + new Vector3(0f, 0.02f, 0f);
                                        }
                                        screenshot = null;
                                    }
                                    content = ChatContent.DEFAULT;  // set back to default
                                    return;
                                }
                            }
                        }
                        DoomScroll._log.LogInfo("Couldn't find the ChatBubble, id: " + chatText);
                        break;
                    }
                case ChatContent.DEFAULT:
                default:
                    return;
            }
        }

        private static void AddEndorseButtonsToChatbubble(string ID, GameObject chatbubble, Vector2 size, bool isLocalPlayer)
        {
            HeadlineEndorsement endorsement = new HeadlineEndorsement(chatbubble, size, ID);
            float xPosEndorse = isLocalPlayer ? size.x * 1.32f : size.x / 2f;
            endorsement.LikeButtons.ArrangeButtons(0.3f, 2, xPosEndorse, -size.y / 2 + 0.6f);
            HeadlineDisplay.Instance.endorsementList.Add(endorsement);
        }
    }
}
