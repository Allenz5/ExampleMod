using Godot;
using MegaCrit.Sts2.Core.Entities.Players;

public partial class PlayerInfoRowNode : VBoxContainer
{
    private Player _player;
    private Player? _localPlayer;
    private GoldTransferService? _goldTransferService;
    private Label _nameLabel;
    private Label _damageDealtLabel;
    private Label _damageTakenLabel;
    private Label _goldLabel;
    private SpinBox? _amountInput;
    private Button? _sendButton;

    public void Initialize(Player player, Player? localPlayer, GoldTransferService? goldTransferService = null)
    {
        _player = player;
        _localPlayer = localPlayer;
        _goldTransferService = goldTransferService;
        BuildUI();
        UpdateDisplay();
    }

    private void BuildUI()
    {
        AddThemeConstantOverride("separation", 2);
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ShrinkBegin;

        _nameLabel = new Label();
        _nameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.6f));
        AddChild(_nameLabel);

        _damageDealtLabel = new Label();
        AddChild(_damageDealtLabel);

        _damageTakenLabel = new Label();
        AddChild(_damageTakenLabel);

        _goldLabel = new Label();
        AddChild(_goldLabel);

        if (_localPlayer != null && !ReferenceEquals(_localPlayer, _player))
        {
            var sendRow = new HBoxContainer();
            sendRow.AddThemeConstantOverride("separation", 4);

            _amountInput = new SpinBox();
            _amountInput.MinValue = 0;
            _amountInput.MaxValue = 9999;
            _amountInput.Value = 1;
            _amountInput.Step = 1;
            _amountInput.CustomMinimumSize = new Vector2(80, 0);
            sendRow.AddChild(_amountInput);

            _sendButton = new Button();
            _sendButton.Text = "Send";
            _sendButton.Pressed += OnSendGold;
            sendRow.AddChild(_sendButton);

            AddChild(sendRow);
        }

        var sep = new HSeparator();
        sep.AddThemeConstantOverride("separation", 4);
        AddChild(sep);
    }

    public void UpdateDisplay()
    {
        if (_player == null) return;

        _nameLabel.Text = _player.Character?.Id.Entry ?? "Unknown";
        _damageDealtLabel.Text = $"Dmg Dealt: {DamageTracker.GetDealt(_player)}";
        _damageTakenLabel.Text = $"Dmg Taken: {DamageTracker.GetTaken(_player)}";
        _goldLabel.Text = $"Gold: {_player.Gold}";

        if (_amountInput != null && _localPlayer != null)
        {
            _amountInput.MaxValue = _localPlayer.Gold;
        }
    }

    private void OnSendGold()
    {
        if (_localPlayer == null || _player == null || _amountInput == null) return;
        int amount = (int)_amountInput.Value;
        if (amount <= 0 || _localPlayer.Gold < amount) return;

        if (_goldTransferService != null)
            _goldTransferService.SendGold(_localPlayer, _player, amount);
        else
        {
            _localPlayer.Gold -= amount;
            _player.Gold += amount;
        }
    }
}
