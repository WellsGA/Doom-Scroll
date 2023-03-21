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
using Sentry.Internal;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(MainMenuManager))]
    class MainMenuManagerPatch
    {
        private static Vector2 buttonSize = new Vector2(0.5f, 0.5f);
        public static CustomButton test_button;
        public static CustomModal credits_overlay;
        public static CustomButton close_button;
        public static DialogueBox Dialogue = new DialogueBox();
        public static CreditsScreenPopUp our_credits = new();
        public static bool AreCreditsOpen { get; private set; }

        public static void ShowPopUp(string text)
        {
            Dialogue.Show(text);
        }

        private static MainMenuManager mainMenuManagerInstance;
        
        [HarmonyPrefix]
        [HarmonyPatch("LateUpdate")]
        public static void PrefixLateUpdate()
        {
            CheckButtonClicks();
        }


        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(MainMenuManager __instance)
        {
            AreCreditsOpen = false;

            MainMenuManager mainMenuManagerInstance = __instance;
            GameObject m_UIParent = GameObject.Find("BottomButtons").gameObject;
            GameObject inventoryButton = GameObject.Find("InventoryButton").gameObject;
            Vector3 doomscrollBtnPos = inventoryButton.gameObject.transform.position;
            SpriteRenderer doomscrollButtonSr = inventoryButton.GetComponent<SpriteRenderer>();
            Vector3 position = new Vector3(doomscrollBtnPos.x - doomscrollButtonSr.size.x * inventoryButton.transform.localScale.x, doomscrollBtnPos.y, doomscrollBtnPos.z-70);
            Vector2 scaledSize = doomscrollButtonSr.size * inventoryButton.transform.localScale;
            scaledSize = scaledSize / 2;
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] doomscrollBtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.MainMenu_Button_Green.png", slices);
            test_button = new CustomButton(m_UIParent, "DoomScroll Info Toggle Button", doomscrollBtnSprites, position, scaledSize.x);
            DoomScroll._log.LogInfo("1:" + doomscrollBtnSprites[0].bounds.size + " , 2:  " + doomscrollBtnSprites[1].bounds.size);

            test_button.ButtonEvent.MyAction += OnClickDoomScroll;
            test_button.ActivateCustomUI(true);

            credits_overlay = InitCreditsOverlay(GameObject.Find("MainUI").gameObject);
            close_button = AddCloseButton(credits_overlay.UIGameObject);
            close_button.ButtonEvent.MyAction += ToggleOurCredits;
        }
        public static CustomModal InitCreditsOverlay(GameObject parent)
        {
            CustomModal creditsOverlay = CreateCreditsOverlay(parent);

            return creditsOverlay;
        }
        public static CustomModal CreateCreditsOverlay(GameObject parent)
        {
            SpriteRenderer bannerSR = GameObject.Find("bannerLogo_AmongUs").GetComponent<SpriteRenderer>();
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.folderOverlay.png");

            // create the overlay background
            CustomModal creditsOverlay = new CustomModal(parent, "CreditsOverlay", spr);
            creditsOverlay.SetLocalPosition(new Vector3(0f, 0f, -50f));
            creditsOverlay.SetScale(parent.transform.localScale * 0.4f);
            // deactivate by default
            creditsOverlay.ActivateCustomUI(false);
            return creditsOverlay;
        }
        public static CustomButton AddCloseButton(GameObject parent)
        {
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            Vector3 position = new Vector3(-sr.size.x / 2 - buttonSize.x / 2, sr.size.y / 2 - buttonSize.y / 2, -5f);
            Sprite[] closeBtnImg = { ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.closeButton.png") };
            return new CustomButton(parent, "Close OurCredits", closeBtnImg, position, buttonSize.x);
        }

        public static void CheckButtonClicks()
        {
            if (mainMenuManagerInstance == null) return;
            //DoomScroll._log.LogMessage("Replacing hover image");
            test_button.ReplaceImgageOnHover();

            try
            {
                // Invoke methods on mouse click - opens DoomScroll info popup
                if (test_button.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    DoomScroll._log.LogMessage("Button clicked");
                    DoomScroll._log.LogMessage("Clicked and action being invoked");
                    test_button.ButtonEvent.InvokeAction();
                }
            }
            catch (System.Exception e)
            {
                DoomScroll._log.LogError("Error invoking method: " + e);
            }
            if (AreCreditsOpen)
            {
                try
                {
                    if (close_button.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        close_button.ButtonEvent.InvokeAction();
                    }
                }
                catch (System.Exception e)
                {
                    DoomScroll._log.LogError("Error invoking overlay button method: " + e);
                }
            }
        }

        public static void OnClickDoomScroll()
        {
            ToggleOurCredits();
        }
        public static void ToggleOurCredits()
        {
            if (AreCreditsOpen)
            {
                test_button.EnableButton(true);
                credits_overlay.ActivateCustomUI(false);
                //our_credits.enabled = false;
                AreCreditsOpen = false;
                DoomScroll._log.LogInfo("Closing");
            }
            else
            {
                test_button.EnableButton(false);
                credits_overlay.ActivateCustomUI(true);
                //our_credits.enabled = true;
                AreCreditsOpen = true;
                DoomScroll._log.LogInfo("opening");
            }
        }
    }
}