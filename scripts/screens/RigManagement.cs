using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Rig Management — browsable list of all rigs at the drop zone.
/// Filter chips let the operator narrow by status (All / OK / Warning / Overdue).
/// Tapping a rig card navigates to its detailed profile.
/// </summary>
public partial class RigManagement : Control
{
    // ── Colors ───────────────────────────────────────────────────────────
    private static readonly Color Primary  = new Color("#1B3A5C");
    private static readonly Color Orange   = new Color("#E87B35");
    private static readonly Color Green    = new Color("#2D9F5C");
    private static readonly Color Red      = new Color("#D94141");
    private static readonly Color Amber    = new Color("#F5A623");
    private static readonly Color Bg       = new Color("#F5F5F5");
    private static readonly Color TextClr  = new Color("#1E1E1E");
    private static readonly Color TextSec  = new Color("#4A4A4A");
    private static readonly Color White    = Colors.White;

    private GameData _gameData;
    private NavManager _navManager;

    private string _activeFilter = "all";
    private VBoxContainer _rigListContainer;
    private readonly List<Button> _filterButtons = new List<Button>();

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

        var rootVbox = new VBoxContainer();
        rootVbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        rootVbox.AddThemeConstantOverride("separation", 0);
        AddChild(rootVbox);

        // ── Header ──────────────────────────────────────────────────────
        rootVbox.AddChild(CreateHeader());

        // ── Filter chips ────────────────────────────────────────────────
        var chipPad = new MarginContainer();
        chipPad.AddThemeConstantOverride("margin_left", 16);
        chipPad.AddThemeConstantOverride("margin_right", 16);
        chipPad.AddThemeConstantOverride("margin_top", 12);
        chipPad.AddThemeConstantOverride("margin_bottom", 4);
        rootVbox.AddChild(chipPad);

        var chipRow = new HBoxContainer();
        chipRow.AddThemeConstantOverride("separation", 8);
        chipPad.AddChild(chipRow);

        string[] filters = { "All", "OK", "Warning", "Overdue" };
        foreach (string filter in filters)
        {
            var chip = CreateFilterChip(filter);
            _filterButtons.Add(chip);
            chipRow.AddChild(chip);
        }

        // ── Scrollable rig list ─────────────────────────────────────────
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        rootVbox.AddChild(scroll);

        var scrollPad = new MarginContainer();
        scrollPad.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scrollPad.AddThemeConstantOverride("margin_left", 16);
        scrollPad.AddThemeConstantOverride("margin_right", 16);
        scrollPad.AddThemeConstantOverride("margin_top", 8);
        scrollPad.AddThemeConstantOverride("margin_bottom", 16);
        scroll.AddChild(scrollPad);

        _rigListContainer = new VBoxContainer();
        _rigListContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _rigListContainer.AddThemeConstantOverride("separation", 12);
        scrollPad.AddChild(_rigListContainer);

        PopulateRigs();
        UpdateFilterStyles();
    }

    // ── Header ──────────────────────────────────────────────────────────
    private PanelContainer CreateHeader()
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat();
        style.BgColor = Primary;
        style.ContentMarginLeft = 16;
        style.ContentMarginRight = 16;
        style.ContentMarginTop = 12;
        style.ContentMarginBottom = 12;
        panel.AddThemeStyleboxOverride("panel", style);

        var title = new Label();
        title.Text = "Rigs";
        title.AddThemeFontSizeOverride("font_size", 22);
        title.AddThemeColorOverride("font_color", White);
        panel.AddChild(title);

        return panel;
    }

    // ── Filter chip ─────────────────────────────────────────────────────
    private Button CreateFilterChip(string label)
    {
        var btn = new Button();
        btn.Text = label;
        btn.AddThemeFontSizeOverride("font_size", 13);
        btn.CustomMinimumSize = new Vector2(70, 32);

        string filterKey = label.ToLower();
        btn.Pressed += () => OnFilterSelected(filterKey);

        return btn;
    }

    private void OnFilterSelected(string filter)
    {
        _activeFilter = filter;
        UpdateFilterStyles();
        PopulateRigs();
    }

    private void UpdateFilterStyles()
    {
        foreach (var btn in _filterButtons)
        {
            string key = btn.Text.ToLower();
            bool active = key == _activeFilter;

            var style = new StyleBoxFlat();
            style.CornerRadiusTopLeft = 16;
            style.CornerRadiusTopRight = 16;
            style.CornerRadiusBottomLeft = 16;
            style.CornerRadiusBottomRight = 16;
            style.ContentMarginLeft = 14;
            style.ContentMarginRight = 14;
            style.ContentMarginTop = 6;
            style.ContentMarginBottom = 6;

            if (active)
            {
                style.BgColor = Primary;
                btn.AddThemeColorOverride("font_color", White);
            }
            else
            {
                style.BgColor = White;
                style.BorderWidthTop = 1;
                style.BorderWidthBottom = 1;
                style.BorderWidthLeft = 1;
                style.BorderWidthRight = 1;
                style.BorderColor = new Color(0.8f, 0.8f, 0.8f);
                btn.AddThemeColorOverride("font_color", TextSec);
            }

            btn.AddThemeStyleboxOverride("normal", style);
            btn.AddThemeStyleboxOverride("hover", style);
            btn.AddThemeStyleboxOverride("pressed", style);
        }
    }

    // ── Rig list ────────────────────────────────────────────────────────
    private void PopulateRigs()
    {
        // Clear existing cards
        foreach (Node child in _rigListContainer.GetChildren())
        {
            child.QueueFree();
        }

        var rigs = _gameData.Rigs;
        foreach (var rig in rigs)
        {
            string status = rig.ContainsKey("status") ? rig["status"].ToString() : "ok";
            if (_activeFilter != "all" && status != _activeFilter)
                continue;

            _rigListContainer.AddChild(CreateRigCard(rig));
        }
    }

    // ── Rig card ────────────────────────────────────────────────────────
    private PanelContainer CreateRigCard(Godot.Collections.Dictionary rig)
    {
        string rigId = rig.ContainsKey("id") ? rig["id"].ToString() : "";
        string owner = rig.ContainsKey("owner") ? rig["owner"].ToString() : "";
        string status = rig.ContainsKey("status") ? rig["status"].ToString() : "ok";
        int packCount = rig.ContainsKey("packCount") ? Convert.ToInt32(rig["packCount"]) : 0;

        var panel = new PanelContainer();
        panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var style = new StyleBoxFlat();
        style.BgColor = White;
        style.CornerRadiusTopLeft = 10;
        style.CornerRadiusTopRight = 10;
        style.CornerRadiusBottomLeft = 10;
        style.CornerRadiusBottomRight = 10;
        style.ContentMarginLeft = 16;
        style.ContentMarginRight = 16;
        style.ContentMarginTop = 14;
        style.ContentMarginBottom = 14;
        panel.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 6);
        panel.AddChild(vbox);

        // Top row: Rig ID + status
        var topRow = new HBoxContainer();
        vbox.AddChild(topRow);

        var idLabel = new Label();
        idLabel.Text = rigId;
        idLabel.AddThemeFontSizeOverride("font_size", 18);
        idLabel.AddThemeColorOverride("font_color", TextClr);
        idLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        topRow.AddChild(idLabel);

        topRow.AddChild(CreateStatusDot(status));

        // Owner
        var ownerLabel = new Label();
        ownerLabel.Text = owner;
        ownerLabel.AddThemeFontSizeOverride("font_size", 14);
        ownerLabel.AddThemeColorOverride("font_color", TextSec);
        vbox.AddChild(ownerLabel);

        // Pack count
        var packLabel = new Label();
        packLabel.Text = $"{packCount} packs";
        packLabel.AddThemeFontSizeOverride("font_size", 13);
        packLabel.AddThemeColorOverride("font_color", TextSec);
        vbox.AddChild(packLabel);

        // Invisible tap button
        var tapBtn = new Button();
        tapBtn.Flat = true;
        tapBtn.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        tapBtn.MouseDefaultCursorShape = CursorShape.PointingHand;
        tapBtn.Pressed += () => _navManager.NavigateTo($"rig_profile:{rigId}");
        panel.AddChild(tapBtn);

        return panel;
    }

    // ── Status dot + label ──────────────────────────────────────────────
    private HBoxContainer CreateStatusDot(string status)
    {
        Color dotColor;
        string labelText;
        switch (status)
        {
            case "warning":
                dotColor = Amber;
                labelText = "Warning";
                break;
            case "overdue":
                dotColor = Red;
                labelText = "Overdue";
                break;
            default:
                dotColor = Green;
                labelText = "OK";
                break;
        }

        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 6);

        // Dot (small ColorRect)
        var dot = new ColorRect();
        dot.CustomMinimumSize = new Vector2(10, 10);
        dot.Color = dotColor;
        // Center the dot vertically using a CenterContainer
        var dotCenter = new CenterContainer();
        dotCenter.AddChild(dot);
        hbox.AddChild(dotCenter);

        var label = new Label();
        label.Text = labelText;
        label.AddThemeFontSizeOverride("font_size", 13);
        label.AddThemeColorOverride("font_color", dotColor);
        hbox.AddChild(label);

        return hbox;
    }
}
