using Doom_Scroll.UI;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Doom_Scroll.Patches
{
    [HarmonyPatch(typeof(ExileController))]
    static class ExileControllerPatch
    {
        private static string _retainedExileString;
        public static MeetingHud.VoterState[] OriginalArray2;
        public static NetworkedPlayerInfo OriginalExiledPlayer;
        public static bool OriginalTie;
        private static bool _exiledWasOverridden;

        // TO DO: Check this, no references!!
        public static void SetExileInfo(string retainedString, bool wasOverridden, byte ogExiledPlayer = 255)
        {
            _retainedExileString = retainedString;
            _exiledWasOverridden = wasOverridden;
            NetworkedPlayerInfo ogExiled = null;
            if (ogExiledPlayer != 255)
            {
                foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers)
                {
                    if (player.PlayerId == ogExiledPlayer)
                    {
                        ogExiled = player;
                        break;
                    }
                }
            }
            OriginalExiledPlayer = ogExiled;
        }


        [HarmonyPrefix]
        [HarmonyPatch("Begin")]
        public static bool PrefixBegin(ExileController __instance, object[] __args)
        {
            // DoomScroll._log.LogInfo($"Length of args: {__args.Length}");

            int numAliveImpostors = 0;
            int numImpostors = 0;
            int numAlivePlayers = 0;
            foreach (NetworkedPlayerInfo npi in GameData.Instance.AllPlayers)
            {
                if (npi.Role.IsImpostor)
                {
                    numImpostors++;
                    if (!npi.IsDead && !npi.Disconnected)
                    {
                        numAliveImpostors++;
                    }
                } else if (!npi.IsDead && !npi.Disconnected) {
                    numAlivePlayers++;
                }    
            }


            if (!DoomScrollVictoryManager.IsHeadlineVoteSuccess)
            {
                DoomScroll._log.LogInfo("Voting was not a success!");
                __instance.exiled = null;

                if (OriginalExiledPlayer != null)
                {
                    DoomScroll._log.LogInfo(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> OriginalExiledPlayer: " + OriginalExiledPlayer.ToString());
                    //CALLS SAME STUFF AS ACTUAL THING, THEN CANCELS METHOD IF WE MODIFYIING THE EXILE.
                    // If exicled is not impostor or the amount of living impostors left is > 1;
                    // basically, if the game doesn't end from them voting out this player, SET THE EXILED PLAYER BACK TO NORMAL AND RUN OUR VERSION OF THE REAL METHOD
                    if (!OriginalExiledPlayer.Role.IsImpostor || numAliveImpostors > 1)
                    {
                        DoomExileBeginLikeNormal(__instance, numAliveImpostors, numImpostors);
                        return false;
                    }
                    __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSP, (OriginalExiledPlayer.PlayerName));
                    _retainedExileString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSP, (OriginalExiledPlayer.PlayerName));
                    DoomScroll._log.LogInfo($"String set to: {__instance.completeString}");

                    //if exiled was overridden
                    _exiledWasOverridden = true;
                    __instance.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(__instance.Animate());

                    __instance.Player.UpdateFromEitherPlayerDataOrCache(OriginalExiledPlayer, PlayerOutfitType.Default, PlayerMaterial.MaskType.Exile, false);
                    // ACTION DELETED HERE, MIGHT NEED???

                    __instance.Player.ToggleName(false);
                    if (!__instance.useIdleAnim)
                    {
                        __instance.Player.SetCustomHatPosition(__instance.exileHatPosition);
                        __instance.Player.SetCustomVisorPosition(__instance.exileVisorPosition);
                    }

                    if (OriginalExiledPlayer.Role.IsImpostor)
                    {
                        numAliveImpostors--;
                    }

                    if (numAliveImpostors == 1)
                    {
                        __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorsRemainS, numAliveImpostors);
                    }
                    else
                    {
                        __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorsRemainP, numAliveImpostors);
                    }

                    return false;
                }
            }
            // IF THEY VOTED CORRECTLY, OR IF IMPOSTOR NOT VOTED OUT, THEY CAN VOTE OUT WHOEVER THEY WANT AND RUN THE REAL METHOD

            //ACTUALLY INSTEADDDDD, WE USE THEIR CODE AND GO TO THE NEXT THING. THEN PREVENT THEIR METHOD FROM RUNNING.
            DoomExileBeginLikeNormal(__instance, numAliveImpostors, numImpostors);
            return false;
        }

        public static void DoomExileBeginLikeNormal(ExileController __instance, int num, int num2)
        {

            if (__instance.specialInputHandler != null)
            {
                __instance.specialInputHandler.disableVirtualCursor = true;
            }
            ExileController.Instance = __instance;
            ControllerManager.Instance.CloseAndResetAll();
            if (OriginalExiledPlayer != null)
            {
                __instance.exiled = OriginalExiledPlayer;
            }
            __instance.Text.gameObject.SetActive(false);
            __instance.Text.text = string.Empty;

            if (OriginalExiledPlayer != null)
            {
                if (!GameManager.Instance.LogicOptions.GetConfirmImpostor())
                {
                    __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextNonConfirm, OriginalExiledPlayer.PlayerName);
                }
                else if (OriginalExiledPlayer.Role.IsImpostor)
                {
                    if (num2 > 1)
                    {
                        __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPP, OriginalExiledPlayer.PlayerName);
                    }
                    else
                    {
                        __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSP, OriginalExiledPlayer.PlayerName);
                    }
                }
                else if (num2 > 1)
                {
                    __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPN, OriginalExiledPlayer.PlayerName);
                }
                else
                {
                    __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSN, OriginalExiledPlayer.PlayerName);
                }

                __instance.Player.UpdateFromEitherPlayerDataOrCache(OriginalExiledPlayer, PlayerOutfitType.Default, PlayerMaterial.MaskType.Exile, false);
                // ACTION DELETED HERE, MIGHT NEED???

                __instance.Player.ToggleName(false);
                if (!__instance.useIdleAnim)
                {
                    __instance.Player.SetCustomHatPosition(__instance.exileHatPosition);
                    __instance.Player.SetCustomVisorPosition(__instance.exileVisorPosition);
                }
                if (OriginalExiledPlayer.Role.IsImpostor)
                {
                    num--;
                }
            }
            else
            {
                if (OriginalTie)
                {
                    __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoExileTie);
                }
                else
                {
                    __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoExileSkip);
                }
                __instance.Player.gameObject.SetActive(false);
            }
            if (num == 1)
            {
                __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorsRemainS, num);
            }
            else
            {
                __instance.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorsRemainP, num);
            }
            __instance.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(__instance.Animate());
        }

        [HarmonyPostfix]
        [HarmonyPatch("Begin")]
        public static void PostfixBegin(ExileController __instance)
        {
            int numAliveImpostors = 0;
            int numCrewmates = 0;
            foreach (NetworkedPlayerInfo p in GameData.Instance.AllPlayers)
            {
                if (p.Role.IsImpostor && !p.IsDead && !p.Disconnected)
                {
                    numAliveImpostors++;
                }
                if (!p.Role.IsImpostor && !p.Disconnected)
                {
                    numCrewmates++;
                }
            }
           
            int numCrewVotesNeeded = (int)System.Math.Ceiling(numCrewmates * DoomScrollVictoryManager.PercentCorrectHeadlinesNeeded);

            //
            //  ADD EDITED VOTING STRING HERE, AND DON'T LET THEM VOTE IMPOSTOR OUT UNTIL THEY GET ALL HEADLINE VOTES CORRECT
            //
            if (_exiledWasOverridden)
            {
                __instance.completeString = _retainedExileString;
            }
            if (!DoomScrollVictoryManager.IsHeadlineVoteSuccess)
            {
                if (OriginalExiledPlayer != null && OriginalExiledPlayer.Role.IsImpostor && numAliveImpostors < 1)
                {
                    __instance.completeString += $"\nHOWEVER, {OriginalExiledPlayer.PlayerName} was not ejected, because Crewmates cannot succeed until\n{numCrewVotesNeeded} crewmates vote correctly for all Headlines.";
                }
                else
                {
                    __instance.completeString += $"Crewmates cannot succeed until\n{numCrewVotesNeeded} crewmates vote correctly for all Headlines.";
                }
            }
            else
            {
                __instance.completeString += "\nCrewmates have voted correctly for all current Headlines,\nand can now win the game through voting or tasks.";
            }


            List<System.Tuple<int, string, string>> scoresByNumCorrect = new List<System.Tuple<int, string, string>>();

            foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers)
            {
                if (HeadlineDisplay.Instance.PlayerScores.ContainsKey(player.PlayerId) && !player.Disconnected)
                {
                    int currentScore = HeadlineDisplay.Instance.PlayerScores[player.PlayerId].Item1;
                    string headlineScore = HeadlineDisplay.Instance.CalculateScoreStrings(player.PlayerId);
                    scoresByNumCorrect.Add(new System.Tuple<int, string, string>(currentScore, player.PlayerName, headlineScore.Trim(' ', '\n', '\t', '[', ']')));
                }
            }


            DoomScroll._log.LogInfo("Things in scoresByNumCorrect: ");
            string votingResultsText = "<b>Headline Voting Scores:</b>\n";
            string scoreboardText = "<b>Rankings:</b>\n";
            foreach (System.Tuple<int, string, string> thing in scoresByNumCorrect)
            {
                DoomScroll._log.LogInfo(thing.Item3);
                votingResultsText += thing.Item2 + ": " + thing.Item3 + "\n";
                scoreboardText += thing.Item2 + "\n";
            }
           
            CustomText votingResults = new CustomText(__instance.Text.gameObject, "Voting Results CustomText", votingResultsText);
            votingResults.SetLocalPosition(new Vector3(-2f, -1.3f, -10f));
            votingResults.SetSize(1.5f);
            votingResults.SetColor(Color.yellow);

            CustomText scoreBoard = new CustomText(__instance.Text.gameObject, "Scoreboard CustomText", scoreboardText);
            scoreBoard.SetLocalPosition(new Vector3(2f, -1.3f, -10f));
            scoreBoard.SetSize(1.5f);
            scoreBoard.SetColor(Color.yellow);

            DoomScrollTasksManager.UpdateHeadlineSortingCompletion(DoomScrollVictoryManager.IsHeadlineVoteSuccess);

        }

        [HarmonyPrefix]
        [HarmonyPatch("WrapUp")]
        public static void PrefixWrapUp(ExileController __instance)
        {
            //Reset exile stuff
            if (OriginalExiledPlayer != null && !_exiledWasOverridden)
            {
                foreach (PlayerControl exiled in PlayerControl.AllPlayerControls)
                {
                    if (exiled.PlayerId == OriginalExiledPlayer.PlayerId)
                    {
                        DoomScroll._log.LogInfo("Calling DoomScroll exiled!!!");
                        PlayerControlPatch.DoomScrollExiled(exiled);
                    }
                }
            }

            if (_exiledWasOverridden)
            {
                __instance.ReEnableGameplay();
            }

            OriginalExiledPlayer = null;
            _exiledWasOverridden = false;
        }
    }
}
