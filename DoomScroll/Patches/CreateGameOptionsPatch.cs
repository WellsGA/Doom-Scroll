using HarmonyLib;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(CreateGameOptions))]
    class CreateGameOptionsPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        public static void PostfixShow()
        {
            MMOnlineManagerPatch.hostCreatingGame = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Hide")]
        public static void PostfixHide()
        {
            MMOnlineManagerPatch.hostCreatingGame = false;
        }
    }
}
