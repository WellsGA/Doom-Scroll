using HarmonyLib;
using UnityEngine;
using Doom_Scroll.UI;
using Doom_Scroll.Common;
using System.Reflection;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(ProgressionScreen))]
    class ProgressionScreenPatch
    {
        private static Vector2 buttonSize = new Vector2(1.5f, 1.5f);
        public static CustomButton link_button;

        public static void OpenLink()
        {
            Application.OpenURL("https://www.google.com/");
        }

        [HarmonyPostfix]
        [HarmonyPatch("Activate")]
        public static void PostfixActivate(ProgressionScreen __instance)
        {
            DoomScroll._log.LogInfo("On Progression Screen, playerSWClist = " + SecondaryWinConditionManager.overallSWCResultsText());
            CustomText overallSWCText = new CustomText(__instance.XpBar.gameObject, "SWCResults", SecondaryWinConditionManager.overallSWCResultsText());
            overallSWCText.SetColor(Color.white);
            overallSWCText.SetSize(2f);
            Vector3 textPos = new Vector3(overallSWCText.UIGameObject.transform.localPosition.x, overallSWCText.UIGameObject.transform.localPosition.y - 1, overallSWCText.UIGameObject.transform.localPosition.z);
            overallSWCText.SetLocalPosition(textPos);
            SecondaryWinConditionManager.gameOver();

            //<<CREATE LINK BUTTON>>
            GameObject BloodSplat = GameObject.Find("UI_BloodSplat").gameObject;
            SpriteRenderer BloodSplatSR = BloodSplat.GetComponent<SpriteRenderer>();
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] doomscrollBtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.MainMenu_Button_Green.png", slices);
            //.UIGameObject.GetComponent<SpriteRenderer>();
            Vector3 link_button_pos = BloodSplat.gameObject.transform.localPosition + new Vector3(4.5f, 0, -10);
            Sprite[] closeBtnImg = { ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.closeButton.png") };
            link_button = new CustomButton(BloodSplat, "Close OurCredits", doomscrollBtnSprites, link_button_pos, buttonSize.x);
            link_button.ButtonEvent.MyAction += OpenLink;
        }
        /*
        public void Update()
        {
            CheckButtonClicks();
        }
        */

        /*
        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static void PrefixUpdate(ProgressionScreen __instance)
        {
            CheckButtonClicks();
        }
        */

        public static void CheckButtonClicks()
        {
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
    }
}
