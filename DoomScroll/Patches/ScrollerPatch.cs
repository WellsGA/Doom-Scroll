﻿namespace Doom_Scroll.Patches
{
    //Disable chat scrolling limit:
    //[HarmonyPatch(typeof(Scroller), nameof(Scroller.Update))]
    //public static class ScrollerUpdatePatch
    //{
    //    private static bool m_ScrollerPatchHasRun = false;
    //    public static void Postfix(Scroller __instance)
    //    {
    //        if (GameObject.Find("ChatUI") != null)
    //        {
    //            __instance.ContentYBounds = new FloatRange(float.MinValue, float.MaxValue);
    //        }
    //    }
    //}
}