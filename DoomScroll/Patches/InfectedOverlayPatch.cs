using HarmonyLib;

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
            DoomScroll._log.LogInfo("Sabotage!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            HeadlineCreator.RpcSabotageContribution(false);
        }
    }
}
