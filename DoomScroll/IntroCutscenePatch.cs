using HarmonyLib;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(IntroCutscene))]
    class IntroCutscenePatch
    {
        /*[HarmonyPrefix]
        [HarmonyPatch("BeginCrewmate")]
        public static void PrefixBeginCrewmate()
        {
            SecondaryWinConditionManager.InitSecondaryWinCondition(false);
        }
*/
        // displays SWC if local player is crewmate
        [HarmonyPostfix]
        [HarmonyPatch("BeginCrewmate")]
        public static void PostfixBeginCrewmate(IntroCutscene __instance)
        {
            if (SecondaryWinConditionManager.LocalPLayerSWC == null) return;
            __instance.TeamTitle.text += "\n<size=20%><color=\"yellow\">Secondary Win Condition: " + SecondaryWinConditionManager.LocalPLayerSWC.ToString() + "</color></size>";
            DoomScroll._log.LogInfo("SecondaryWinCondition showing under role assignment: " + SecondaryWinConditionManager.LocalPLayerSWC.ToString());
        }

        /* // replaces SWC with empty SWC if local player is impostor (impostors can't have SWCs)
         [HarmonyPrefix]
         [HarmonyPatch("BeginImpostor")]
         public static void PrefixBeginImpostor()
         {
             if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
             {
                 SecondaryWinConditionManager.InitSecondaryWinCondition(true);
             }
         }*/

        // displays SWC if local player is impostor
        [HarmonyPostfix]
        [HarmonyPatch("BeginImpostor")]
        public static void PostfixBeginImpostor(IntroCutscene __instance)
        {
            if (SecondaryWinConditionManager.LocalPLayerSWC == null) return;
            __instance.TeamTitle.text += "\n<size=20%><color=\"yellow\">Secondary Win Condition: " + SecondaryWinConditionManager.LocalPLayerSWC.ToString() + "</color></size>";
            DoomScroll._log.LogInfo("SecondaryWinCondition showing under role assignment: " + SecondaryWinConditionManager.LocalPLayerSWC.ToString());
        }

        // displays SWC on second role text screen
        // **NEED TO CHECK if this works!!**
        [HarmonyPrefix]
        [HarmonyPatch("ShowRole")]
        public static void PrefixShowRole(IntroCutscene __instance)
        {
            if (SecondaryWinConditionManager.LocalPLayerSWC == null) return;
            __instance.RoleText.text += "\n<size=10%><color=\"yellow\">Secondary Win Condition: " + SecondaryWinConditionManager.LocalPLayerSWC.ToString() + "</color></size>";
            DoomScroll._log.LogInfo("SecondaryWinCondition showing under second role screen: " + SecondaryWinConditionManager.LocalPLayerSWC.ToString());
        }
    }
}
