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
        public static TutorialBookletManager tutorialBookletManagerInstance;
        public static CustomButton tutorialBookletButton;
        public static CustomModal tutorialBookletOverlay;
        public static bool gameBegun = false;

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(LobbyBehaviour __instance)
        {
            gameBegun = false;
            //bottomCodeText = GameObject.Find("GameRoomButton");
            bottomCodeText = GameObject.Find("PlayerCounter_TMP");
            lobbyBehaviourInstance = __instance;
            //Create tooltip
            DoomScroll._log.LogInfo("Lobby starting! Trying to add tooltip.");
            GameObject uiParent = bottomCodeText;
            lobbyToolTipText = new CustomText(uiParent, "LobbyTooltip", "<b>Recommended Rules</b>:\r\n-No Voice Chat! To simulate a social media discussion,\n only use the text chat during meetings.\r\n-Add 30 seconds to Meetings. Use the extra time to \nexamine the evidence in the folder.");
            lobbyToolTipText.SetColor(Color.yellow);
            lobbyToolTipText.SetSize(3f);
            Vector3 textPos = uiParent.transform.localPosition;
            textPos = new Vector3(uiParent.transform.localPosition.x-5f, 7.6f, -10);
            //Vector3 textPos = new Vector3(-3, -1.5f, -10);
            lobbyToolTipText.SetLocalPosition(textPos);
            lobbyToolTipText.ActivateCustomUI(true);
            DoomScroll._log.LogInfo("Text should be activated!");

            if (tutorialBookletManagerInstance != null)
            {
                tutorialBookletManagerInstance.Reset();
            }
            tutorialBookletManagerInstance = TutorialBookletManager.Instance;

            //Create tutorial booklet button
            //tutorialBookletButton = TutorialBookletOverlay.CreateTutorialBookletBtn(bottomCodeText);

            //Create overlay
            //tutorialBookletOverlay = TutorialBookletOverlay.CreateTutorialBookletOverlay(bottomCodeText);
            
            //CODE FOR FINDING ALL THE OBJECTS IN THE SCENE
            /*GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            foreach (GameObject go in allObjects)
            {
                DoomScroll._log.LogInfo(go.name + " is an object in the scene");
            }*/
        }
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void PostfixUpdate()
        {
            if (!gameBegun && bottomCodeText != null && bottomCodeText.activeSelf)
            {
                tutorialBookletManagerInstance.CheckForButtonClicks();
            }
        }
    }
}
