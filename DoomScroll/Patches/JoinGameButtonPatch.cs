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
    //ONLY WORKS when actaul arrow is clicked. So slightly helpful, but not super necessary and kinda useless.
    [HarmonyPatch(typeof(JoinGameButton))]
    class JoinGameButtonPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnClick")]
        public static void PostfixOnClick()
        {
            if (MMOnlineManagerPatch.AreCreditsOpen)
            {
                MMOnlineManagerPatch.ToggleOurCredits();
            }
        }
    }
}