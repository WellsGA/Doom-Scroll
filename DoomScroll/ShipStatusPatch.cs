using HarmonyLib;

namespace Doom_Scroll
{
    [HarmonyPatch(typeof(ShipStatus))]
    class ShipStatusPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void PostfixStart(ShipStatus __instance)
        {
            ScreenshotManager.Instance.ActivateCameraButton(true);
            DoomScroll._log.LogInfo("ShipStatusPatch.Start ---- CAMERA INIT");

            // list all tasks - for debug purposes
            /*DoomScroll._log.LogInfo("TASK INFO");            
            foreach (NormalPlayerTask task in __instance.CommonTasks)
            {
                DoomScroll._log.LogInfo("COMMON name: " + task.name + ", Index: " + task.Index +
                ", type: " + task.TaskType);
            }
            foreach (NormalPlayerTask task in __instance.LongTasks)
            {
                DoomScroll._log.LogInfo("LONG name: " + task.name + ", Index: " + task.Index +
                ", type: " + task.TaskType);
            }
            foreach (NormalPlayerTask task in __instance.NormalTasks)
            {
                DoomScroll._log.LogInfo("NORMAL name: " + task.name + ", Index: " + task.Index +
                ", type: " + task.TaskType);
            }*/
        }
    }
}
