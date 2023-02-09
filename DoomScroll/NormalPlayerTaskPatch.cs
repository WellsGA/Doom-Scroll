using HarmonyLib;
using Il2CppSystem.Text;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(NormalPlayerTask))]
    class NormalPlayerTaskPatchtch
    {
      /*[HarmonyPostfix]
        [HarmonyPatch("AppendTaskText")]
        public static void PostfixAppendTaskText(NormalPlayerTask __instance, ref StringBuilder sb)
        {
            sb.Append("(SWC goes here)");
            //SecondaryWinConditionHolder.getThisPlayerSWC().SWCAssignText()
        }*/

       /* [HarmonyPrefix]
        [HarmonyPatch("NextStep")]
        public static void PrefiNextStep(NormalPlayerTask __instance)
        {
            if (__instance.taskStep >= __instance.MaxStep)
            {
                GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(PlayerControl.LocalPlayer.PlayerId);
                foreach (GameData.TaskInfo task in playerInfo.Tasks)
                {
                    if(task.Id == __instance.Id)
                    {
                        TaskAssigner.Instance.AssignPlayerToTask(task.TypeId);
                    }
                }
            }
        }*/
    }
}
