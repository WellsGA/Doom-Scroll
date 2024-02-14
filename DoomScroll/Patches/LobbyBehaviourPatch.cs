using HarmonyLib;
using UnityEngine;
using Doom_Scroll.UI;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(LobbyBehaviour))]
    class LobbyBehaviourPatch
    {
        public static GameObject playerCountText;
        public static CustomText lobbyToolTipText;
        public static LobbyBehaviour lobbyBehaviourInstance;
        /*public static CustomButton tutorialBookletButton;
        public static CustomModal tutorialBookletOverlay;*/
        public static bool gameBegun = false;

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(LobbyBehaviour __instance)
        {
            gameBegun = false;
            //bottomCodeText = GameObject.Find("GameRoomButton");
            playerCountText = GameObject.Find("PlayerCounter_TMP");
            lobbyBehaviourInstance = __instance;
            //Create tooltip
            DoomScroll._log.LogInfo("Lobby starting! Trying to add tooltip.");
            GameObject uiParent = playerCountText;
            lobbyToolTipText = new CustomText(uiParent, "LobbyTooltip", "<b>Recommended Rules</b>:\r\n-No Voice Chat! To simulate a social media discussion,\n only use the text chat during meetings.\r\n-Add 30 seconds to Meetings. Use the extra time to \nexamine the evidence in the folder.\r\n-7 players minimum.\r\n-Turn on Anonymous Votes.");
            lobbyToolTipText.SetColor(UnityEngine.Color.yellow);
            lobbyToolTipText.SetSize(3f);
            Vector3 textPos = uiParent.transform.localPosition;
            textPos = new Vector3(uiParent.transform.localPosition.x - 5f, 7.6f, -10);
            //Vector3 textPos = new Vector3(-3, -1.5f, -10);
            lobbyToolTipText.SetLocalPosition(textPos);
            lobbyToolTipText.ActivateCustomUI(true);
            DoomScroll._log.LogInfo("Text should be activated!");
        }
    }
}