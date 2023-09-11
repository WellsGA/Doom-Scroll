using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Doom_Scroll.Common;

namespace Doom_Scroll;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class DoomScroll : BasePlugin
{
    public static ManualLogSource _log;
    public override void Load()
    {
        // Plugin startup logic
        _log = Log;
        _log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        // Add mod stamp
        SceneManager.add_sceneLoaded((UnityAction<Scene, LoadSceneMode>)((scene, _) =>
        {
            if (scene.name == "MainMenu")
            {
                ModManager.Instance.ShowModStamp();
            }
        }));
    }
}
