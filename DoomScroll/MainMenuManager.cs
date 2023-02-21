/*using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;
using Microsoft.Extensions.Logging;
using Il2CppSystem;
using Il2CppSystem.Text;
using Doom_Scroll.UI;
using System.Reflection;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(MainMenuManager))]
    class MainMenuManagerPatch
    {
        public GenericPopup PopupPrefab;
        public DialogueBox Dialogue;

        public void ShowPopUp(string text)
        {
            this.Dialogue.Show(text);
        }

        /*
        private void ShowPopup(string error)
        {
            if (this.PopupPrefab)
            {
                GenericPopup genericPopup = Object.Instantiate<GenericPopup>(this.PopupPrefab);
                genericPopup.TextAreaTMP.text = error;
                genericPopup.transform.SetWorldZ(base.transform.position.z - 1f);
            }
        }*/
/*

        [HarmonyPostfix]
        [HarmonyPatch("Activate")]
        public static void PostfixActivate(MainMenuManager __instance)
        {

            //GameObject m_UIParent = __instance.playerCustomizationPrefab.transform.parent.gameObject;
            GameObject m_UIParent = __instance.DefaultButtonSelected.transform.parent.gameObject.transform.Find("BottomButtons").gameObject;
            GameObject inventbutton = m_UIParent.transform.Find("InventoryButton").gameObject;
            Vector3 doomscrollBtnPos = inventbutton.gameObject.transform.position;
            SpriteRenderer mapButtonSr = inventbutton.GetComponent<SpriteRenderer>();
            Vector3 position = new Vector3(doomscrollBtnPos.x, doomscrollBtnPos.y - mapButtonSr.size.y * inventbutton.transform.localScale.y, doomscrollBtnPos.z);
            Vector2 scaledSize = mapButtonSr.size * inventbutton.transform.localScale;
            //Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] cameraBtnSprites = [ImageLoader.ReadImageFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.MainMenu_Button copy.png")];

            SecondaryWinCondition.test_button = new CustomButton(m_UIParent, cameraBtnSprites, position, scaledSize.x, "Camera Toggle Button");
            
        }
    }
}*/