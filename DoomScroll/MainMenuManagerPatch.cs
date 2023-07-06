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
    /*
    [HarmonyPatch(typeof(MainMenuManager))]
    class MainMenuManagerPatch
    {
        private static Vector2 buttonSize = new Vector2(1.5f, 1.5f);
        public static CustomButton test_button;
        public static CustomModal credits_overlay;
        public static CustomButton close_button;
        public static CustomButton link_button;
        public static bool AreCreditsOpen { get; private set; }

        public static void OpenLink()
        {
            Application.OpenURL("https://www.google.com/");
        }

        private static MainMenuManager mainMenuManagerInstance;


        public static CustomModal CreateCreditsOverlay(GameObject parent)
        {
            SpriteRenderer bannerSR = mainMenuManagerInstance.DefaultButtonSelected.transform.gameObject.GetComponent<SpriteRenderer>();
            Sprite cardSprite = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.folderOverlay.png");

            // create the overlay background
            CustomModal creditsOverlay = new CustomModal(parent, "CreditsOverlay", cardSprite);
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

        public static void ToggleOurCredits()
        {
            if (AreCreditsOpen)
            {
                test_button.EnableButton(true);
                credits_overlay.ActivateCustomUI(false);
                close_button.EnableButton(false);
                link_button.EnableButton(false);
                AreCreditsOpen = false;
                DoomScroll._log.LogInfo("Closing");
            }
            else
            {
                test_button.EnableButton(false);
                credits_overlay.ActivateCustomUI(true);
                close_button.EnableButton(true);
                link_button.EnableButton(true);
                AreCreditsOpen = true;
                DoomScroll._log.LogInfo("opening");
            }
        }
    

        [HarmonyPrefix]
        [HarmonyPatch("LateUpdate")]
        public static void PrefixLateUpdate(MainMenuManager __instance)
        {
            //CheckButtonClicks();
        }


        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(MainMenuManager __instance)
        {
            AreCreditsOpen = false;
            mainMenuManagerInstance = __instance;

            GameObject m_UIParent = __instance.DefaultButtonSelected.transform.parent.gameObject;
            GameObject inventoryButton = m_UIParent.transform.GetChild(2).gameObject;
            Vector3 doomscrollBtnPos = inventoryButton.gameObject.transform.position;
            SpriteRenderer doomscrollButtonSr = inventoryButton.GetComponent<SpriteRenderer>();
            Vector3 position = new Vector3(doomscrollBtnPos.x - doomscrollButtonSr.size.x * inventoryButton.transform.localScale.x, doomscrollBtnPos.y, doomscrollBtnPos.z-10);
            Vector2 scaledSize = doomscrollButtonSr.size * inventoryButton.transform.localScale;
            scaledSize = scaledSize / 2;
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] doomscrollBtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.MainMenu_Button_Green.png", slices);

            test_button = new CustomButton(m_UIParent, "DoomScroll Info Toggle Button", doomscrollBtnSprites, position, scaledSize.x);

            test_button.ActivateCustomUI(false);
            test_button.ActivateCustomUI(true);
            test_button.UIGameObject.gameObject.SetActive(false);
            test_button.UIGameObject.gameObject.SetActive(true);

            test_button.ButtonEvent.MyAction += OnClickDoomScroll;

            credits_overlay = CreateCreditsOverlay(mainMenuManagerInstance.DefaultButtonSelected.transform.gameObject);
            close_button = AddCloseButton(credits_overlay.UIGameObject);
            close_button.ButtonEvent.MyAction += ToggleOurCredits;

            //<<CREATE CREDITS TEXT>>
            string creditsText = "DoomScroll is an Among Us mod intended to shape the gameplay towards developing the player’s\n" +
                " ability to identify and avoid misinformation. The game’s core mechanics are already well built\n" +
                " for creating social deception – players must listen to each other’s insights and perspectives\n" +
                " and determine who might not be telling the truth, lest they choose incorrectly and let the imposter\n" +
                " win. Our goal is to add even greater depth to this process by introducing elements commonly found\n" +
                " when assessing misinformation on social media. Some of these elements include:" +
                "\r\n\r\n<b>Secondary Win Conditions (SWC):</b> \nAdditional objectives for crewmates. Frame a rival player or protect an ally!" +
                "\r\n<b>Chat logs:</b> \nchat logs are no longer removed after each meeting, \nallowing you look back at everyone’s actions from earlier in the game." +
                "\r\n<b>Sign-in Forms:</b> \nWhile completing a task, sign in as another player to make it appear as if they completed it." +
                "\r\n<b>Headlines:</b> \nPlayers will randomly be given the chance to share a news headline with the rest of the group. \nThese hints might reveal the truth, or just create mass confusion!" +
                "\r\n<b>Folder System:</b> \nThe folder system allows you to pull up evidence (sign-in forms and headlines)\n during meetings to support your arguments." +
                "\r\n\r\n<b>DoomScroll Development Team:</b>" +
                "\r\n\r\n-                                                      Garrison Wells - Designer, Producer \t\t\t Alaina Klaes - Programmer, Artist                                        -" +
                "\r\n\r\n-                                                          Agnes Romhanyi - Lead Programmer \t\t\t    Ashley Chia Sun - Intern                                                          -" +
                "\r\n\r\nTwitter: @DoomScrollMod";
            CustomText credits_text = new CustomText(credits_overlay.UIGameObject, "DoomScrollTeamCredits", creditsText);
            credits_text.SetColor(Color.black);
            credits_text.SetSize(3f);
            Vector3 textPos = new Vector3(0, credits_overlay.GetSize().x / 2 + 0.5f-3f, -10);
            credits_text.SetLocalPosition(textPos);
            credits_text.TextMP.m_enableWordWrapping = false;

            //<<CREATE LINK BUTTON>>
            SpriteRenderer sr = credits_overlay.UIGameObject.GetComponent<SpriteRenderer>();
            Vector3 link_button_pos = textPos + new Vector3(0, -5.8f, 0);
            Sprite[] closeBtnImg = { ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.closeButton.png") };
            link_button = new CustomButton(credits_overlay.UIGameObject, "Close OurCredits", doomscrollBtnSprites, link_button_pos, buttonSize.x);
            link_button.ButtonEvent.MyAction += OpenLink;

        }
        public static void CheckButtonClicks()
        {
            if (mainMenuManagerInstance == null) return;

            test_button.ReplaceImgageOnHover();

            try
            {
                // Invoke methods on mouse click - open DoomScroll info popup
                if (test_button.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    test_button.ButtonEvent.InvokeAction();
                }
            }
            catch (System.Exception e)
            {
                DoomScroll._log.LogError("Error invoking method: " + e);
            }

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

            try
            {
                if (link_button.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    link_button.ButtonEvent.InvokeAction();
                }
            }
            catch (System.Exception e)
            {
                DoomScroll._log.LogError("Error invoking overlay button method: " + e);
            }
        }

        public static void OnClickDoomScroll()
        {
            ToggleOurCredits();
        }
    }
    */
}