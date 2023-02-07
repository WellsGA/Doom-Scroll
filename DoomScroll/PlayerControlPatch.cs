using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(PlayerControl))]
    public static class PlayerControlPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetTasks")]
        public static void PostfixSetTasks([HarmonyArgument(0)] List<GameData.TaskInfo> tasks)
        {
            byte[] taskIds = new byte[tasks.Count];
            for(int i=0; i < tasks.Count; i++)
            {
                taskIds[i] = tasks[i].TypeId;
            }
            TaskAssigner.Instance.SelectRandomTasks(taskIds);
            DoomScroll._log.LogInfo("SetTasks in PlayerControl called");
        }

        [HarmonyPostfix]
        [HarmonyPatch("CompleteTask")]
        public static void PostfixCompleteTasks(PlayerControl __instance, uint idx)
        {
            foreach(GameData.TaskInfo ti in __instance.Data.Tasks)
            {
                if(ti.Id == idx)
                {
                    if (TaskAssigner.Instance.AssignableTasksIDs.Contains(ti.TypeId))
                    {
                        TaskAssigner.Instance.AssignPlayerToTask(ti.TypeId);
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
                case 254:
                    {
                        string SWCstring = reader.ReadString();
                        DoomScroll._log.LogInfo("HandleRpc 254- Text received!: " + SWCstring);
                        SecondaryWinCondition.addToPlayerSWCList(SWCstring);
                        DoomScroll._log.LogInfo("SWC text added to list: " + SWCstring);
                        return;
                    }
                case 255:
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
