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

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(LobbyBehaviour))]
    class LobbyBehaviourPatch
    {
        private static CustomButton m_tutorialModeToggleBtn;
        public static GameObject playerCountText;
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
            playerCountText = GameObject.Find("PlayerCounter_TMP");
            lobbyBehaviourInstance = __instance;
            //Create tooltip
            DoomScroll._log.LogInfo("Lobby starting! Trying to add tooltip.");
            GameObject uiParent = playerCountText;
            lobbyToolTipText = new CustomText(uiParent, "LobbyTooltip", "<b>Recommended Rules</b>:\r\n-No Voice Chat! To simulate a social media discussion,\n only use the text chat during meetings.\r\n-Add 30 seconds to Meetings. Use the extra time to \nexamine the evidence in the folder.");
            lobbyToolTipText.SetColor(Color.yellow);
            lobbyToolTipText.SetSize(3f);
            Vector3 textPos = uiParent.transform.localPosition;
            textPos = new Vector3(uiParent.transform.localPosition.x - 5f, 7.6f, -10);
            //Vector3 textPos = new Vector3(-3, -1.5f, -10);
            lobbyToolTipText.SetLocalPosition(textPos);
            lobbyToolTipText.ActivateCustomUI(true);
            DoomScroll._log.LogInfo("Text should be activated!");

            Tooltip.ResetCurrentTooltips();

            //lobby tutorial mode button
            m_tutorialModeToggleBtn = CreateTutorialModeToggleBtn(playerCountText);
            m_tutorialModeToggleBtn.ButtonEvent.MyAction += Tooltip.ToggleTutorialMode;
            DoomScroll._log.LogInfo("Button event added to button.");
            m_tutorialModeToggleBtn.EnableButton(true);
            m_tutorialModeToggleBtn.ActivateCustomUI(true);

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
            if (!gameBegun && playerCountText != null && playerCountText.activeSelf)
            {
                tutorialBookletManagerInstance.CheckForButtonClicks();
                LobbyCheckForButtonClicks();
            }
        }

        public static CustomButton CreateTutorialModeToggleBtn(GameObject parent)
        {
            Vector3 pos = parent.transform.localPosition;
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector2 size = sr ? sr.size : new Vector2(1f, 1f);
            Vector3 position = new(pos.x + size.x * 3.0f, pos.y + size.y * 5f+1f, pos.z);
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] btnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.tutorialModeToggle.png", slices);
            Sprite[] threeBtnSprites = new Sprite[] { btnSprites[0], btnSprites[1], btnSprites[1] };
            CustomButton tutorialModeBtn = new CustomButton(parent, "TutorialBookletToggleButton", btnSprites, position, size.x);
            tutorialModeBtn.ActivateCustomUI(false);
            return tutorialModeBtn;
        }

        public static void LobbyCheckForButtonClicks()
        {

            // If the Tutorial Mode toggle button is active invoke toggle on mouse click 
            if (m_tutorialModeToggleBtn != null)
            {
                try
                {
                    m_tutorialModeToggleBtn.ReplaceImgageOnHover();
                    if (m_tutorialModeToggleBtn.IsHovered())
                    {
                    }
                    if (m_tutorialModeToggleBtn.IsHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        m_tutorialModeToggleBtn.ButtonEvent.InvokeAction();
                    }
                }
                catch (System.Exception e)
                {
                    DoomScroll._log.LogError("Error invoking tutorialBookletToggle: " + e);
                }
            }
        }
    }
}
