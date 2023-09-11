﻿using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using Il2CppSystem;
using static Il2CppMono.Security.X509.X520;
using System.Linq;

namespace Doom_Scroll.Patches
{
    public enum CustomRPC : byte
    {
        SENDIMAGETOCHAT = 246,
        SENDENDORSEMENT = 247,
        SENDNEWSTOCHAT = 248,
        SENDPLAYERCANPOST = 249,
        SENDNEWS = 250,
        SENDIMAGEPIECE = 251,
        DEATHNOTE = 252,
        SENDASSIGNEDTASK = 253,
        SENDSWC = 254,
        SENDIMAGE = 255
    }

    [HarmonyPatch(typeof(PlayerControl))]
    public static class PlayerControlPatch
    {
        static int count = 0;

        private static Dictionary<string, DoomScrollImage> currentImagesAssembling = new Dictionary<string, DoomScrollImage>();

        public static void ResetImageDictionary()
        {
            currentImagesAssembling = new Dictionary<string, DoomScrollImage>();
        }


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
                    foreach (PlayerTask task in __instance.myTasks)
                    {
                        taskIds.Add(task.Id);
                    }
                    for (int i = 0; i < TaskAssigner.Instance.MaxAssignableTasks; i++)
                    {
                        int taskIndex = UnityEngine.Random.Range(0, taskIds.Count);
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
            // local player dead, has to update swc list for themself and the other players
            SecondaryWinConditionManager.UpdateSWCList(__instance.PlayerId, reason);
            SecondaryWinConditionManager.RPCDeathNote(__instance.PlayerId, reason);
        }

        [HarmonyPrefix]
        [HarmonyPatch("HandleRpc")]
        public static void PrefixHandleRpc([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader, PlayerControl __instance)
        {
            switch (callId)
            {
                case (byte)CustomRPC.SENDIMAGETOCHAT:
                    {
                        byte playerid = reader.ReadByte();
                        int imageid = reader.ReadInt32();
                        if (currentImagesAssembling[(string)$"{playerid}{imageid}"].CompileImage())
                        {
                            if (DestroyableSingleton<HudManager>.Instance && AmongUsClient.Instance.AmClient)
                            {
                                ChatControllerPatch.content = ChatContent.SCREENSHOT;
                                ChatControllerPatch.screenshot = currentImagesAssembling[(string)$"{playerid}{imageid}"].Image;
                                string chatText = PlayerControl.LocalPlayer.PlayerId.ToString() + "#" + ScreenshotManager.Instance.Screenshots.ToString();
                                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, chatText);
                                DoomScroll._log.LogMessage("Should have added image locally!");
                            }
                            return;
                        }
                        else
                        {
                            DoomScroll._log.LogMessage("TRIED TO SEND IMAGE IN CHAT, BUT IMAGE MISSING SECTION(S)");
                            DoomScroll._log.LogMessage($"MISSING SECTIONS: {currentImagesAssembling[(string)$"{playerid}{imageid}"].GetMissingLines()}");

                            DoomScroll._log.LogMessage($"ENDING RPC FOR IMAGE WITH pID {playerid} AND imgID {imageid}\n");
                            // LATER HERE WE CAN FIX THIS
                        }
                        return;
                    }
                case (byte)CustomRPC.SENDENDORSEMENT:
                    {
                        NewsItem news = NewsFeedManager.Instance.GetNewsByID(reader.ReadInt32());
                        if (news != null)
                        {
                            if (reader.ReadBoolean())
                            {
                                news.TotalEndorsement = reader.ReadBoolean() ? news.TotalEndorsement + 1 : news.TotalEndorsement - 1;
                                news.EndorseLable.SetText(news.TotalEndorsement.ToString());
                            }
                            else
                            {
                                news.TotalDenouncement = reader.ReadBoolean() ? news.TotalDenouncement + 1 : news.TotalDenouncement - 1; ;
                                news.DenounceLable.SetText(news.TotalDenouncement.ToString());
                            }
                        }
                        else
                        {
                            DoomScroll._log.LogInfo("=========== Couldn't find news!!!! ==============");
                        }
                        return;
                    }
                case (byte)CustomRPC.SENDNEWSTOCHAT:
                    {
                        if (DestroyableSingleton<HudManager>.Instance)
                        {
                            NewsItem news = NewsFeedManager.Instance.GetNewsByID(reader.ReadInt32());
                            if (news != null)
                            {
                                ChatControllerPatch.content = ChatContent.TEXT;
                                string chatText = news.NewsToChatText();
                                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, chatText);
                            }
                        }
                        return;
                    }
                case (byte)CustomRPC.SENDPLAYERCANPOST:
                    {
                        if (reader.ReadByte() == PlayerControl.LocalPlayer.PlayerId)
                        {
                            NewsFeedManager.Instance.CanPostNews(true);
                        }
                        DoomScroll._log.LogInfo("==== CAN POST: " + reader.ReadString());
                        return;
                    }
                case (byte)CustomRPC.SENDNEWS:
                    {
                        NewsFeedManager.Instance.AddNews(new NewsItem(reader.ReadInt32(), reader.ReadByte(), reader.ReadString(), reader.ReadBoolean(), reader.ReadString()));
                        return;
                    }
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
                        SecondaryWinConditionManager.AddToPlayerSWCList(new SecondaryWinCondition(reader.ReadByte(), (Goal)reader.ReadByte(), reader.ReadByte()));
                        return;
                    }
                case (byte)CustomRPC.SENDIMAGE:
                    {
                        DoomScroll._log.LogMessage("--------------\nReceiving RPC image\n--------------");
                        int numMessages = reader.ReadInt32();
                        byte pID = reader.ReadByte();
                        int imgID = reader.ReadByte();
                        DoomScrollImage currentImage = new DoomScrollImage(numMessages, pID, imgID);
                        string currentImageKey = $"{pID}{imgID}";
                        currentImagesAssembling.Add((string)$"{pID}{imgID}", currentImage);
                        DoomScroll._log.LogMessage($"Received image info, inserted to currentImagesAssembling as DoomScrollImage({numMessages}, {pID}, {imgID})");
                        DoomScroll._log.LogMessage($"Image stored at key {currentImageKey}. Current Dictionary items: ");
                        foreach (string key in currentImagesAssembling.Keys)
                        {
                            DoomScroll._log.LogMessage($"- item key: {key}, item info: {currentImagesAssembling[key].ToString()}");
                        }
                        DoomScroll._log.LogMessage($"Current Dictionary over.");
                        return;
                    }
                case (byte)CustomRPC.SENDIMAGEPIECE:
                    {
                        byte playerid = reader.ReadByte();
                        int imageid = reader.ReadByte();
                        int sectionIndex = reader.ReadInt32();
                        byte[] imageBytesSection = reader.ReadBytesAndSize();
                        DoomScroll._log.LogMessage($"Trying to access at key \'{playerid}{imageid}\'. Current Dictionary: {currentImagesAssembling}");
                        currentImagesAssembling[(string)$"{playerid}{imageid}"].InsertByteChunk(sectionIndex, imageBytesSection);
                        DoomScroll._log.LogMessage($"Received image chunk #{sectionIndex}, inserted to current DoomScrollImage");

                        if (sectionIndex == currentImagesAssembling[(string)$"{playerid}{imageid}"].Image.Count() && currentImagesAssembling[(string)$"{playerid}{imageid}"].CompileImage())
                        {
                            DoomScroll._log.LogMessage("IMAGE COMPLETELY RECEIVED.");
                            return;
                        }
                        else
                        {
                            DoomScroll._log.LogMessage("TRIED TO RECEIVE AND COMPILE IMAGE, BUT IMAGE MISSING SECTION(S)");
                            DoomScroll._log.LogMessage($"MISSING SECTIONS: {currentImagesAssembling[(string)$"{playerid}{imageid}"].GetMissingLines()}");

                            DoomScroll._log.LogMessage($"ENDING RPC FOR IMAGE WITH pID {playerid} AND imgID {imageid}\n");
                            // LATER HERE WE CAN FIX THIS
                        }
                        break;
                    }

            }
        }
    }
}