using Godot;
using MegaCrit.Sts2.Core.Entities.Players;

public partial class PlayerInfoRowNode : Control
{
    private Player _player;
    private Label _nameLabel;
    private Label _hpLabel;
    private Label _goldLabel;

    public void Initialize(Player player)
    {
        _player = player;
        BuildUI();
        UpdateDisplay();
    }

    private void BuildUI()
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 2);
        AddChild(vbox);

        _nameLabel = new Label();
        _nameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.6f));
        vbox.AddChild(_nameLabel);

        _hpLabel = new Label();
        vbox.AddChild(_hpLabel);

        _goldLabel = new Label();
        vbox.AddChild(_goldLabel);

        var sep = new HSeparator();
        sep.AddThemeConstantOverride("separation", 4);
        vbox.AddChild(sep);
    }

    public void UpdateDisplay()
    {
        if (_player == null) return;

        _nameLabel.Text = _player.Character?.Id.Entry ?? "Unknown";

        if (_player.Creature != null)
        {
            _hpLabel.Text = $"HP: {_player.Creature.CurrentHp} / {_player.Creature.MaxHp}";
        }
        else
        {
            _hpLabel.Text = "HP: --";
        }

        _goldLabel.Text = $"Gold: {_player.Gold}";
    }
}
