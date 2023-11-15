using HarmonyLib;

namespace Doom_Scroll.Patches
{
    //ONLY WORKS when actaul arrow is clicked. So slightly helpful, but not super necessary and kinda useless.
    [HarmonyPatch(typeof(JoinGameButton))]
    class JoinGameButtonPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnClick")]
        public static void PostfixOnClick()
        {
            if (MMOnlineManagerPatch.credits_overlay.IsModalOpen)
            {
                MMOnlineManagerPatch.credits_overlay.CloseButton.ButtonEvent.InvokeAction();
            }
        }
    }
}