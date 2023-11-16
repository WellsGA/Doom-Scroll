using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;
using Doom_Scroll.UI;
using System.Reflection;


namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(MMOnlineManager))]
    class MMOnlineManagerPatch
    {
        private static Vector2 buttonSize = new Vector2(1.5f, 1.5f);
        public static CustomButton DoomScrollInfoToggle;
        public static CustomModal credits_overlay;
        public static CustomButton link_button_pre;
        public static CustomButton link_button_post;
        public static bool hostCreatingGame;

        private static MMOnlineManager MMOnlineManagerInstance;

        public static void OpenPreLink()
        {
            Application.OpenURL("https://uci.co1.qualtrics.com/jfe/form/SV_6zo325uPNFDM2zQ");
        }

        public static void OpenPostLink()
        {
            Application.OpenURL("https://uci.co1.qualtrics.com/jfe/form/SV_eL2BZfLKHFZvgY6");
        }

        private static CustomModal CreateCreditsOverlay(GameObject parent, CustomButton toggler)
        {
            Sprite spr = ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.folderOverlay.png");
            // create the overlay background
            CustomModal creditsOverlay = new CustomModal(parent, "CreditsOverlay", spr, toggler, true);
            creditsOverlay.SetLocalPosition(new Vector3(0f, 0f, -60f));
            creditsOverlay.SetSize(6.2f);
            // deactivate by default
            creditsOverlay.ActivateCustomUI(false);
            return creditsOverlay;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        public static void PostfixUpdate()
        {
            CheckButtonClicks();
        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(MMOnlineManager __instance)
        {
            hostCreatingGame = false;
            MMOnlineManagerInstance = __instance;
            //<<CREATE DOOMSCROLL BUTTON>>
            DoomScroll._log.LogInfo("About to set up for credits toggle button...");
            DoomScroll._log.LogInfo("Getting BackButton...");
            GameObject m_UIParent = __instance.BackButton.transform.gameObject;

            DoomScroll._log.LogInfo("Getting HelpButton from BackButton...");
            // GameObject helpButton = m_UIParent.transform.parent.transform.Find("HelpButton").gameObject; // this is null in the local mode
            GameObject helpButton = GameObject.Find("HelpButton");
            DoomScroll._log.LogInfo(helpButton == null ? "NO HELP BUTTON" : "Got HelpButton");
            Sprite[] doomscrollBtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.MainMenu_Button_Green.png", ImageLoader.slices2);
            Vector3 doomscrollBtnPos = helpButton.transform.position;
            SpriteRenderer doomscrollButtonSr = helpButton.GetComponent<SpriteRenderer>(); // it can be null! unchecked
            Vector3 position = new Vector3(doomscrollBtnPos.x + 4f, doomscrollBtnPos.y + 0.3f, doomscrollBtnPos.z - 10);
            Vector2 scaledSize = doomscrollButtonSr.size * helpButton.transform.localScale * 2;
            DoomScroll._log.LogInfo("About to create credits toggle button...");

            DoomScrollInfoToggle = new CustomButton(m_UIParent, "DoomScroll Info Toggle Button", doomscrollBtnSprites, position, scaledSize.x);
            DoomScroll._log.LogInfo("Credits toggle button created");

            //<<CREATE CREDITS OVERLAY>>
            DoomScroll._log.LogInfo("About to create credits overlay...");
            GameObject overlayParent = GameObject.Find("NormalMenu"); // it is null in local game mode
            credits_overlay = CreateCreditsOverlay(overlayParent == null ? __instance.gameObject : overlayParent, DoomScrollInfoToggle); // another alternative may be better
            DoomScroll._log.LogInfo("Credits overlay created");

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
            credits_text.SetSize(1f);
            Vector3 textPos = new Vector3(0, 0, -10);
            credits_text.SetLocalPosition(textPos);
            credits_text.TextMP.m_enableWordWrapping = false;
            credits_text.ActivateCustomUI(true);

            //<<CREATE LINK BUTTON LABEL TEXT>>
            string linkText = "<b><----</b>Take the <b>Pre-Test</b> on the left before playing\nTake the <b>Post-Test</b> on the right after playing<b>----></b>";
            CustomText link_text = new CustomText(credits_overlay.UIGameObject, "DoomScrollTeamCredits", linkText);
            link_text.SetColor(Color.black);
            link_text.SetSize(1f);
            Vector3 linkTextPos = textPos + new Vector3(0, -credits_overlay.GetSize().y / 2 + 1.2f, 0);
            link_text.SetLocalPosition(linkTextPos);
            link_text.TextMP.m_enableWordWrapping = false;
            link_text.ActivateCustomUI(true);

            //<<CREATE LINK BUTTON>>
            Vector3 link_button_pre_pos = textPos + new Vector3(-5, -5.8f, 0);
            link_button_pre = new CustomButton(credits_overlay.UIGameObject, "Open Pre Survey", doomscrollBtnSprites, link_button_pre_pos, buttonSize.x);
            link_button_pre.ButtonEvent.MyAction += OpenPreLink;

            Vector3 link_button_post_pos = textPos + new Vector3(5, -5.8f, 0);
            link_button_post = new CustomButton(credits_overlay.UIGameObject, "Open Post Survey", doomscrollBtnSprites, link_button_post_pos, buttonSize.x);
            link_button_post.ButtonEvent.MyAction += OpenPostLink;

        }
        public static void CheckButtonClicks()
        {
            if (MMOnlineManagerInstance == null || credits_overlay == null ) return;
            DoomScrollInfoToggle.ReplaceImgageOnHover();
            try
            {
                // Invoke methods on mouse click - open DoomScroll info popup
                credits_overlay.ListenForButtonClicks();
            }
            catch (System.Exception e)
            {
                DoomScroll._log.LogError("Error invoking method: " + e);
            }
            try
            {
                if (link_button_pre.IsHovered() && link_button_pre.IsActive && Input.GetKeyUp(KeyCode.Mouse0))
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
                if (link_button_post.IsHovered() && link_button_post.IsActive && Input.GetKeyUp(KeyCode.Mouse0))
                {
                    link_button_post.ButtonEvent.InvokeAction();
                }
            }
            catch (System.Exception e)
            {
                DoomScroll._log.LogError("Error invoking overlay button method: " + e);
            }
        }

    }
}