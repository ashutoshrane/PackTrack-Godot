using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Operator Dashboard — overview of DZ activity for the drop zone operator.
/// Shows today's stats (packs, revenue, active packers, alerts),
/// a red alert banner if overdue rigs exist, and a live activity feed.
/// </summary>
public partial class OperatorDashboard : Control
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
        // Root background
        var bgRect = new ColorRect();
        bgRect.Color = Bg;
        bgRect.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(bgRect);

        // Main vertical layout
        var rootVbox = new VBoxContainer();
        rootVbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        rootVbox.AddThemeConstantOverride("separation", 0);
        AddChild(rootVbox);

        // ── Header ──────────────────────────────────────────────────────
        rootVbox.AddChild(CreateHeader());

        // ── Scrollable body ─────────────────────────────────────────────
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        rootVbox.AddChild(scroll);

        var body = new VBoxContainer();
        body.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        body.AddThemeConstantOverride("separation", 16);
        scroll.AddChild(body);

        // Content padding wrapper
        var pad = new MarginContainer();
        pad.AddThemeConstantOverride("margin_left", 16);
        pad.AddThemeConstantOverride("margin_right", 16);
        pad.AddThemeConstantOverride("margin_top", 16);
        pad.AddThemeConstantOverride("margin_bottom", 16);
        body.AddChild(pad);

        var content = new VBoxContainer();
        content.AddThemeConstantOverride("separation", 16);
        pad.AddChild(content);

        // ── Alert banner (if overdue rigs exist) ────────────────────────
        var overdueRigs = GetRigsByStatus("overdue");
        if (overdueRigs.Count > 0)
        {
            content.AddChild(CreateAlertBanner(overdueRigs));
        }

        // ── 2x2 stat grid ───────────────────────────────────────────────
        var grid = new GridContainer();
        grid.Columns = 2;
        grid.AddThemeConstantOverride("h_separation", 12);
        grid.AddThemeConstantOverride("v_separation", 12);
        content.AddChild(grid);

        var todayPacks = _gameData.GetPacksForToday();
        float revenue = _gameData.GetTotalEarningsToday();
        int activePackers = CountActivePackers(todayPacks);
        var warningRigs = GetRigsByStatus("warning");
        int alertCount = warningRigs.Count + overdueRigs.Count;

        grid.AddChild(CreateStatCard("Packs Today", todayPacks.Count.ToString(), Orange));
        grid.AddChild(CreateStatCard("Revenue", $"${revenue:F2}", Green));
        grid.AddChild(CreateStatCard("Active Packers", activePackers.ToString(), Primary));
        grid.AddChild(CreateStatCard("Alerts", alertCount.ToString(), alertCount > 0 ? Red : TextSec));

        // ── Activity feed ───────────────────────────────────────────────
        var feedLabel = new Label();
        feedLabel.Text = "Recent Activity";
        feedLabel.AddThemeFontSizeOverride("font_size", 18);
        feedLabel.AddThemeColorOverride("font_color", TextClr);
        content.AddChild(feedLabel);

        var feedContainer = new VBoxContainer();
        feedContainer.AddThemeConstantOverride("separation", 8);
        content.AddChild(feedContainer);

        // Sort logs descending by date, show up to 10
        var sortedLogs = _gameData.PackLogs
            .OrderByDescending(l => l.ContainsKey("date") ? l["date"].ToString() : "")
            .Take(10)
            .ToList();

        foreach (var log in sortedLogs)
        {
            feedContainer.AddChild(CreateFeedItem(log));
        }

        // ── Bottom Nav ──────────────────────────────────────────────────
        rootVbox.AddChild(CreateBottomNav());
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

        var hbox = new HBoxContainer();
        panel.AddChild(hbox);

        var title = new Label();
        title.Text = "Dashboard";
        title.AddThemeFontSizeOverride("font_size", 22);
        title.AddThemeColorOverride("font_color", White);
        title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        hbox.AddChild(title);

        var dateLabel = new Label();
        var dt = Time.GetDatetimeDictFromSystem();
        dateLabel.Text = $"{(int)dt["year"]:D4}-{(int)dt["month"]:D2}-{(int)dt["day"]:D2}";
        dateLabel.AddThemeFontSizeOverride("font_size", 14);
        dateLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1, 0.7f));
        hbox.AddChild(dateLabel);

        return panel;
    }

    // ── Alert banner ────────────────────────────────────────────────────
    private PanelContainer CreateAlertBanner(List<Godot.Collections.Dictionary> overdueRigs)
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat();
        style.BgColor = Red;
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        style.ContentMarginLeft = 16;
        style.ContentMarginRight = 16;
        style.ContentMarginTop = 12;
        style.ContentMarginBottom = 12;
        panel.AddThemeStyleboxOverride("panel", style);

        var rigIds = overdueRigs.Select(r => r.ContainsKey("id") ? r["id"].ToString() : "").ToList();
        var label = new Label();
        label.Text = $"OVERDUE: {overdueRigs.Count} rig(s) need immediate repack — {string.Join(", ", rigIds)}";
        label.AddThemeFontSizeOverride("font_size", 14);
        label.AddThemeColorOverride("font_color", White);
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        panel.AddChild(label);

        return panel;
    }

    // ── Stat card ───────────────────────────────────────────────────────
    private PanelContainer CreateStatCard(string titleText, string valueText, Color accent)
    {
        var panel = new PanelContainer();
        panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var style = new StyleBoxFlat();
        style.BgColor = White;
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        style.BorderWidthTop = 3;
        style.BorderColor = accent;
        style.ContentMarginLeft = 14;
        style.ContentMarginRight = 14;
        style.ContentMarginTop = 14;
        style.ContentMarginBottom = 14;
        panel.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 4);
        panel.AddChild(vbox);

        var valueLabel = new Label();
        valueLabel.Text = valueText;
        valueLabel.AddThemeFontSizeOverride("font_size", 28);
        valueLabel.AddThemeColorOverride("font_color", accent);
        vbox.AddChild(valueLabel);

        var titleLabel = new Label();
        titleLabel.Text = titleText;
        titleLabel.AddThemeFontSizeOverride("font_size", 13);
        titleLabel.AddThemeColorOverride("font_color", TextSec);
        vbox.AddChild(titleLabel);

        return panel;
    }

    // ── Feed item ───────────────────────────────────────────────────────
    private PanelContainer CreateFeedItem(Godot.Collections.Dictionary logEntry)
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat();
        style.BgColor = White;
        style.CornerRadiusTopLeft = 6;
        style.CornerRadiusTopRight = 6;
        style.CornerRadiusBottomLeft = 6;
        style.CornerRadiusBottomRight = 6;
        style.ContentMarginLeft = 12;
        style.ContentMarginRight = 12;
        style.ContentMarginTop = 10;
        style.ContentMarginBottom = 10;
        panel.AddThemeStyleboxOverride("panel", style);

        string dateStr = logEntry.ContainsKey("date") ? logEntry["date"].ToString() : "";
        string timeStr = "";
        if (dateStr.Length >= 16)
        {
            string hourMin = dateStr.Substring(11, 5);
            string[] parts = hourMin.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[0], out int h))
            {
                string ampm = h < 12 ? "AM" : "PM";
                if (h == 0) h = 12;
                else if (h > 12) h -= 12;
                timeStr = $"{h}:{parts[1]} {ampm}";
            }
        }

        string packer = logEntry.ContainsKey("packer") ? logEntry["packer"].ToString() : "";
        string rigId = logEntry.ContainsKey("rigId") ? logEntry["rigId"].ToString() : "";

        var label = new Label();
        label.Text = $"{packer} packed {rigId} · {timeStr}";
        label.AddThemeFontSizeOverride("font_size", 14);
        label.AddThemeColorOverride("font_color", TextClr);
        panel.AddChild(label);

        return panel;
    }

    // ── Bottom nav ──────────────────────────────────────────────────────
    private PanelContainer CreateBottomNav()
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat();
        style.BgColor = White;
        style.BorderWidthTop = 1;
        style.BorderColor = new Color(0.85f, 0.85f, 0.85f);
        style.ContentMarginTop = 8;
        style.ContentMarginBottom = 8;
        panel.AddThemeStyleboxOverride("panel", style);

        var hbox = new HBoxContainer();
        hbox.Alignment = BoxContainer.AlignmentMode.Center;
        hbox.AddThemeConstantOverride("separation", 0);
        panel.AddChild(hbox);

        string[] tabs = { "Dashboard", "Rigs", "Billing", "Settings" };
        string[] screens = { "operator_dashboard", "rig_management", "billing", "settings" };

        for (int i = 0; i < tabs.Length; i++)
        {
            var btn = new Button();
            btn.Text = tabs[i];
            btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            btn.AddThemeFontSizeOverride("font_size", 13);
            btn.Flat = true;

            if (i == 0)
            {
                btn.AddThemeColorOverride("font_color", Primary);
            }
            else
            {
                btn.AddThemeColorOverride("font_color", TextSec);
                string targetScreen = screens[i];
                btn.Pressed += () => _navManager.NavigateTo(targetScreen);
            }
            hbox.AddChild(btn);
        }

        return panel;
    }

    // ── Helpers ──────────────────────────────────────────────────────────
    private List<Godot.Collections.Dictionary> GetRigsByStatus(string status)
    {
        return _gameData.Rigs
            .Where(r => r.ContainsKey("status") && r["status"].ToString() == status)
            .ToList();
    }

    private int CountActivePackers(List<Godot.Collections.Dictionary> todayPacks)
    {
        var packers = new HashSet<string>();
        foreach (var log in todayPacks)
        {
            if (log.ContainsKey("packer"))
                packers.Add(log["packer"].ToString());
        }
        return packers.Count;
    }
}
