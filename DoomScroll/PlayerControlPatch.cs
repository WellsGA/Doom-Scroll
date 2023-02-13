using HarmonyLib;
using Hazel;
using System;
using System.Linq;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(PlayerControl))]
    public static class PlayerControlPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControl._CoSetTasks_d__113), nameof(PlayerControl._CoSetTasks_d__113.MoveNext))]
        public static void PostfixCoSetTasks(PlayerControl._CoSetTasks_d__113 __instance)
        {
            // check for impostor
            if (PlayerControl.LocalPlayer.AmOwner && PlayerControl.LocalPlayer.Data.Role.Role != AmongUs.GameOptions.RoleTypes.Impostor)
            {
                TaskAssigner.Instance.SelectRandomTasks(__instance.tasks);
            }
            else
            {
                DoomScroll._log.LogInfo("You cannot assign tasks ...");
            }
           
        }

        [HarmonyPostfix]
        [HarmonyPatch("CompleteTask")]
        public static void PostfixCompleteTasks(PlayerControl __instance, uint idx)
        {
            foreach (GameData.TaskInfo ti in __instance.Data.Tasks)
            {
                if (ti.Id == idx)
                {
                    if (TaskAssigner.Instance.AssignableTasksIDs.Contains(ti.TypeId))
                    {
                        TaskAssigner.Instance.AssignPlayerToTask(ti.TypeId);
                        continue;
                    }
                }
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
                        TaskAssigner.Instance.AddToAssignedTasks(reader.ReadByte(), reader.ReadByte());
                        return;
                    }
                case (byte)CustomRPC.SENDSWC:
                    {
                        DoomScroll._log.LogInfo("HandleRpc for swc");
                       //string SWCstring = ;
                        SecondaryWinCondition.addToPlayerSWCList(reader.ReadString());
                        ///DoomScroll._log.LogInfo("SWC text added to list: " + SWCstring);
                        return;
                    }
                case (byte)CustomRPC.SENDIMAGE:
                    {
                        DoomScroll._log.LogInfo("reader buffer: " + reader.Buffer);
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
