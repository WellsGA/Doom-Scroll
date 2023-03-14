using HarmonyLib;
using UnityEngine;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(Minigame))]
    public static class MiniGamePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Begin")]
        public static void PostfixBegin(Minigame __instance) 
        {
            if(__instance == null) return;
            GameObject closeBtn = __instance.GetComponentInChildren<CloseButtonConsoleBehaviour>().gameObject;
            if (closeBtn != null)
            {
                TaskAssigner.Instance.CreateTaskAssignerPanel(closeBtn);
                DoomScroll._log.LogInfo("Close button found, adding panel ");
            }
        }
    }
}
