using HarmonyLib;
using Il2CppSystem.Text;
using UnityEngine;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(NormalPlayerTask))]
    class NormalPlayerTaskPatch
    {
        /*[HarmonyPostfix]
          [HarmonyPatch("AppendTaskText")]
          public static void PostfixAppendTaskText(NormalPlayerTask __instance, ref StringBuilder sb)
          {
              sb.Append("(SWC goes here)");
              //SecondaryWinConditionHolder.getThisPlayerSWC().SWCAssignText()
          }*/

        [HarmonyPrefix]
        [HarmonyPatch("NextStep")]
        public static void PrefixNextStep(NormalPlayerTask __instance)
        {
            if(__instance.Id != TaskAssigner.Instance.CurrentMinigameTask) return;
            // if current is the last step, set maxstep to one more and activate the assign panel

        }
    }
}
