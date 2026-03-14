using Godot;
using HarmonyLib;

public partial class ModEntry : Node
{
    public override void _Ready()
    {
        var harmony = new Harmony("com.mod.multiplayersidebar");
        harmony.PatchAll();
        GD.Print("[MultiplayerSidebarMod] Loaded! Sidebar will appear in all runs.");
    }
}
