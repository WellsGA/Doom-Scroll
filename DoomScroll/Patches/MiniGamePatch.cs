using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(Minigame))]
    public static class MiniGamePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Begin")]
        public static void PostfixBegin(Minigame __instance, PlayerTask task)
        {
            // no prefab or not a player task or no assignable tasks
            if (__instance == null || __instance.TaskType == TaskTypes.None || TaskAssigner.Instance.AssignableTasks == null) return;
            DoomScroll._log.LogInfo("MiniGame Begin method called");
            foreach (uint pt in TaskAssigner.Instance.AssignableTasks)
            {
                if (pt == task.Id)
                {
                    DoomScroll._log.LogInfo("Task Assignable - task id: " + task.Id + ", pt id" + pt);

                    TaskAssigner.Instance.ActivatePanel(true);
                    TaskAssigner.Instance.SetCurrentMinigameTask(pt); //set the active assignable task id
                    break;
                }
                DoomScroll._log.LogInfo("Task NOT Assignable");
            }
        }

    }
}
