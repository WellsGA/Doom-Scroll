using HarmonyLib;
using Il2CppSystem.Text;
using UnityEngine;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(NormalPlayerTask))]
    class NormalPlayerTaskPatch
    {
        
        /*[HarmonyPrefix]
        [HarmonyPatch("NextStep")]
        public static void PrefixNextStep(NormalPlayerTask __instance)
        {
            if (__instance.taskStep + 1 >= __instance.MaxStep)
            {
                DoomScroll._log.LogInfo("last step!");
                TaskAssigner.Instance.ActivatePanel(__instance.Id, false);
            }
        }*/
    }
}
