using HarmonyLib;
using Il2CppSystem.Text;

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
    }
}
