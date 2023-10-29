using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(InfectedOverlay))]
    class InfectedOverlayPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        public static void OnEnablePostfix()
        {
            HeadlineCreator.AddToStartedSabotage(PlayerControl.LocalPlayer.PlayerId);
            HeadlineCreator.RpcSabotageContribution(false);
        }
    }
}
