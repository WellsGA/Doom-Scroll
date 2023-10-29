using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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