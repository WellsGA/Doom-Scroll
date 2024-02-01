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
        public static readonly int MAX_CHAT_BUBBLES = 10;

        [HarmonyPrefix]
        [HarmonyPatch("ReclaimOldest")]
        public static bool PrefixReclaimOldest(ObjectPoolBehavior __instance)
        {
            if (__instance.activeChildren.Count > MAX_CHAT_BUBBLES)
            {
                __instance.Reclaim(__instance.activeChildren[0]);
            }
            __instance.InitPool(__instance.Prefab);
            return false;
        }
    }
}
