/*using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;
using Microsoft.Extensions.Logging;
using Il2CppSystem;
using Il2CppSystem.Text;
using Doom_Scroll.UI;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(MainMenuManager))]
    class MainMenuManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Activate")]
        public static void PostfixActivate(MainMenuManager __instance)
        {
            
            GameObject m_UIParent = __instance.playerCustomizationPrefab.transform.parent.gameObject;
            Vector3 doomscrollBtnPos = __instance.playerCustomizationPrefab.transform.parent.gameObject.transform.position;
            SpriteRenderer mapButtonSr = hud.MapButton.GetComponent<SpriteRenderer>();
            Vector3 position = new Vector3(doomscrollBtnPos.x, doomscrollBtnPos.y - mapButtonSr.size.y * hud.MapButton.transform.localScale.y, doomscrollBtnPos.z);
            Vector2 scaledSize = mapButtonSr.size * hud.MapButton.transform.localScale;
            Vector4[] slices = { new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 1, 0.5f) };
            Sprite[] cameraBtnSprites = ImageLoader.ReadImageSlicesFromAssembly(Assembly.GetExecutingAssembly(), "Doom_Scroll.Assets.cameraFlash.png", slices);

            return new CustomButton(m_UIParent, cameraBtnSprites, position, scaledSize.x, "Camera Toggle Button");
            
        }
    }
}*/