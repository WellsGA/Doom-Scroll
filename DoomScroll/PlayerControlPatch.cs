using HarmonyLib;
using Hazel;
using UnityEngine;
using System.Collections.Generic;

namespace Doom_Scroll
{
    public enum CustomRPC : byte
    {
        DEATHNOTE = 252,
        SENDASSIGNEDTASK = 253,
        SENDSWC = 254,
        SENDIMAGE = 255
    }

    [HarmonyPatch(typeof(PlayerControl))]
    public static class PlayerControlPatch
    {
        static int count = 0;

        [HarmonyPostfix]
        [HarmonyPatch("SetTasks")]
        public static void PostfixSetTasks(PlayerControl __instance)
        {
            
            if (__instance.myTasks != null && __instance.myTasks.Count > 0)
            {
                TaskAssigner.Instance.CreateTaskAssignerPanel(); // players are ready, create the panel
                // check for impostor
                if (__instance.AmOwner && PlayerControl.LocalPlayer.Data.Role.Role != AmongUs.GameOptions.RoleTypes.Impostor)
                {
                    List<uint> taskIds = new List<uint>();
                    List<uint> assignableTasks = new List<uint>();
                    foreach(PlayerTask task in __instance.myTasks)
                    {
                        taskIds.Add(task.Id);
                    }
                    // more taskIds to do than to assign
                    for (int i = 0; i < TaskAssigner.Instance.MaxAssignableTasks; i++)
                    {
                        int taskIndex = Random.Range(0, taskIds.Count - 1);
                        assignableTasks.Add(taskIds[taskIndex]);
                        taskIds.RemoveAt(taskIndex);
                    }
                        TaskAssigner.Instance.SetAssignableTasks(assignableTasks);
                        DoomScroll._log.LogInfo("original " + __instance.myTasks.Count + " copy: " + assignableTasks.Count);
                }                    
                DoomScroll._log.LogInfo("SelectRandomTasks Function called " + ++count + " times");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Die")]
        public static void PostfixDie(PlayerControl __instance, DeathReason reason)
        {
            // local player dead, has to update swc list
            SecondaryWinConditionManager.UpdateSWCList(__instance.PlayerId, reason);
        }

        [HarmonyPrefix]
        [HarmonyPatch("HandleRpc")]
        public static void PrefixHandleRpc([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader, PlayerControl __instance)
        {
            switch (callId)
            {
                case (byte)CustomRPC.DEATHNOTE:
                    {
                        // other player dead, has to update swc list
                        SecondaryWinConditionManager.UpdateSWCList(reader.ReadByte(), (DeathReason)reader.ReadByte());
                        return;
                    }
                case (byte)CustomRPC.SENDASSIGNEDTASK:
                    {
                        TaskAssigner.Instance.AddToAssignedTasks(__instance, reader.ReadByte(), reader.ReadUInt32());
                        return;
                    }
                case (byte)CustomRPC.SENDSWC:
                    {
                        SecondaryWinConditionManager.addToPlayerSWCList(new SecondaryWinCondition(reader.ReadByte(), (Goal)reader.ReadByte(), reader.ReadByte()));
                        return;
                    }
                case (byte)CustomRPC.SENDIMAGE:
                    {
                        byte[] imageBytes = reader.ReadBytesAndSize();
                        // DoomScroll._log.LogInfo("Image received! Size:" + imageBytes.Length); // debug
                        if (DestroyableSingleton<HudManager>.Instance)
                        {
                            ChatControllerPatch.screenshot = imageBytes;
                            string chatMessage = __instance.PlayerId + "#" + ScreenshotManager.Instance.Screenshots;
                            HudManager.Instance.Chat.AddChat(__instance, chatMessage);
                            return;
                        }
                        break;
                    }
            }
        }
    }
}
