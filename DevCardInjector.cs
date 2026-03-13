using System;
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

public partial class DevCardInjector : Control
{
    private LineEdit _input;
    private Label _statusLabel;
    private Timer _statusTimer;

    public override void _Ready()
    {
        // Anchor to top-left, render on top of everything
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsPreset(LayoutPreset.TopLeft);
        ZIndex = 100;

        var panel = new PanelContainer();
        panel.MouseFilter = MouseFilterEnum.Stop;
        panel.SetAnchorsPreset(LayoutPreset.TopLeft);
        panel.Position = new Vector2(10, 10);

        // Semi-transparent background
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        styleBox.SetCornerRadiusAll(6);
        styleBox.SetContentMarginAll(8);
        panel.AddThemeStyleboxOverride("panel", styleBox);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 4);

        var titleLabel = new Label();
        titleLabel.Text = "Dev Card Injector";
        titleLabel.AddThemeFontSizeOverride("font_size", 12);
        vbox.AddChild(titleLabel);

        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 4);

        _input = new LineEdit();
        _input.PlaceholderText = "Card name (e.g. ANGER)";
        _input.CustomMinimumSize = new Vector2(200, 0);
        _input.TextSubmitted += OnTextSubmitted;
        hbox.AddChild(_input);

        var handButton = new Button();
        handButton.Text = "Add to Hand";
        handButton.Pressed += () => InjectCard(PileType.Hand);
        hbox.AddChild(handButton);

        var deckButton = new Button();
        deckButton.Text = "Add to Deck";
        deckButton.Pressed += () => InjectCard(PileType.Deck);
        hbox.AddChild(deckButton);

        vbox.AddChild(hbox);

        _statusLabel = new Label();
        _statusLabel.AddThemeFontSizeOverride("font_size", 11);
        vbox.AddChild(_statusLabel);

        panel.AddChild(vbox);
        AddChild(panel);

        _statusTimer = new Timer();
        _statusTimer.WaitTime = 3.0;
        _statusTimer.OneShot = true;
        _statusTimer.Timeout += () => _statusLabel.Text = "";
        AddChild(_statusTimer);
    }

    private void OnTextSubmitted(string text)
    {
        InjectCard(PileType.Hand);
    }

    private void InjectCard(PileType targetPile)
    {
        string cardName = _input.Text.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(cardName))
        {
            ShowStatus("Enter a card name.", true);
            return;
        }

        if (!RunManager.Instance.IsInProgress)
        {
            ShowStatus("No run in progress.", true);
            return;
        }

        CardModel cardModel = ModelDb.AllCards.FirstOrDefault(c => c.Id.Entry == cardName);
        if (cardModel == null)
        {
            ShowStatus($"Card '{cardName}' not found.", true);
            return;
        }

        RunState runState = RunManager.Instance.DebugOnlyGetState();
        Player player = LocalContext.GetMe(runState);
        if (player == null)
        {
            ShowStatus("No local player found.", true);
            return;
        }

        // If targeting hand but not in combat, fall back to deck
        if (targetPile.IsCombatPile() && CombatManager.Instance.DebugOnlyGetState() == null)
        {
            targetPile = PileType.Deck;
            ShowStatus($"Not in combat — adding '{cardName}' to Deck instead.", false);
        }

        // Check hand capacity
        if (targetPile == PileType.Hand)
        {
            CardPile handPile = PileType.Hand.GetPile(player);
            if (handPile.Cards.Count >= 10)
            {
                ShowStatus("Hand is full (10/10).", true);
                return;
            }
        }

        ICardScope cardScope = targetPile.IsCombatPile()
            ? CombatManager.Instance.DebugOnlyGetState()
            : (ICardScope)runState;

        CardModel card = cardScope.CreateCard(cardModel, player);
        Task task = CardPileCmd.Add(card, targetPile);
        TaskHelper.RunSafely(task);

        ShowStatus($"Added '{cardName}' to {targetPile}.", false);
        _input.Text = "";
    }

    private void ShowStatus(string message, bool isError)
    {
        _statusLabel.Text = message;
        _statusLabel.AddThemeColorOverride("font_color",
            isError ? new Color(1f, 0.33f, 0.33f) : new Color(0.33f, 1f, 0.33f));
        _statusTimer.Start();
    }
}
