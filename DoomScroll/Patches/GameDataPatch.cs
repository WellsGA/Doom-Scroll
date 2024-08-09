using HarmonyLib;
using Hazel;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(GameData))]
    class GameDataPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch("AddPlayer")]   // TO DO: check if this is the best access point
        public static void PostfixAddPlayer(PlayerControl pc) 
        {
            ScreenshotManager.Instance.AddPlayerToTheWaitList(pc.PlayerId); // fills up the waitlist for screenshot taking
        }

        // Moved to the NetworkedPlayerInfo class and set to private ... cannot patch (easily)
        // [HarmonyPostfix] [HarmonyPatch("SetTasks")] public static void PostfixSetTasks(ref byte playerId){ }

    }
}
