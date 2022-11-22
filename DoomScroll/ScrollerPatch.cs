using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;

namespace Doom_Scroll
{
    //Disable chat scrolling limit:
    [HarmonyPatch(typeof(Scroller), nameof(Scroller.Update))]
    public static class ScrollerUpdatePatch
    {
        private static bool m_ScrollerPatchHasRun = false;
        public static void Postfix(Scroller __instance)
        {

            if (!m_ScrollerPatchHasRun)
            {
                __instance.ContentYBounds = new FloatRange(float.MinValue, float.MaxValue);
            }
        }
    }
}
