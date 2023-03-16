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
            // GameObject parent = __instance.gameObject.transform.Find("Background").gameObject;
            GameObject closeBtn = __instance.gameObject.transform.Find("CloseButton").gameObject;
            if(closeBtn)
            {
                TaskAssigner.Instance.CreateTaskAssignerPanel(__instance.gameObject, closeBtn);
                DoomScroll._log.LogInfo("Background found, adding panel ");

            }
            else
            {
                DoomScroll._log.LogInfo("No background found in Minigame: " + __instance.TaskType);
            }
        }
    }
}
