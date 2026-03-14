using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Runs;
using System.Collections.Generic;

public partial class MultiplayerSidebarNode : Control
{
    private const float SidebarWidth = 220f;
    private const float ToggleButtonWidth = 28f;
    private const float AnimDuration = 0.25f;

    private Panel _panel;
    private VBoxContainer _playerListContainer;
    private Button _toggleButton;
    private bool _collapsed = false;
    private Tween? _tween;
    private readonly List<PlayerInfoRowNode> _rows = new();
    private GoldTransferService? _goldTransferService;

    public void Initialize(IRunState runState)
    {
        BuildUI();

        var localPlayer = LocalContext.GetMe(runState.Players);
        _goldTransferService = new GoldTransferService(runState);

        foreach (var player in runState.Players)
        {
            var row = new PlayerInfoRowNode();
            _playerListContainer.AddChild(row);
            row.Initialize(player, localPlayer, _goldTransferService);
            _rows.Add(row);
        }
    }

    public override void _ExitTree()
    {
        _goldTransferService?.Dispose();
        _goldTransferService = null;
    }

    private void BuildUI()
    {
        // Anchor to the top-right corner of the parent
        AnchorLeft = 1.0f;
        AnchorRight = 1.0f;
        AnchorTop = 0.0f;
        AnchorBottom = 0.0f;

        // Offset leftward to make room for sidebar + toggle button
        OffsetLeft = -(SidebarWidth + ToggleButtonWidth);
        OffsetRight = 0f;
        OffsetTop = 80f;
        OffsetBottom = 80f + 400f;

        // Background panel
        _panel = new Panel();
        _panel.SetAnchorsPreset(LayoutPreset.FullRect);
        _panel.OffsetRight = -ToggleButtonWidth;
        AddChild(_panel);

        // Title label inside the panel
        var title = new Label();
        title.Text = "Players";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
        title.AddThemeConstantOverride("outline_size", 1);
        title.SetAnchorsAndOffsetsPreset(LayoutPreset.TopWide);
        title.OffsetBottom = 28f;
        _panel.AddChild(title);

        // Separator under title
        var sep = new HSeparator();
        sep.SetAnchorsAndOffsetsPreset(LayoutPreset.TopWide);
        sep.OffsetTop = 28f;
        sep.OffsetBottom = 32f;
        _panel.AddChild(sep);

        // Scrollable area for player rows
        var scroll = new ScrollContainer();
        scroll.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        scroll.OffsetTop = 34f;
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        _panel.AddChild(scroll);

        _playerListContainer = new VBoxContainer();
        _playerListContainer.AddThemeConstantOverride("separation", 6);
        _playerListContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _playerListContainer.SizeFlagsVertical = SizeFlags.ShrinkBegin;
        scroll.AddChild(_playerListContainer);

        // Toggle button on the right edge
        _toggleButton = new Button();
        _toggleButton.Text = "◀";
        _toggleButton.SetAnchorsAndOffsetsPreset(LayoutPreset.TopRight);
        _toggleButton.OffsetLeft = -ToggleButtonWidth;
        _toggleButton.OffsetRight = 0f;
        _toggleButton.OffsetTop = 0f;
        _toggleButton.OffsetBottom = 60f;
        _toggleButton.Pressed += OnTogglePressed;
        AddChild(_toggleButton);
    }

    private void OnTogglePressed()
    {
        _collapsed = !_collapsed;
        _toggleButton.Text = _collapsed ? "▶" : "◀";

        float targetOffsetLeft = _collapsed
            ? -(ToggleButtonWidth)
            : -(SidebarWidth + ToggleButtonWidth);

        _tween?.Kill();
        _tween = CreateTween();
        _tween.TweenProperty(this, "offset_left", targetOffsetLeft, AnimDuration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.InOut);
    }

    public override void _Process(double delta)
    {
        foreach (var row in _rows)
        {
            row.UpdateDisplay();
        }
    }
}
