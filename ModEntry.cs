using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

[ModInitializer("Initialize")]
public class ModEntry
{
    public static void Initialize()
    {
        var harmony = new Harmony("com.example.nodamagemod");
        harmony.PatchAll();
        GD.Print("[NoDamageMod] Loaded! Player characters will not lose HP.");

    }
}
