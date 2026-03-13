using Godot;

public partial class ModEntry : Node
{
    public override void _Ready()
    {
        GD.Print("[NoDamageMod] Loaded! Player characters will not lose HP.");
    }
}
