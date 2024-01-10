using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using Doom_Scroll.Common;
using System.Text.RegularExpressions;
using UnityEngine;


namespace Doom_Scroll.Patches
{
    public enum CustomRPC : byte
    {
        SETPLAYERFORSCREENSHOT = 236,
        COMPLETEDUMMYTASK = 237,
        SETDUMMYTASKS = 238,
        ENQUEUEIMAGE = 239,
        IMAGESENDINGCOMPLETE = 240,
        CANSENDIMAGE = 241,
        SENDSABOTAGECONTRIBUTION = 242,
        SENDTRUSTSELECTION = 243,
        SENDTEXTCHAT = 244,
        // SENDVOTE = 245,
        ADDIMAGETOCHAT = 246,
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
        // static int count = 0; // debug

        [HarmonyPrefix]
        [HarmonyPatch("RpcSendChat")]
        public static bool PrefixRpcSendChat(ref string chatText)
        {
            ChatControllerPatch.content = ChatContent.DEFAULT;
            chatText = ChatControllerPatch.GetChatID() + Regex.Replace(chatText, "<.*?>", string.Empty);
           
            return true; //  skip origianl method
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
                        int taskIndex = Random.Range(0, taskIds.Count);
                        assignableTasks.Add(taskIds[taskIndex]);
                        taskIds.RemoveAt(taskIndex);
                    }
                    TaskAssigner.Instance.SetAssignableTasks(assignableTasks);
                    // DoomScroll._log.LogInfo("original " + __instance.myTasks.Count + " copy: " + assignableTasks.Count);
                }
               // DoomScroll._log.LogInfo("SelectRandomTasks Function called " + ++count + " times");
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
        [HarmonyPatch("Exiled")]
        public static bool PrefixExiled()
        {
            DoomScroll._log.LogInfo("Trying to stop Among Us EXILED method!!!");
            return false;
        }

        public static void DoomScrollExiled(PlayerControl player)
        {
            player.Die(DeathReason.Exile, true);
            if (PlayerControl.LocalPlayer.PlayerId == player.PlayerId)
            {
                DoomScroll._log.LogInfo("Self was exiled!");
                StatsManager.Instance.IncrementStat(StringNames.StatsTimesEjected);
            }
        }

            [HarmonyPrefix]
        [HarmonyPatch("HandleRpc")]
        public static void PrefixHandleRpc([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader, PlayerControl __instance)
        {
            switch (callId)
            {
                case (byte)CustomRPC.SENDSABOTAGECONTRIBUTION:
                    if (AmongUsClient.Instance.AmHost)
                    {
                        bool helpedFix = reader.ReadBoolean();
                        if (helpedFix)
                        {
                            HeadlineCreator.AddToFixSabotage(__instance.PlayerId);
                        }
                        else
                        {
                            HeadlineCreator.AddToStartedSabotage(__instance.PlayerId);
                        }
                    }
                    return;
                case (byte)CustomRPC.SENDTRUSTSELECTION:
                    Headline news = HeadlineDisplay.Instance.GetNewsByID(reader.ReadInt32());
                    if (news != null)
                    {
                        if (reader.ReadBoolean())
                        {
                            news.PlayersTrustSelections[__instance.PlayerId] = reader.ReadBoolean();
                        }
                        else
                        {
                            news.PlayersTrustSelections.Remove(__instance.PlayerId);
                        }
                    }
                    break;
                case (byte)CustomRPC.SENDTEXTCHAT:
                    if (DestroyableSingleton<HudManager>.Instance)
                    {
                        ChatControllerPatch.content = ChatContent.TEXT;
                        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, reader.ReadString());
                    }
                    return;               
                case (byte)CustomRPC.SENDENDORSEMENT:
                    {
                        HeadlineDisplay.Instance.UpdateEndorsementList(reader.ReadString(), reader.ReadBoolean(), reader.ReadBoolean());
                        break;
                    }
                case (byte)CustomRPC.SENDNEWSTOCHAT:
                    {
                        if (DestroyableSingleton<HudManager>.Instance)
                        {
                            string id = reader.ReadString();
                            Headline headline = HeadlineDisplay.Instance.GetNewsByID(reader.ReadInt32());
                            if (headline != null)
                            {
                                ChatControllerPatch.content = ChatContent.HEADLINE;
                                string chatText = id + headline.NewsToChatText();
                                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, chatText);
                            }
                        }
                        return;
                    }
                case (byte)CustomRPC.SENDPLAYERCANPOST:
                    {
                        if (reader.ReadByte() == PlayerControl.LocalPlayer.PlayerId)
                        {
                            HeadlineManager.Instance.CanPostNews(true);
                        }
                        DoomScroll._log.LogInfo("==== CAN POST: " + reader.ReadString());
                        return;
                    }
                case (byte)CustomRPC.SENDNEWS:
                    {
                        int headlineID = reader.ReadInt32();
                        byte authorID = reader.ReadByte();
                        if (HeadlineManager.Instance != null)
                        {
                            if (HeadlineManager.Instance.numTimesPlayersPosted.ContainsKey(authorID))
                            {
                                HeadlineManager.Instance.numTimesPlayersPosted[authorID] += 1;
                            }
                            else
                            {
                                HeadlineManager.Instance.numTimesPlayersPosted[authorID] = 1;
                            }
                        }
                        HeadlineDisplay.Instance.AddNews(new Headline(headlineID, authorID, reader.ReadString(), reader.ReadBoolean(), reader.ReadString()));
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
                case (byte)CustomRPC.ADDIMAGETOCHAT: // add image that has been sent over earlier
                    {
                        ScreenshotManager.Instance.AddImageToChat(reader.ReadInt32());
                        break;
                    }
                case (byte)CustomRPC.CANSENDIMAGE:  // host notifies the next in line
                    {
                        byte id = reader.ReadByte();
                        int itemId = reader.ReadInt32();
                        if(id == PlayerControl.LocalPlayer.PlayerId)
                        {
                            DoomScroll._log.LogInfo("2) You can send image (LP): " + itemId);
                            ScreenshotManager.Instance.SendImageInPieces(itemId);
                        }
                        else
                        {   // create a dictionary entry with the id to prepare for receiving
                            DoomScroll._log.LogInfo("2) Player " + id + " will send an image: " + itemId);
                        }
                        break;
                    }
                case (byte)CustomRPC.IMAGESENDINGCOMPLETE: // sender notifies players and host that the sending is complete
                    {
                        int itemID = reader.ReadInt32();
                        if (AmongUsClient.Instance.AmHost)
                        {
                            ImageQueuer.FinishedSharing(__instance.PlayerId, itemID);
                        }
                        DoomScroll._log.LogInfo("4) Image sending complete" + itemID);
                        break;
                    }
                case (byte)CustomRPC.SENDIMAGEPIECE:  // receive image and add piece to the byte array 
                    {
                        int id = reader.ReadInt32();
                        byte[] nextPart = reader.ReadBytesAndSize();
                        byte[] firstPart = ScreenshotManager.Instance.GetScreenshotById(id);
                        if(firstPart == null)
                        {
                            ScreenshotManager.Instance.AddImage(id, nextPart); // add the first part to the dictionary
                        }
                        else
                        {
                            byte[] combinedPart = new byte[firstPart.Length + nextPart.Length];
                            firstPart.CopyTo(combinedPart, 0);
                            nextPart.CopyTo(combinedPart, firstPart.Length);
                            ScreenshotManager.Instance.AddImage(id, combinedPart); // upserts item in the dictionary
                        }
                        break;
                    } 
                case (byte)CustomRPC.ENQUEUEIMAGE:  // send the image id to the host to queue
                    {
                        if (AmongUsClient.Instance.AmHost)
                        {
                            ImageQueuer.AddToQueue(__instance.PlayerId, reader.ReadInt32());
                        }
                        else
                        {
                            DoomScroll._log.LogInfo("I'm not the host....");
                        }
                        break;
                    }
                case (byte)CustomRPC.SETDUMMYTASKS:
                    {
                        foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers)
                        {
                            GameDataPatch.AddDummyTasksToThisList(playerInfo);
                        }
                        break;
                    }
                case (byte)CustomRPC.COMPLETEDUMMYTASK:
                    {
                        if (reader.ReadByte() == GameDataPatch.headlineTaskID)
                        {
                            GameDataPatch.UpdateBlankHeadlineTaskCompletion(reader.ReadByte());
                        }
                        break;
                    }
                case (byte)CustomRPC.SETPLAYERFORSCREENSHOT:
                    {
                        ScreenshotManager.Instance.PlayerCanScreenshot(reader.ReadByte());
                        break;
                    }
            }
        }
    }
}
