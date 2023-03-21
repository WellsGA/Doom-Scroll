using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(Minigame))]
    public static class MiniGamePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Begin")]
        public static void PostfixBegin(Minigame __instance, PlayerTask task) 
        {
            // no prefab or not a player task or not among the assignable tasks
            if(__instance == null || __instance.TaskType == TaskTypes.None 
                || !TaskAssigner.Instance.AssignableTasksIDs.Contains(task.Id)) return;
            
            // GameObject closeBtn = __instance.GetComponentInChildren<CloseButtonConsoleBehaviour>().gameObject; // didn't work, more than one go has this class
            GameObject closeBtn = __instance.transform.Find("CloseButton").gameObject;
            if (closeBtn != null)
            {
                TaskAssigner.Instance.CreateTaskAssignerPanel(closeBtn, task.Id);  
            }
            DoomScroll._log.LogInfo("No Close button... :( ");
        }

    }
}
