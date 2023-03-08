using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(ChatController))]
    public class ChatControllerPatch
    {
        public static byte[] screenshot = null;

        [HarmonyPostfix]
        [HarmonyPatch("AddChat")]
        public static void PostfixAddChat(ChatController __instance, PlayerControl sourcePlayer, string chatText)
        {
            bool flag = sourcePlayer == PlayerControl.LocalPlayer;
            TextMeshPro[] texts = __instance.scroller.gameObject.GetComponentsInChildren<TextMeshPro>();
            if (texts != null)
            {
                foreach (TextMeshPro text in texts)
                {
                    if(text.text == chatText)
                    {
                        GameObject chatbubble = text.transform.parent.gameObject;
                        DoomScroll._log.LogInfo("image bytes: " + screenshot.Length);
                        if(chatbubble != null && screenshot != null)
                        {
                            SendImageInChat.SetImage(flag, chatbubble, screenshot);
                        }
                    }
                }
            }
            
           // __instance.AlignAllBubbles(); // private method, use reflection?
        }
    }
}
