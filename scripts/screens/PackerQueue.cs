using Godot;
using Godot.Collections;

/// <summary>
/// PackTrack - Packer Queue Screen (C#)
/// Displays the queue of rigs awaiting packing at the drop zone.
/// </summary>
public partial class PackerQueue : Control
{
    // ── Color constants ──────────────────────────────────────────────────
    private static readonly Color Primary   = new("#1B3A5C");
    private static readonly Color Orange    = new("#E87B35");
    private static readonly Color Green     = new("#2D9F5C");
    private static readonly Color Red       = new("#D94141");
    private static readonly Color Amber     = new("#F5A623");
    private static readonly Color Bg        = new("#F5F5F5");
    private static readonly Color CardColor = Colors.White;
    private static readonly Color TextColor = new("#1E1E1E");
    private static readonly Color TextSec   = new("#4A4A4A");

    // ── Cached references ────────────────────────────────────────────────
    private Label _subtitleLabel;
    private VBoxContainer _rigListContainer;
    private Dictionary<string, Button> _navButtons = new();

    public override void _Ready()
    {
        Name = "PackerQueue";
        SetAnchorsPreset(LayoutPreset.FullRect);

        // ── Background ──────────────────────────────────────────────────
        var bgPanel = new PanelContainer();
        bgPanel.SetAnchorsPreset(LayoutPreset.FullRect);
        var bgStyle = new StyleBoxFlat();
        bgStyle.BgColor = Bg;
        bgPanel.AddThemeStyleboxOverride("panel", bgStyle);
        AddChild(bgPanel);

        // ── Root vertical layout ────────────────────────────────────────
        var rootVbox = new VBoxContainer();
        rootVbox.SetAnchorsPreset(LayoutPreset.FullRect);
        rootVbox.AddThemeConstantOverride("separation", 0);
        AddChild(rootVbox);

        // ── Header ──────────────────────────────────────────────────────
        var headerPanel = new PanelContainer();
        var headerStyle = new StyleBoxFlat();
        headerStyle.BgColor = Primary;
        headerStyle.ContentMarginLeft   = 20;
        headerStyle.ContentMarginRight  = 20;
        headerStyle.ContentMarginTop    = 16;
        headerStyle.ContentMarginBottom = 16;
        headerPanel.AddThemeStyleboxOverride("panel", headerStyle);
        rootVbox.AddChild(headerPanel);

        var headerVbox = new VBoxContainer();
        headerVbox.AddThemeConstantOverride("separation", 4);
        headerPanel.AddChild(headerVbox);

        var titleLabel = new Label();
        titleLabel.Text = "Queue";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleLabel.AddThemeColorOverride("font_color", Colors.White);
        headerVbox.AddChild(titleLabel);

        _subtitleLabel = new Label();
        _subtitleLabel.AddThemeFontSizeOverride("font_size", 14);
        _subtitleLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1, 0.7f));
        headerVbox.AddChild(_subtitleLabel);

        // ── Scroll area for rig cards ───────────────────────────────────
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        rootVbox.AddChild(scroll);

        var scrollPadding = new MarginContainer();
        scrollPadding.AddThemeConstantOverride("margin_left",   16);
        scrollPadding.AddThemeConstantOverride("margin_right",  16);
        scrollPadding.AddThemeConstantOverride("margin_top",    12);
        scrollPadding.AddThemeConstantOverride("margin_bottom", 12);
        scrollPadding.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(scrollPadding);

        _rigListContainer = new VBoxContainer();
        _rigListContainer.AddThemeConstantOverride("separation", 12);
        _rigListContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scrollPadding.AddChild(_rigListContainer);

        // ── Bottom nav bar ──────────────────────────────────────────────
        BuildNavBar(rootVbox, 0);

        // ── Populate rig cards ──────────────────────────────────────────
        PopulateRigs();
    }

    // ── Populate rig list ────────────────────────────────────────────────

    private void PopulateRigs()
    {
        foreach (var child in _rigListContainer.GetChildren())
        {
            child.QueueFree();
        }

        var rigs = GetRigsData();

        _subtitleLabel.Text = $"{rigs.Count} rigs waiting";

        foreach (Dictionary rig in rigs)
        {
            var card = CreateRigCard(rig);
            _rigListContainer.AddChild(card);
        }
    }

    private Godot.Collections.Array GetRigsData()
    {
        var gameData = GetNodeOrNull<Node>("/root/GameData");
        if (gameData != null)
        {
            var rigs = (Godot.Collections.Array)gameData.Get("rigs");
            if (rigs != null && rigs.Count > 0)
                return rigs;
        }
        return GetSampleRigs();
    }

    // ── Rig card builder ─────────────────────────────────────────────────

    private PanelContainer CreateRigCard(Dictionary rig)
    {
        var card = new PanelContainer();
        var cardStyle = new StyleBoxFlat();
        cardStyle.BgColor = CardColor;
        cardStyle.CornerRadiusTopLeft     = 10;
        cardStyle.CornerRadiusTopRight    = 10;
        cardStyle.CornerRadiusBottomLeft  = 10;
        cardStyle.CornerRadiusBottomRight = 10;
        cardStyle.ShadowColor  = new Color(0, 0, 0, 0.08f);
        cardStyle.ShadowSize   = 4;
        cardStyle.ShadowOffset = new Vector2(0, 2);
        cardStyle.ContentMarginLeft   = 0;
        cardStyle.ContentMarginRight  = 16;
        cardStyle.ContentMarginTop    = 0;
        cardStyle.ContentMarginBottom = 0;
        card.AddThemeStyleboxOverride("panel", cardStyle);
        card.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        // Card content HBox
        var cardHbox = new HBoxContainer();
        cardHbox.AddThemeConstantOverride("separation", 0);
        card.AddChild(cardHbox);

        // Colored left strip
        string status = rig.ContainsKey("status") ? (string)rig["status"] : "ok";
        Color statusColor = GetStatusColor(status);

        var strip = new PanelContainer();
        var stripStyle = new StyleBoxFlat();
        stripStyle.BgColor = statusColor;
        stripStyle.CornerRadiusTopLeft    = 10;
        stripStyle.CornerRadiusBottomLeft = 10;
        stripStyle.ContentMarginLeft  = 0;
        stripStyle.ContentMarginRight = 0;
        strip.AddThemeStyleboxOverride("panel", stripStyle);
        strip.CustomMinimumSize = new Vector2(6, 0);
        cardHbox.AddChild(strip);

        // Card content area
        var contentMargin = new MarginContainer();
        contentMargin.AddThemeConstantOverride("margin_left",   14);
        contentMargin.AddThemeConstantOverride("margin_right",  0);
        contentMargin.AddThemeConstantOverride("margin_top",    14);
        contentMargin.AddThemeConstantOverride("margin_bottom", 14);
        contentMargin.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        cardHbox.AddChild(contentMargin);

        var contentVbox = new VBoxContainer();
        contentVbox.AddThemeConstantOverride("separation", 4);
        contentMargin.AddChild(contentVbox);

        // Row 1: Rig ID + Status
        var row1 = new HBoxContainer();
        row1.AddThemeConstantOverride("separation", 8);
        contentVbox.AddChild(row1);

        string rigId = rig.ContainsKey("rig_id") ? (string)rig["rig_id"] : "Unknown";

        var rigIdLabel = new Label();
        rigIdLabel.Text = rigId;
        rigIdLabel.AddThemeFontSizeOverride("font_size", 16);
        rigIdLabel.AddThemeColorOverride("font_color", TextColor);
        rigIdLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        row1.AddChild(rigIdLabel);

        var statusLabel = new Label();
        statusLabel.Text = status;
        statusLabel.AddThemeFontSizeOverride("font_size", 12);
        statusLabel.AddThemeColorOverride("font_color", statusColor);
        row1.AddChild(statusLabel);

        // Row 2: Owner
        string ownerName = rig.ContainsKey("owner_name") ? (string)rig["owner_name"] : "";
        var ownerLabel = new Label();
        ownerLabel.Text = ownerName;
        ownerLabel.AddThemeFontSizeOverride("font_size", 14);
        ownerLabel.AddThemeColorOverride("font_color", TextSec);
        contentVbox.AddChild(ownerLabel);

        // Row 3: Last packed + Total packs
        var row3 = new HBoxContainer();
        row3.AddThemeConstantOverride("separation", 16);
        contentVbox.AddChild(row3);

        string lastPacked = rig.ContainsKey("last_packed_date") ? (string)rig["last_packed_date"] : "N/A";
        var lastPackedLabel = new Label();
        lastPackedLabel.Text = $"Last: {lastPacked}";
        lastPackedLabel.AddThemeFontSizeOverride("font_size", 12);
        lastPackedLabel.AddThemeColorOverride("font_color", TextSec);
        row3.AddChild(lastPackedLabel);

        int totalPacks = rig.ContainsKey("total_packs") ? (int)rig["total_packs"] : 0;
        var packCountLabel = new Label();
        packCountLabel.Text = $"Packs: {totalPacks}";
        packCountLabel.AddThemeFontSizeOverride("font_size", 12);
        packCountLabel.AddThemeColorOverride("font_color", TextSec);
        row3.AddChild(packCountLabel);

        // Invisible click overlay
        var cardButton = new Button();
        cardButton.Flat = true;
        cardButton.SetAnchorsPreset(LayoutPreset.FullRect);
        cardButton.MouseDefaultCursorShape = CursorShape.PointingHand;
        cardButton.Pressed += () => OnRigPressed(rigId);
        card.AddChild(cardButton);

        return card;
    }

    // ── Status color mapping ─────────────────────────────────────────────

    private Color GetStatusColor(string status)
    {
        return status switch
        {
            "ok" or "green"        => Green,
            "warning" or "amber"   => Amber,
            "overdue" or "red"     => Red,
            _                      => Green,
        };
    }

    // ── Navigation ───────────────────────────────────────────────────────

    private void OnRigPressed(string rigId)
    {
        var nav = GetNodeOrNull<NavManager>("/root/NavManager");
        nav?.NavigateTo($"rig_detail:{rigId}");
    }

    private void OnNavTabPressed(string screenName)
    {
        if (screenName == "packer_queue") return;
        var nav = GetNodeOrNull<NavManager>("/root/NavManager");
        nav?.NavigateTo(screenName);
    }

    // ── Bottom nav bar builder ───────────────────────────────────────────

    private void BuildNavBar(VBoxContainer parent, int activeIndex)
    {
        var navPanel = new PanelContainer();
        var navStyle = new StyleBoxFlat();
        navStyle.BgColor = Colors.White;
        navStyle.BorderColor = new Color(0, 0, 0, 0.1f);
        navStyle.BorderWidthTop = 1;
        navStyle.ContentMarginTop    = 8;
        navStyle.ContentMarginBottom = 8;
        navStyle.ContentMarginLeft   = 8;
        navStyle.ContentMarginRight  = 8;
        navPanel.AddThemeStyleboxOverride("panel", navStyle);
        parent.AddChild(navPanel);

        var navHbox = new HBoxContainer();
        navHbox.Alignment = BoxContainer.AlignmentMode.Center;
        navHbox.AddThemeConstantOverride("separation", 0);
        navPanel.AddChild(navHbox);

        string[] tabs       = { "Queue", "History", "Billing", "Profile" };
        string[] tabScreens = { "packer_queue", "packer_history", "packer_billing", "packer_profile" };

        for (int i = 0; i < tabs.Length; i++)
        {
            var tabBtn = new Button();
            tabBtn.Text = tabs[i];
            tabBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            tabBtn.AddThemeFontSizeOverride("font_size", 13);
            tabBtn.Flat = true;
            tabBtn.AddThemeColorOverride("font_color", i == activeIndex ? Orange : TextSec);

            string screen = tabScreens[i];
            tabBtn.Pressed += () => OnNavTabPressed(screen);
            navHbox.AddChild(tabBtn);
            _navButtons[tabs[i]] = tabBtn;
        }
    }

    // ── Sample data ──────────────────────────────────────────────────────

    private Godot.Collections.Array GetSampleRigs()
    {
        return new Godot.Collections.Array
        {
            new Dictionary
            {
                { "rig_id", "N4521-Main" },
                { "owner_name", "Sarah Chen" },
                { "status", "ok" },
                { "last_packed_date", "2026-04-14" },
                { "total_packs", 287 }
            },
            new Dictionary
            {
                { "rig_id", "N4521-Rsv" },
                { "owner_name", "Sarah Chen" },
                { "status", "ok" },
                { "last_packed_date", "2026-03-18" },
                { "total_packs", 4 }
            },
            new Dictionary
            {
                { "rig_id", "N7833-Main" },
                { "owner_name", "Marcus Rodriguez" },
                { "status", "warning" },
                { "last_packed_date", "2026-04-13" },
                { "total_packs", 412 }
            },
            new Dictionary
            {
                { "rig_id", "N6290-Main" },
                { "owner_name", "Emily Foster" },
                { "status", "ok" },
                { "last_packed_date", "2026-04-12" },
                { "total_packs", 195 }
            },
            new Dictionary
            {
                { "rig_id", "N5117-Main" },
                { "owner_name", "Dylan Park" },
                { "status", "warning" },
                { "last_packed_date", "2026-04-14" },
                { "total_packs", 534 }
            },
            new Dictionary
            {
                { "rig_id", "N5117-Rsv" },
                { "owner_name", "Dylan Park" },
                { "status", "overdue" },
                { "last_packed_date", "2025-10-30" },
                { "total_packs", 3 }
            },
            new Dictionary
            {
                { "rig_id", "N8842-Tandem" },
                { "owner_name", "SkyHigh DZ (Fleet)" },
                { "status", "ok" },
                { "last_packed_date", "2026-04-13" },
                { "total_packs", 1023 }
            },
        };
    }
}
