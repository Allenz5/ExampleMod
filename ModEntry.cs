using Godot;
using HarmonyLib;

public partial class ModEntry : Node
{
    public override void _Ready()
    {
        var harmony = new Harmony("com.example.nodamagemod");
        harmony.PatchAll();
        GD.Print("[NoDamageMod] Loaded! Player characters will not lose HP.");

        var cardInjector = new DevCardInjector();
        cardInjector.Name = "DevCardInjector";
        AddChild(cardInjector);
        GD.Print("[DevCardInjector] Card injection UI loaded.");
    }
}
