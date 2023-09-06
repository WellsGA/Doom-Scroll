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
    [HarmonyPatch(typeof(CreateGameOptions))]
    class CreateGameOptionsPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        public static void PostfixShow()
        {
            MMOnlineManagerPatch.hostCreatingGame = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Hide")]
        public static void PostfixHide()
        {
            MMOnlineManagerPatch.hostCreatingGame = false;
        }
    }
}
