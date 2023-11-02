using HarmonyLib;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(SabotageTask))]
    internal class SabotageTaskPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("MarkContributed")]
        public static void MarkContributedPostfix()
        {
            HeadlineCreator.AddToFixSabotage(PlayerControl.LocalPlayer.PlayerId);
            HeadlineCreator.RpcSabotageContribution(true);
        }
    }
}