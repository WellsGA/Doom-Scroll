using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Doom_Scroll.Common;

namespace Doom_Scroll.Patches
{
    //NEW Disable chat scrolling limit

    //DEPRECATED Disable chat scrolling limit:
    [HarmonyPatch(typeof(ObjectPoolBehavior), nameof(ObjectPoolBehavior.Reclaim))]
    public static class ObjectPoolBehaviorReclaimPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ReclaimOldest")]
        public static bool PrefixReclaimOldest(MainMenuManager __instance)
        {
            return false;
        }
    }
}
