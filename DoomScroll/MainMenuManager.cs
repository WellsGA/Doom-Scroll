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

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(MainMenuManager))]
    class MainMenuManagerPatch
    {
        public static CustomButton test_button;
        public static GenericPopup PopupPrefab;
        public static DialogueBox Dialogue = new DialogueBox();
        public static CreditsScreenPopUp our_credits = new CreditsScreenPopUp();

        public static void ShowPopUp(string text)
        {
            Dialogue.Show(text);
        }

        private static MainMenuManager mainMenuManagerInstance;

        //DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverImpostorKills, Array.Empty<object>()));


        /*private void ShowPopup(string error)
        {
            if (this.PopupPrefab)
            {
                GenericPopup genericPopup = Object.Instantiate<GenericPopup>(this.PopupPrefab);
                genericPopup.TextAreaTMP.text = error;
                genericPopup.transform.SetWorldZ(base.transform.position.z - 1f);
            }
        }*/
        
        [HarmonyPrefix]
        [HarmonyPatch("LateUpdate")]
        public static void PrefixLateUpdate()
        {
           // CheckButtonClicks();
        }


            [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(MainMenuManager __instance)
        {

            mainMenuManagerInstance = __instance;
            //GameObject m_UIParent = __instance.playerCustomizationPrefab.transform.parent.gameObject;
            GameObject m_UIParent = __instance.gameObject.transform.Find("BottomButtons").gameObject;
            GameObject inventoryButton = m_UIParent.transform.Find("InventoryButton").gameObject;
            Vector3 doomscrollBtnPos = inventoryButton.gameObject.transform.position;
            SpriteRenderer doomscrollButtonSr = inventoryButton.GetComponent<SpriteRenderer>();
            Vector3 position = new Vector3(doomscrollBtnPos.x - doomscrollButtonSr.size.x * inventoryButton.transform.localScale.x, doomscrollBtnPos.y, doomscrollBtnPos.z);
            Vector2 scaledSize = doomscrollButtonSr.size * inventoryButton.transform.localScale;
            scaledSize = scaledSize / 2;
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] doomscrollBtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.MainMenu_Button_Green.png", slices);


            test_button = new CustomButton(m_UIParent, "DoomScroll Info Toggle Button", doomscrollBtnSprites, position, scaledSize.x);

            test_button.ActivateCustomUI(true);

            test_button.ButtonEvent.MyAction += OnClickDoomScroll;
            
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
        }

        public static void OnClickDoomScroll()
        {
            our_credits.enabled = true;
            test_button.EnableButton(false);

            //DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverTaskWin, Array.Empty<object>()));
            //mainMenuManagerInstance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverTaskWin, Array.Empty<object>()));
            ShowPopUp("DOOM SCROLL: A mod made by very cool people! :D");
        }
    }
}