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
    [HarmonyPatch(typeof(MMOnlineManager))]
    class MMOnlineManagerPatch
    {
        private static Vector2 buttonSize = new Vector2(1.5f, 1.5f);
        public static CustomButton test_button;
        public static CustomModal credits_overlay;
        public static CustomButton close_button;
        public static CustomButton link_button_pre;
        public static CustomButton link_button_post;
        public static GameObject gameCreationOptionsMenu;
        public static GameObject joinGameMenu;
        public static bool hostCreatingGame;
        public static bool AreCreditsOpen { get; private set; }

        public static void OpenPreLink()
        {
            Application.OpenURL("https://uci.co1.qualtrics.com/jfe/form/SV_6zo325uPNFDM2zQ");
        }

        public static void OpenPostLink()
        {
            Application.OpenURL("https://uci.co1.qualtrics.com/jfe/form/SV_eL2BZfLKHFZvgY6");
        }

        private static MMOnlineManager MMOnlineManagerInstance;


        public static CustomModal CreateCreditsOverlay(GameObject parent)
        {
            SpriteRenderer backButtonSR = MMOnlineManagerInstance.BackButton.transform.gameObject.GetComponent<SpriteRenderer>();
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

        public static void ToggleOurCredits()
        {
            if (AreCreditsOpen)
            {
                test_button.EnableButton(true);
                credits_overlay.ActivateCustomUI(false);
                close_button.EnableButton(false);
                link_button_pre.EnableButton(false);
                AreCreditsOpen = false;
                DoomScroll._log.LogInfo("Closing");
            }
            else
            {
                test_button.EnableButton(false);
                credits_overlay.ActivateCustomUI(true);
                close_button.EnableButton(true);
                link_button_pre.EnableButton(true);
                AreCreditsOpen = true;
                DoomScroll._log.LogInfo("opening");
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static void PrefixUpdate()
        {
            if (!hostCreatingGame)
            {
                CheckButtonClicks();
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(MMOnlineManager __instance)
        {
            hostCreatingGame = false;
            if (__instance.HelpMenu != null && __instance.HelpMenu.gameObject.activeSelf == false)
            {
                DoomScroll._log.LogInfo("About to set MMOnlineManager instance...");
                AreCreditsOpen = false;
                MMOnlineManagerInstance = __instance;

                //<<CREATE DOOMSCROLL BUTTON>>
                DoomScroll._log.LogInfo("About to set up for credits toggle button...");
                DoomScroll._log.LogInfo("Getting BackButton...");
                GameObject m_UIParent = __instance.BackButton.transform.gameObject;
                DoomScroll._log.LogInfo("Getting HelpButton from BackButton...");
                GameObject helpButton = m_UIParent.transform.parent.transform.Find("HelpButton").gameObject;
                DoomScroll._log.LogInfo("Got HelpButton");
                Vector3 doomscrollBtnPos = helpButton.gameObject.transform.position;
                SpriteRenderer doomscrollButtonSr = helpButton.GetComponent<SpriteRenderer>();
                Vector3 position = new Vector3(doomscrollBtnPos.x + 4f, doomscrollBtnPos.y + 0.3f, doomscrollBtnPos.z - 10);
                Vector2 scaledSize = doomscrollButtonSr.size * helpButton.transform.localScale;
                scaledSize = scaledSize * 3;
                Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
                Sprite[] doomscrollBtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.MainMenu_Button_Green.png", slices);

                DoomScroll._log.LogInfo("About to create credits toggle button...");
                test_button = new CustomButton(m_UIParent, "DoomScroll Info Toggle Button", doomscrollBtnSprites, position, scaledSize.x);
                DoomScroll._log.LogInfo("Credits toggle button created");

                test_button.ActivateCustomUI(false);
                test_button.ActivateCustomUI(true);
                test_button.UIGameObject.gameObject.SetActive(false);
                test_button.UIGameObject.gameObject.SetActive(true);

                test_button.ButtonEvent.MyAction += OnClickDoomScroll;

                //<<CREATE CREDITS OVERLAY>>
                DoomScroll._log.LogInfo("About to create credits overlay...");
                credits_overlay = CreateCreditsOverlay(GameObject.Find("NormalMenu").gameObject);
                DoomScroll._log.LogInfo("Credits overlay created");
                close_button = AddCloseButton(credits_overlay.UIGameObject);
                DoomScroll._log.LogInfo("Added close button");
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
                Vector3 textPos = new Vector3(0, credits_overlay.GetSize().x / 2 + 0.5f - 3f, -10);
                credits_text.SetLocalPosition(textPos);
                credits_text.TextMP.m_enableWordWrapping = false;

                //<<CREATE LINK BUTTON LABEL TEXT>>
                string linkText = "<b><----</b>Take the <b>Pre-Test</b> on the left before playing\nTake the <b>Post-Test</b> on the right after playing<b>----></b>";
                CustomText link_text = new CustomText(credits_overlay.UIGameObject, "DoomScrollTeamCredits", linkText);
                link_text.SetColor(Color.black);
                link_text.SetSize(3f);
                Vector3 linkTextPos = textPos + new Vector3(0, -5.7f, 0);
                link_text.SetLocalPosition(linkTextPos);
                link_text.TextMP.m_enableWordWrapping = false;

                //<<CREATE LINK BUTTON>>
                Vector3 link_button_pre_pos = textPos + new Vector3(-5, -5.8f, 0);
                link_button_pre = new CustomButton(credits_overlay.UIGameObject, "Open Pre Survey", doomscrollBtnSprites, link_button_pre_pos, buttonSize.x);
                link_button_pre.ButtonEvent.MyAction += OpenPreLink;

                Vector3 link_button_post_pos = textPos + new Vector3(5, -5.8f, 0);
                link_button_post = new CustomButton(credits_overlay.UIGameObject, "Open Post Survey", doomscrollBtnSprites, link_button_post_pos, buttonSize.x);
                link_button_post.ButtonEvent.MyAction += OpenPostLink;
            }

        }
        public static void CheckButtonClicks()
        {
            if (MMOnlineManagerInstance == null) return;

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
                if (link_button_pre.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    link_button_pre.ButtonEvent.InvokeAction();
                }
            }
            catch (System.Exception e)
            {
                DoomScroll._log.LogError("Error invoking overlay button method: " + e);
            }

            try
            {
                if (link_button_post.isHovered() && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    link_button_post.ButtonEvent.InvokeAction();
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
}