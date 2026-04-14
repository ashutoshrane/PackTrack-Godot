using Godot;
using System;

/// <summary>
/// Onboarding — role selection screen shown on first launch.
/// Presents "PackTrack" branding and three role cards (Packer, DZ Operator, Skydiver).
/// On selection, sets CurrentUser role/name in GameData and navigates to the home screen.
/// </summary>
public partial class Onboarding : Control
{
    // ── Colors ───────────────────────────────────────────────────────────
    private static readonly Color Primary  = new Color("#1B3A5C");
    private static readonly Color Orange   = new Color("#E87B35");
    private static readonly Color Blue     = new Color("#4A9FD9");
    private static readonly Color Green    = new Color("#2D9F5C");
    private static readonly Color Bg       = new Color("#F5F5F5");
    private static readonly Color TextClr  = new Color("#1E1E1E");
    private static readonly Color TextSec  = new Color("#4A4A4A");
    private static readonly Color White    = Colors.White;

    private GameData _gameData;
    private NavManager _navManager;

    public override void _Ready()
    {
        _gameData = GetNode<GameData>("/root/GameData");
        _navManager = GetNode<NavManager>("/root/NavManager");
        BuildUi();
    }

    private void BuildUi()
    {
        // Root background
        var bgRect = new ColorRect();
        bgRect.Color = Bg;
        bgRect.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(bgRect);

        // Centered wrapper
        var center = new CenterContainer();
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(center);

        var mainVbox = new VBoxContainer();
        mainVbox.AddThemeConstantOverride("separation", 32);
        mainVbox.Alignment = BoxContainer.AlignmentMode.Center;
        mainVbox.CustomMinimumSize = new Vector2(320, 0);
        center.AddChild(mainVbox);

        // ── Logo + subtitle ─────────────────────────────────────────────
        var titleSection = new VBoxContainer();
        titleSection.AddThemeConstantOverride("separation", 8);
        titleSection.Alignment = BoxContainer.AlignmentMode.Center;
        mainVbox.AddChild(titleSection);

        var logo = new Label();
        logo.Text = "PackTrack";
        logo.AddThemeFontSizeOverride("font_size", 42);
        logo.AddThemeColorOverride("font_color", Primary);
        logo.HorizontalAlignment = HorizontalAlignment.Center;
        titleSection.AddChild(logo);

        var subtitle = new Label();
        subtitle.Text = "Welcome! Select your role";
        subtitle.AddThemeFontSizeOverride("font_size", 16);
        subtitle.AddThemeColorOverride("font_color", TextSec);
        subtitle.HorizontalAlignment = HorizontalAlignment.Center;
        titleSection.AddChild(subtitle);

        // ── Role cards ──────────────────────────────────────────────────
        var cards = new VBoxContainer();
        cards.AddThemeConstantOverride("separation", 16);
        mainVbox.AddChild(cards);

        cards.AddChild(CreateRoleCard(
            "Packer", "I pack parachutes",
            Orange, "packer", "packer_queue"
        ));
        cards.AddChild(CreateRoleCard(
            "DZ Operator", "I manage the drop zone",
            Blue, "operator", "operator_dashboard"
        ));
        cards.AddChild(CreateRoleCard(
            "Skydiver", "I jump!",
            Green, "skydiver", "skydiver_rig"
        ));
    }

    // ── Role card ───────────────────────────────────────────────────────
    private PanelContainer CreateRoleCard(string titleText, string descText,
        Color accent, string roleKey, string homeScreen)
    {
        var panel = new PanelContainer();
        panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var style = new StyleBoxFlat();
        style.BgColor = White;
        style.CornerRadiusTopLeft = 12;
        style.CornerRadiusTopRight = 12;
        style.CornerRadiusBottomLeft = 12;
        style.CornerRadiusBottomRight = 12;
        style.BorderWidthLeft = 5;
        style.BorderColor = accent;
        style.ContentMarginLeft = 20;
        style.ContentMarginRight = 20;
        style.ContentMarginTop = 20;
        style.ContentMarginBottom = 20;
        panel.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 6);
        panel.AddChild(vbox);

        var title = new Label();
        title.Text = titleText;
        title.AddThemeFontSizeOverride("font_size", 20);
        title.AddThemeColorOverride("font_color", TextClr);
        vbox.AddChild(title);

        var desc = new Label();
        desc.Text = descText;
        desc.AddThemeFontSizeOverride("font_size", 14);
        desc.AddThemeColorOverride("font_color", TextSec);
        vbox.AddChild(desc);

        // Invisible tap overlay
        var btn = new Button();
        btn.Flat = true;
        btn.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        btn.MouseDefaultCursorShape = CursorShape.PointingHand;
        btn.Pressed += () => OnRoleSelected(roleKey, homeScreen);
        panel.AddChild(btn);

        return panel;
    }

    // ── Callbacks ────────────────────────────────────────────────────────
    private void OnRoleSelected(string role, string homeScreen)
    {
        _gameData.CurrentUser["role"] = role;

        // Set demo user identity based on role
        switch (role)
        {
            case "packer":
                _gameData.CurrentUser["name"] = "Jake Mitchell";
                _gameData.CurrentUser["id"] = "PKR-001";
                break;
            case "operator":
                _gameData.CurrentUser["name"] = "SkyHigh DZ Admin";
                _gameData.CurrentUser["id"] = "OPR-001";
                break;
            case "skydiver":
                _gameData.CurrentUser["name"] = "Sarah Chen";
                _gameData.CurrentUser["id"] = "SKY-101";
                break;
        }

        _navManager.SetRootScreen(homeScreen);
    }
}
