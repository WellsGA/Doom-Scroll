using Doom_Scroll.Common;
using Doom_Scroll.UI;
using HarmonyLib;
using TMPro;
using UnityEngine;


namespace Doom_Scroll.Patches
{
    public enum ChatContent
    {
        TEXT,
        HEADLINE,
        SCREENSHOT,
        DEFAULT
    }

    // in the 2023.2.28 release ChatBubble is an internal class,
    // so we have to access the Gamobject that has the ChatBubble script on it and set the image to be displayed
    [HarmonyPatch(typeof(ChatController))]
    public class ChatControllerPatch
    {
        public static byte[] screenshot = null; // for sharing images
        public static ChatContent content = ChatContent.TEXT;
        private static int numberOfMessages = 10;

        public static string GetChatID()
        {
            string playerId = PlayerControl.LocalPlayer.PlayerId < 10 ? "0" + PlayerControl.LocalPlayer.PlayerId.ToString() : PlayerControl.LocalPlayer.PlayerId.ToString();
            string id = playerId + numberOfMessages.ToString();
            numberOfMessages++;
            return id;
        }

        [HarmonyPrefix]
        [HarmonyPatch("AddChat")]
        public static void PrefixAddChat(out string __state, ref string chatText)
        {

            __state = chatText.Substring(4);        // save content into state
            chatText = chatText.Substring(0, 4);    // id - so we can find the chatbubble after created and added to the scroller
        }


        [HarmonyPostfix]
        [HarmonyPatch("AddChat")]
        public static void PostfixAddChat(ChatController __instance, PlayerControl sourcePlayer, string chatText, string __state)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                GameLogger.Write(GameLogger.GetTime() + " - " + sourcePlayer.name + " texted: " + __state);
            }

            bool isLocalPlayer = sourcePlayer == PlayerControl.LocalPlayer;
            GameObject scroller = __instance.GetComponentInChildren<Scroller>(true).gameObject;
            TextMeshPro[] texts = scroller.gameObject.GetComponentsInChildren<TextMeshPro>(true);
            if (texts != null)
            {
                foreach (TextMeshPro text in texts)
                {
                    if (text.text == chatText)
                    {
                        string ID = text.text;
                        DoomScroll._log.LogInfo("ChatBubble was found, id: + " + ID + ", text: " + chatText);
                        text.text = __state;
                        Transform chatbubble = text.transform.parent;
                        // child elements of ChatBubble needed to set the content correctly
                        SpriteRenderer background = chatbubble.transform.Find("Background").gameObject.GetComponent<SpriteRenderer>();
                        TextMeshPro nameText = chatbubble.Find("NameText (TMP)").gameObject.GetComponent<TextMeshPro>();
                        SpriteRenderer maskArea = chatbubble.Find("MaskArea").gameObject.GetComponent<SpriteRenderer>();

                        // if (nameText != null) nameText.text = nameText.text + " " + ID; // debug
                        /*RectMask2D mask2D = text.transform.parent.GetComponentInChildren<RectMask2D>();
                        if (mask2D != null) DoomScroll._log.LogInfo("2d mask: " + mask2D.gameObject.name);*/

                        switch (content)
                        {
                            case ChatContent.TEXT:
                                if(chatbubble != null && nameText != null && background != null && maskArea != null)
                                {
                                    background.color = new Color(0.94f, 0.93f, 0.99f, 1);
                                    text.ForceMeshUpdate(true, true);
                                    background.size = new Vector2(background.size.x, background.size.y + 0.3f);
                                    maskArea.size = background.size - new Vector2(0f, 0.03f);
                                    background.transform.localPosition += new Vector3(0, nameText.transform.localPosition.y - background.size.y / 2f + 0.05f, 0);
                                    maskArea.transform.localPosition = background.transform.localPosition + new Vector3(0f, 0.02f, 0f);
                                    AddEndorseButtonsToChatbubble(ID, chatbubble.gameObject, background.size, isLocalPlayer);

                                }
                                break;
                            case ChatContent.HEADLINE:
                                if (chatbubble != null && nameText != null && background != null && maskArea != null)
                                {
                                    background.color = new Color(0.97f, 0.96f, 0.90f, 1);
                                    text.ForceMeshUpdate(true, true);
                                    background.size = new Vector2(5.52f, 0.3f + nameText.GetNotDumbRenderedHeight() + text.GetNotDumbRenderedHeight());
                                    maskArea.size = background.size - new Vector2(0f, 0.03f);
                                    background.transform.localPosition += new Vector3(0, nameText.transform.localPosition.y - background.size.y / 2f + 0.05f, 0);
                                    maskArea.transform.localPosition = background.transform.localPosition + new Vector3(0f, 0.02f, 0f);
                                    AddEndorseButtonsToChatbubble(ID, chatbubble.gameObject, background.size, isLocalPlayer);
                                }
                                break;
                            case ChatContent.SCREENSHOT:
                                if (screenshot == null) return;
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
                                        AddEndorseButtonsToChatbubble(ID, chatbubble.gameObject, background.size, isLocalPlayer);
                                    }
                                    screenshot = null;
                                }
                                break;
                            case ChatContent.DEFAULT:
                            default:
                                if (chatbubble != null && nameText != null && background != null && maskArea != null)
                                {
                                    text.ForceMeshUpdate(true, true);
                                    background.size = new Vector2(background.size.x, background.size.y + 0.3f);
                                    maskArea.size = background.size - new Vector2(0f, 0.03f);
                                    background.transform.localPosition += new Vector3(0, nameText.transform.localPosition.y - background.size.y / 2f + 0.05f, 0);
                                    maskArea.transform.localPosition = background.transform.localPosition + new Vector3(0f, 0.02f, 0f);
                                    AddEndorseButtonsToChatbubble(ID, chatbubble.gameObject, background.size, isLocalPlayer);
                                }
                                return;
                        }
                    }
                }
            }
            content = ChatContent.DEFAULT;  // set back to default
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
