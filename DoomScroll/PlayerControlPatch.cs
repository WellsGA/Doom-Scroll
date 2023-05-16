using HarmonyLib;
using Hazel;
using UnityEngine;
using System.Collections.Generic;
using Il2CppSystem;

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
        private static Dictionary<String, DoomScrollImage> currentImagesAssembling = new Dictionary<String, DoomScrollImage>();

        [HarmonyPostfix]
        [HarmonyPatch("SetTasks")]
        public static void PostfixSetTasks(PlayerControl __instance)
        {
            
            if (__instance.myTasks != null && __instance.myTasks.Count > 0)
            {
                if (AmongUsClient.Instance.AmClient)
                {
                    TaskAssigner.Instance.CreateTaskAssignerPanel(); // players are ready, create the panel
                }  
                // check for impostor
                if (__instance.AmOwner && PlayerControl.LocalPlayer.Data.Role.Role != AmongUs.GameOptions.RoleTypes.Impostor)
                {
                    List<uint> taskIds = new List<uint>();
                    List<uint> assignableTasks = new List<uint>();
                    foreach(PlayerTask task in __instance.myTasks)
                    {
                        taskIds.Add(task.Id); 
                    }
                    for (int i = 0; i < TaskAssigner.Instance.MaxAssignableTasks; i++)
                    {
                        int taskIndex = UnityEngine.Random.Range(0, taskIds.Count - 1);
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
                        DoomScroll._log.LogMessage("--------------\nReceiving RPC image\n--------------");
                        int numMessages = reader.ReadInt32();
                        byte pID = reader.ReadByte();
                        byte imgID = reader.ReadByte();
                        DoomScrollImage currentImage = new DoomScrollImage(numMessages, pID, imgID);
                        String currentImageKey = $"{pID}{imgID}";
                        currentImagesAssembling.Add($"{pID}{imgID}", currentImage);
                        DoomScroll._log.LogMessage($"Received image info, inserted to currentImagesAssembling as DoomScrollImage({numMessages}, {pID}, {imgID})");
                        for (int i = 0; i < (int)numMessages; i++)
                        {
                            byte playerid = reader.ReadByte();
                            byte imageid = reader.ReadByte();
                            int sectionIndex = reader.ReadInt32();
                            byte[] imageBytesSection = reader.ReadBytesAndSize();
                            currentImagesAssembling[$"{playerid}{imageid}"].InsertByteChunk(sectionIndex, imageBytesSection);
                            DoomScroll._log.LogMessage($"Received image chunk #{i} out of {numMessages}, inserted to current DoomScrollImage");
                            
                        }
                        if (reader.ReadString() == "END OF MESSAGE")
                        {
                            DoomScroll._log.LogMessage("Image receiving complete!");
                        }
                        if (currentImagesAssembling[currentImageKey].CompileImage())
                        {
                            if (DestroyableSingleton<HudManager>.Instance)
                            {
                                ChatControllerPatch.screenshot = currentImagesAssembling[currentImageKey].Image;
                                string chatMessage = __instance.PlayerId + "#" + ScreenshotManager.Instance.Screenshots;
                                HudManager.Instance.Chat.AddChat(__instance, chatMessage);
                                return;
                            }
                            break;
                        }
                        else
                        {
                            DoomScroll._log.LogMessage("IMAGE MISSING SECTION(S)");
                            DoomScroll._log.LogMessage($"MISSING SECTIONS: {currentImagesAssembling[currentImageKey].GetMissingLines()}");

                            DoomScroll._log.LogMessage($"ENDING RPC FOR IMAGE WITH pID {pID} AND imgID {imgID}\n");
                            // LATER HERE WE CAN FIX THIS
                        }
                        break;

                    }
            }
        }
    }
}
