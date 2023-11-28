using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using Doom_Scroll.Common;
using System.Text.RegularExpressions;
using Assets.CoreScripts;
using System;


namespace Doom_Scroll.Patches
{
    public enum CustomRPC : byte
    {
        IMAGESENDINGCOMPLETE = 240,
        CANSENDIMAGE = 241,
        SENDSABOTAGECONTRIBUTION = 242,
        SENDTRUSTSELECTION = 243,
        SENDTEXTCHAT = 244,
        SENDVOTE = 245,
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
                        int taskIndex = UnityEngine.Random.Range(0, taskIds.Count);
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
                case (byte)CustomRPC.SENDVOTE:
                    byte playerId = reader.ReadByte();
                    string vote = reader.ReadString() + " has voted for " + reader.ReadString();
                    HeadlineCreator.UpdatePlayerVote(playerId, vote);
                    if (AmongUsClient.Instance.AmHost)
                    {
                        GameLogger.Write(GameLogger.GetTime() + " - " + vote);
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
                        ScreenshotManager.Instance.AddImageToChat(reader.ReadString());
                        break;
                    }
                case (byte)CustomRPC.CANSENDIMAGE:  // host notifies the next in line
                    {
                        byte id = reader.ReadByte();
                        string itemId = reader.ReadString();
                        if(id == PlayerControl.LocalPlayer.PlayerId)
                        {
                            DoomScroll._log.LogInfo("Local player can send image: " + itemId);
                            // send
                            ScreenshotManager.Instance.SendImageInPieces(itemId);
                        }
                        else
                        {
                            DoomScroll._log.LogInfo("Player " + id + " can send image: " + itemId);
                            // create a dictionary entry with the id to prepare for receiving
                        }
                        break;
                    }
                case (byte)CustomRPC.IMAGESENDINGCOMPLETE:
                    {
                        string itemID = reader.ReadString();
                        if (PlayerControl.LocalPlayer.AmOwner)
                        {
                            ImageQueuer.FinishedSharing(__instance.PlayerId, itemID);
                        }
                        // check if image is complete and add it to the dictionary of images
                        break;
                    }
                case (byte)CustomRPC.SENDIMAGEPIECE:
                    {
                        // receive image and add piece to the byte array 
                        break;
                    }
               
            }
        }
    }
}
