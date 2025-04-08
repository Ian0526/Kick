using BepInEx;
using HarmonyLib;

[BepInPlugin("com.ovchinikov.kick", "Kick Players Plugin", "1.0.0")]
public class KickPlugin : BaseUnityPlugin
{

    public static bool componentAddedOnce = false;

    private void Awake()
    {
        Logger.LogInfo("Kick Players Plugin Initialized!");
        var harmony = new Harmony("com.ovchinikov.kick");
        harmony.PatchAll();
    }
}
