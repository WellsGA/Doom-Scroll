using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;
using Microsoft.Extensions.Logging;
using Il2CppSystem;
using Il2CppSystem.Text;
using Doom_Scroll.UI;
using System.Reflection;
using UnityEngine.UI;
using TMPro;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(LobbyBehaviour))]
    class LobbyBehaviourPatch
    {
        public static GameObject bottomCodeText;
        public static CustomText lobbyToolTipText;
        public static LobbyBehaviour lobbyBehaviourInstance;

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(LobbyBehaviour __instance)
        {
            bottomCodeText = GameObject.Find("GameRoomButton");
            lobbyBehaviourInstance = __instance;
            DoomScroll._log.LogInfo("Lobby starting! Trying to add tooltip.");
            GameObject uiParent = bottomCodeText;
            lobbyToolTipText = new CustomText(uiParent, "LobbyTooltip", "<b>Recommended Rules</b>:\r\n-No Voice Chat! To simulate a social media discussion,\n only use the text chat during meetings.\r\n-Add 30 seconds to Meetings. Use the extra time to \nexamine the evidence in the folder.");
            lobbyToolTipText.SetColor(Color.yellow);
            lobbyToolTipText.SetSize(2f);
            Vector3 textPos = uiParent.transform.localPosition;
            textPos = new Vector3(uiParent.transform.localPosition.x, 4.6f, -10);
            //Vector3 textPos = new Vector3(-3, -1.5f, -10);
            lobbyToolTipText.SetLocalPosition(textPos);
            lobbyToolTipText.ActivateCustomUI(true);
            DoomScroll._log.LogInfo("Text should be activated!");
        }
    }
}
