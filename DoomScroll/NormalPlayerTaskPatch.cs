using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;
using Microsoft.Extensions.Logging;
using Il2CppSystem;
using Il2CppSystem.Text;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(NormalPlayerTask))]
    class NormalPlayerTaskPatchtch
    {
        [HarmonyPostfix]
        [HarmonyPatch("AppendTaskText")]
        public static void PostfixAppendTaskText(NormalPlayerTask __instance, ref StringBuilder sb)
        {
            sb.Append("(SWC goes here)");
            //SecondaryWinConditionHolder.getThisPlayerSWC().SWCAssignText()
        }
    }
}
