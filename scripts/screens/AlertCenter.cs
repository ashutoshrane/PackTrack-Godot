using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Alert Center — shows all rigs in warning or overdue status.
/// Each card has an amber or red left border, repack countdown info,
/// and action buttons for "Notify Owner" / "Schedule Now".
/// </summary>
public partial class AlertCenter : Control
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

    public override void _Ready()
    {
        _gameData = GetNode<GameData>("/root/GameData");
        _navManager = GetNode<NavManager>("/root/NavManager");
        BuildUi();
    }

    private void BuildUi()
    {
        // Gather warning + overdue rigs
        var alertRigs = _gameData.Rigs
            .Where(r =>
            {
                string s = r.ContainsKey("status") ? r["status"].ToString() : "";
                return s == "warning" || s == "overdue";
            })
            .ToList();

        // Root background
        var bgRect = new ColorRect();
        bgRect.Color = Bg;
        bgRect.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(bgRect);

        var rootVbox = new VBoxContainer();
        rootVbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        rootVbox.AddThemeConstantOverride("separation", 0);
        AddChild(rootVbox);

        // ── Header with count badge ─────────────────────────────────────
        rootVbox.AddChild(CreateHeader(alertRigs.Count));

        // ── Scrollable body ─────────────────────────────────────────────
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        rootVbox.AddChild(scroll);

        var pad = new MarginContainer();
        pad.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        pad.AddThemeConstantOverride("margin_left", 16);
        pad.AddThemeConstantOverride("margin_right", 16);
        pad.AddThemeConstantOverride("margin_top", 16);
        pad.AddThemeConstantOverride("margin_bottom", 16);
        scroll.AddChild(pad);

        var content = new VBoxContainer();
        content.AddThemeConstantOverride("separation", 12);
        pad.AddChild(content);

        if (alertRigs.Count == 0)
        {
            var noAlerts = new Label();
            noAlerts.Text = "No alerts — all rigs are current!";
            noAlerts.AddThemeFontSizeOverride("font_size", 16);
            noAlerts.AddThemeColorOverride("font_color", Green);
            noAlerts.HorizontalAlignment = HorizontalAlignment.Center;
            content.AddChild(noAlerts);
        }
        else
        {
            // Sort overdue first, then warning
            var sorted = alertRigs
                .OrderBy(r => r.ContainsKey("status") && r["status"].ToString() == "overdue" ? 0 : 1)
                .ToList();

            foreach (var rig in sorted)
            {
                content.AddChild(CreateAlertCard(rig));
            }
        }
    }

    // ── Header ──────────────────────────────────────────────────────────
    private PanelContainer CreateHeader(int count)
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat();
        style.BgColor = Primary;
        style.ContentMarginLeft = 16;
        style.ContentMarginRight = 16;
        style.ContentMarginTop = 12;
        style.ContentMarginBottom = 12;
        panel.AddThemeStyleboxOverride("panel", style);

        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 10);
        panel.AddChild(hbox);

        var backBtn = new Button();
        backBtn.Text = "<";
        backBtn.Flat = true;
        backBtn.AddThemeFontSizeOverride("font_size", 20);
        backBtn.AddThemeColorOverride("font_color", White);
        backBtn.Pressed += () => _navManager.GoBack();
        hbox.AddChild(backBtn);

        var title = new Label();
        title.Text = "Alerts";
        title.AddThemeFontSizeOverride("font_size", 22);
        title.AddThemeColorOverride("font_color", White);
        title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        hbox.AddChild(title);

        // Count badge
        if (count > 0)
        {
            var badge = new PanelContainer();
            var badgeStyle = new StyleBoxFlat();
            badgeStyle.BgColor = Red;
            badgeStyle.CornerRadiusTopLeft = 12;
            badgeStyle.CornerRadiusTopRight = 12;
            badgeStyle.CornerRadiusBottomLeft = 12;
            badgeStyle.CornerRadiusBottomRight = 12;
            badgeStyle.ContentMarginLeft = 10;
            badgeStyle.ContentMarginRight = 10;
            badgeStyle.ContentMarginTop = 2;
            badgeStyle.ContentMarginBottom = 2;
            badge.AddThemeStyleboxOverride("panel", badgeStyle);

            var badgeLabel = new Label();
            badgeLabel.Text = count.ToString();
            badgeLabel.AddThemeFontSizeOverride("font_size", 14);
            badgeLabel.AddThemeColorOverride("font_color", White);
            badge.AddChild(badgeLabel);

            hbox.AddChild(badge);
        }

        return panel;
    }

    // ── Alert card ──────────────────────────────────────────────────────
    private PanelContainer CreateAlertCard(Godot.Collections.Dictionary rig)
    {
        string rigId = rig.ContainsKey("id") ? rig["id"].ToString() : "";
        string owner = rig.ContainsKey("owner") ? rig["owner"].ToString() : "";
        string status = rig.ContainsKey("status") ? rig["status"].ToString() : "warning";

        bool isOverdue = status == "overdue";
        Color borderColor = isOverdue ? Red : Amber;

        // Calculate days
        int daysSincePack = 0;
        if (rig.ContainsKey("lastPacked") && DateTime.TryParse(rig["lastPacked"].ToString(), out DateTime lastPacked))
        {
            daysSincePack = (DateTime.Now - lastPacked).Days;
        }

        var panel = new PanelContainer();
        panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var style = new StyleBoxFlat();
        style.BgColor = White;
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        style.BorderWidthLeft = 5;
        style.BorderColor = borderColor;
        style.ContentMarginLeft = 16;
        style.ContentMarginRight = 16;
        style.ContentMarginTop = 14;
        style.ContentMarginBottom = 14;
        panel.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 8);
        panel.AddChild(vbox);

        // Rig ID + Owner
        var idLabel = new Label();
        idLabel.Text = rigId;
        idLabel.AddThemeFontSizeOverride("font_size", 18);
        idLabel.AddThemeColorOverride("font_color", TextClr);
        vbox.AddChild(idLabel);

        var ownerLabel = new Label();
        ownerLabel.Text = owner;
        ownerLabel.AddThemeFontSizeOverride("font_size", 14);
        ownerLabel.AddThemeColorOverride("font_color", TextSec);
        vbox.AddChild(ownerLabel);

        // Status message
        var statusLabel = new Label();
        if (isOverdue)
        {
            int overdueDays = daysSincePack - GameData.REPACK_CYCLE_DAYS;
            statusLabel.Text = $"OVERDUE by {overdueDays} days";
            statusLabel.AddThemeColorOverride("font_color", Red);
        }
        else
        {
            int daysRemaining = GameData.REPACK_CYCLE_DAYS - daysSincePack;
            statusLabel.Text = $"Repack due in {daysRemaining} days";
            statusLabel.AddThemeColorOverride("font_color", Amber);
        }
        statusLabel.AddThemeFontSizeOverride("font_size", 14);
        vbox.AddChild(statusLabel);

        // Action buttons row
        var btnRow = new HBoxContainer();
        btnRow.AddThemeConstantOverride("separation", 10);
        vbox.AddChild(btnRow);

        var notifyBtn = CreateActionButton("Notify Owner", Amber);
        btnRow.AddChild(notifyBtn);

        var scheduleBtn = CreateActionButton("Schedule Now", Primary);
        btnRow.AddChild(scheduleBtn);

        return panel;
    }

    // ── Action button ───────────────────────────────────────────────────
    private Button CreateActionButton(string text, Color color)
    {
        var btn = new Button();
        btn.Text = text;
        btn.AddThemeFontSizeOverride("font_size", 13);
        btn.AddThemeColorOverride("font_color", White);
        btn.CustomMinimumSize = new Vector2(0, 36);

        var style = new StyleBoxFlat();
        style.BgColor = color;
        style.CornerRadiusTopLeft = 6;
        style.CornerRadiusTopRight = 6;
        style.CornerRadiusBottomLeft = 6;
        style.CornerRadiusBottomRight = 6;
        style.ContentMarginLeft = 14;
        style.ContentMarginRight = 14;
        style.ContentMarginTop = 6;
        style.ContentMarginBottom = 6;
        btn.AddThemeStyleboxOverride("normal", style);
        btn.AddThemeStyleboxOverride("hover", style);
        btn.AddThemeStyleboxOverride("pressed", style);

        return btn;
    }
}
