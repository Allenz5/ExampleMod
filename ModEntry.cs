using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

[ModInitializer("Initialize")]
public class ModEntry
{
    public static void Initialize()
    {
        var harmony = new Harmony("com.example.examplemod");
        harmony.PatchAll();
        GD.Print("[ExampleMod] Loaded!");

    }
}
