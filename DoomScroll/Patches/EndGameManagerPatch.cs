using HarmonyLib;
using Il2CppSystem;
using UnityEngine;

namespace Doom_Scroll.Patches
{
    public enum CustomGameOverReason : byte
    {
        SWCNotCompleted
    }

    [HarmonyPatch(typeof(EndGameManager))]
    class EndGameManagerPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch("SetEverythingUp")]
        public static void PostfixSetEverythingUp(EndGameManager __instance)
        {
            if (__instance.WinText.text == DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Victory) && !DoomScrollVictoryManager.CheckVictory()) //If won but SWC not successful
            { // Case where they won but DoomScroll lost
                __instance.WinText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Defeat);
                __instance.WinText.color = Color.red;
                if (!SecondaryWinConditionManager.LocalPLayerSWC.CheckSuccess())
                {
                    __instance.WinText.text += "\n<size=20%><color=\"white\"> { SWC Results } <color=\"red\">" + SecondaryWinConditionManager.LocalPLayerSWC.SWCResultsText() + "</color></color></size>";
                }
                else
                {
                    __instance.WinText.text += "\n<size=20%><color=\"white\"> { SWC Results } <color=\"blue\">" + SecondaryWinConditionManager.LocalPLayerSWC.SWCResultsText() + "</color></color></size>";
                }

                if (!DoomScrollVictoryManager.CheckLocalImpostor() && !DoomScrollVictoryManager.CheckVotingSuccess())
                {
                    __instance.WinText.text += "\n<size=20%><color=\"white\"> { Voting Results } <color=\"red\">Failure: Some crewmates didn't vote correctly.</color></color></size>";
                }
                else if (!DoomScrollVictoryManager.CheckLocalImpostor())
                {
                    __instance.WinText.text += "\n<size=20%><color=\"white\"> { Voting Results } <color=\"blue\">Success: All crewmates voted correctly!</color></color></size>";
                }

            }
            else
            {
                if (!SecondaryWinConditionManager.LocalPLayerSWC.CheckSuccess())
                {
                    __instance.WinText.text += "\n<size=20%><color=\"white\"> { SWC Results } <color=\"red\">" + SecondaryWinConditionManager.LocalPLayerSWC.SWCResultsText() + "</color></color></size>";
                }
                else
                {
                    __instance.WinText.text += "\n<size=20%><color=\"white\"> { SWC Results } <color=\"blue\">" + SecondaryWinConditionManager.LocalPLayerSWC.SWCResultsText() + "</color></color></size>";
                }

                if (!DoomScrollVictoryManager.CheckLocalImpostor() && !DoomScrollVictoryManager.CheckVotingSuccess())
                {
                    __instance.WinText.text += "\n<size=20%><color=\"white\"> { Voting Results } <color=\"red\">Failure: Some crewmates didn't vote correctly.</color></color></size>";
                }
                else if (!DoomScrollVictoryManager.CheckLocalImpostor())
                {
                    __instance.WinText.text += "\n<size=20%><color=\"white\"> { Voting Results } <color=\"blue\">Success: All crewmates voted correctly!</color></color></size>";
                }
            }
        }
    }
}
