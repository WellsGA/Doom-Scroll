using HarmonyLib;
using Hazel;
using Il2CppSystem.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using static GameData;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(PlayerControl))]
    public static class PlayerControlPatch
    {
        static int i = 0;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControl._CoSetTasks_d__113), nameof(PlayerControl._CoSetTasks_d__113.MoveNext))]
        public static void PostfixCoSetTasks(PlayerControl._CoSetTasks_d__113 __instance)
        {
            List<PlayerTask> tasks = __instance.__4__this.myTasks;
            // check for impostor
            if (tasks != null && tasks.Count > 0)
            {
                if (__instance.__4__this.AmOwner && PlayerControl.LocalPlayer.Data.Role.Role != AmongUs.GameOptions.RoleTypes.Impostor)
                {
                    TaskAssigner.Instance.SelectRandomTasks(tasks);
                    DoomScroll._log.LogInfo("SelectRandomTasks Function called " + i++ + " times");
                }
                else
                {
                    DoomScroll._log.LogInfo("You cannot assign tasks ...");
                }
            }

        }

        /*[HarmonyPostfix]
        [HarmonyPatch("CoSetTasks")]
        public static void PostfixCoSetTasks(PlayerControl __instance)
        {

            // check for impostor
            if (__instance.myTasks != null)
            {
                if (PlayerControl.LocalPlayer.AmOwner && PlayerControl.LocalPlayer.Data.Role.Role != AmongUs.GameOptions.RoleTypes.Impostor)
                {
                    TaskAssigner.Instance.SelectRandomTasks(__instance.);
                    DoomScroll._log.LogInfo("SelectRandomTasks Function called " + i++ + " times");
                }
                else
                {
                    DoomScroll._log.LogInfo("You cannot assign tasks ...");
                }
            }

        }*/

        [HarmonyPostfix]
        [HarmonyPatch("CompleteTask")]
        public static void PostfixCompleteTasks(PlayerControl __instance, uint idx)
        {
           TaskInfo taskInfo = __instance.Data.FindTaskById(idx);
            if (taskInfo == null)
            {
                DoomScroll._log.LogInfo("Task not found: " + idx.ToString());
                return;
            }
            if (taskInfo.Complete && TaskAssigner.Instance.AssignableTasksIDs.Contains(idx))
            {
                TaskAssigner.Instance.AssignPlayerToTask(idx);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("HandleRpc")]
        public static void PrefixHandleRpc([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader, PlayerControl __instance)
        {
            switch (callId)
            {
                case (byte)CustomRPC.SENDASSIGNEDTASK:
                    {
                        TaskAssigner.Instance.AddToAssignedTasks(__instance, reader.ReadByte(), reader.ReadUInt32());
                        return;
                    }
                case (byte)CustomRPC.SENDSWC:
                    {
                        SecondaryWinCondition.addToPlayerSWCList(reader.ReadString());
                        return;
                    }
                case (byte)CustomRPC.SENDIMAGE:
                    {
                        byte[] imageBytes = reader.ReadBytesAndSize();
                        DoomScroll._log.LogInfo("Image received! Size:" + imageBytes.Length);
                        if (DestroyableSingleton<HudManager>.Instance)
                        {
                            RPCManager.AddChat(__instance, imageBytes);
                            return;
                        }
                        break;
                    }
            }
        }
    }
}
