using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
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

    private IRunState _runState;
    private LineEdit _cardInput;
    private Label _cardStatusLabel;
    private Timer _cardStatusTimer;

    public void Initialize(IRunState runState)
    {
        _runState = runState;
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
        OffsetBottom = 80f + 600f;

        // Background panel
        _panel = new Panel();
        _panel.SetAnchorsPreset(LayoutPreset.FullRect);
        _panel.OffsetRight = -ToggleButtonWidth;
        AddChild(_panel);

        // Main vertical layout inside the panel (no scrolling)
        var mainVBox = new VBoxContainer();
        mainVBox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        mainVBox.AddThemeConstantOverride("separation", 4);
        _panel.AddChild(mainVBox);

        // Title label
        var title = new Label();
        title.Text = "Players";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
        title.AddThemeConstantOverride("outline_size", 1);
        mainVBox.AddChild(title);

        // Separator under title
        var sep = new HSeparator();
        mainVBox.AddChild(sep);

        // Player list container (no scroll wrapper)
        _playerListContainer = new VBoxContainer();
        _playerListContainer.AddThemeConstantOverride("separation", 6);
        _playerListContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _playerListContainer.SizeFlagsVertical = SizeFlags.ShrinkBegin;
        mainVBox.AddChild(_playerListContainer);

        // Separator before card lookup
        var cardSep = new HSeparator();
        mainVBox.AddChild(cardSep);

        // Get Card section title
        var cardTitle = new Label();
        cardTitle.Text = "Get Card";
        cardTitle.HorizontalAlignment = HorizontalAlignment.Center;
        cardTitle.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
        mainVBox.AddChild(cardTitle);

        // Input row
        var cardRow = new HBoxContainer();
        cardRow.AddThemeConstantOverride("separation", 4);

        _cardInput = new LineEdit();
        _cardInput.PlaceholderText = "e.g. ANGER";
        _cardInput.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _cardInput.TextSubmitted += _ => OnGetCard();
        cardRow.AddChild(_cardInput);

        var getButton = new Button();
        getButton.Text = "Get";
        getButton.Pressed += OnGetCard;
        cardRow.AddChild(getButton);

        mainVBox.AddChild(cardRow);

        // Status label for card lookup feedback
        _cardStatusLabel = new Label();
        _cardStatusLabel.AddThemeFontSizeOverride("font_size", 11);
        mainVBox.AddChild(_cardStatusLabel);

        // Timer for clearing status
        _cardStatusTimer = new Timer();
        _cardStatusTimer.WaitTime = 3.0;
        _cardStatusTimer.OneShot = true;
        _cardStatusTimer.Timeout += () => _cardStatusLabel.Text = "";
        AddChild(_cardStatusTimer);

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

    private void OnGetCard()
    {
        string cardName = _cardInput.Text.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(cardName))
        {
            ShowCardStatus("Enter a card name.", true);
            return;
        }

        if (!RunManager.Instance.IsInProgress)
        {
            ShowCardStatus("No run in progress.", true);
            return;
        }

        CardModel cardModel = ModelDb.AllCards.FirstOrDefault(c => c.Id.Entry == cardName);
        if (cardModel == null)
        {
            ShowCardStatus($"Card '{cardName}' not found.", true);
            return;
        }

        RunState runState = RunManager.Instance.DebugOnlyGetState();
        Player player = LocalContext.GetMe(runState);
        if (player == null)
        {
            ShowCardStatus("No local player found.", true);
            return;
        }

        PileType targetPile = PileType.Hand;

        // If not in combat, fall back to deck
        if (CombatManager.Instance.DebugOnlyGetState() == null)
        {
            targetPile = PileType.Deck;
            ShowCardStatus($"Not in combat — adding '{cardName}' to Deck.", false);
        }

        // Check hand capacity
        if (targetPile == PileType.Hand)
        {
            CardPile handPile = PileType.Hand.GetPile(player);
            if (handPile.Cards.Count >= 10)
            {
                ShowCardStatus("Hand is full (10/10).", true);
                return;
            }
        }

        ICardScope cardScope = targetPile.IsCombatPile()
            ? CombatManager.Instance.DebugOnlyGetState()
            : (ICardScope)runState;

        CardModel card = cardScope.CreateCard(cardModel, player);
        Task task = CardPileCmd.Add(card, targetPile);
        TaskHelper.RunSafely(task);

        if (targetPile == PileType.Hand)
            ShowCardStatus($"Added '{cardName}' to Hand.", false);

        _cardInput.Text = "";
    }

    private void ShowCardStatus(string message, bool isError)
    {
        _cardStatusLabel.Text = message;
        _cardStatusLabel.AddThemeColorOverride("font_color",
            isError ? new Color(1f, 0.33f, 0.33f) : new Color(0.33f, 1f, 0.33f));
        _cardStatusTimer.Start();
    }

    public override void _Process(double delta)
    {
        foreach (var row in _rows)
        {
            row.UpdateDisplay();
        }
    }
}
