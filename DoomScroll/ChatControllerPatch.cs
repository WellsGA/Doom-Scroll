using Doom_Scroll.Common;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Doom_Scroll
{
    public enum ChatContent
    {
        TEXT,
        SCREENSHOT,
    }

    // in the 2023.2.28 release ChatBubble is an internal class,
    // so we have to access the Gamobject that has the ChatBubble script on it and set the image to be displayed
    [HarmonyPatch(typeof(ChatController))]
    public class ChatControllerPatch
    {
        public static byte[] screenshot = null; // for sharing images
        public static ChatContent content = ChatContent.TEXT;

        [HarmonyPostfix]
        [HarmonyPatch("AddChat")]
        public static void PostfixAddChat(ChatController __instance, PlayerControl sourcePlayer, string chatText)
        {
            bool isLocalPlayer = sourcePlayer == PlayerControl.LocalPlayer;
            GameObject scroller = __instance.GetComponentInChildren<Scroller>().gameObject;
            TextMeshPro[] texts = scroller.gameObject.GetComponentsInChildren<TextMeshPro>();

            switch (content)
            {
                case ChatContent.TEXT:
                    return;

                case ChatContent.SCREENSHOT:
                    if (screenshot == null) return;
                    if (texts != null)
                    {
                        foreach (TextMeshPro text in texts)
                        {
                            if (text.text == chatText)
                            {
                                Transform chatbubble = text.transform.parent;
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

                                    // child elements of ChatBubble needed to set the image correctly
                                    TextMeshPro nameText = chatbubble.Find("NameText (TMP)").gameObject.GetComponent<TextMeshPro>();
                                    SpriteRenderer maskArea = chatbubble.Find("MaskArea").gameObject.GetComponent<SpriteRenderer>();
                                    SpriteRenderer background = chatbubble.Find("Background").gameObject.GetComponent<SpriteRenderer>();

                                    text.text = "";
                                    text.ForceMeshUpdate(true, true);
                                    Vector3 chatpos = text.transform.localPosition;
                                    float xOffset = isLocalPlayer ? -sr.size.x / 2 : sr.size.x / 2;
                                    image.transform.localPosition = new Vector3(chatpos.x + xOffset, chatpos.y - sr.size.y / 2 - 0.3f, chatpos.z);
                                    if (nameText != null && background != null && maskArea != null)
                                    {
                                        background.size = new Vector2(5.52f, 0.5f + nameText.GetNotDumbRenderedHeight() + text.GetNotDumbRenderedHeight() + sr.size.y);
                                        maskArea.size = background.size - new Vector2(0f, 0.08f);
                                        background.transform.localPosition = new Vector3(background.transform.localPosition.x, background.transform.localPosition.y - background.size.y / 3, background.transform.localPosition.z);
                                    }
                                    screenshot = null;
                                }
                                break;
                            }
                        }
                    }
                    return;
            }
        }
    }
}
