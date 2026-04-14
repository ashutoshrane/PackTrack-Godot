using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Rig Profile — detailed view of a single rig.
/// Shows rig ID, status badge, repack countdown progress bar, component list,
/// pack history timeline, and a "Schedule Inspection" button.
/// </summary>
public partial class RigProfile : Control
{
    // ── Colors ───────────────────────────────────────────────────────────
    private static readonly Color Primary   = new Color("#1B3A5C");
    private static readonly Color Orange    = new Color("#E87B35");
    private static readonly Color Green     = new Color("#2D9F5C");
    private static readonly Color Red       = new Color("#D94141");
    private static readonly Color Amber     = new Color("#F5A623");
    private static readonly Color Blue      = new Color("#4A9FD9");
    private static readonly Color Bg        = new Color("#F5F5F5");
    private static readonly Color TextClr   = new Color("#1E1E1E");
    private static readonly Color TextSec   = new Color("#4A4A4A");
    private static readonly Color White     = Colors.White;

    public string RigId { get; set; } = "";

    private GameData _gameData;
    private NavManager _navManager;

    /// <summary>
    /// Called by MainApp when navigating with a compound screen name like "rig_profile:N4521-Main".
    /// </summary>
    public void Setup(string rigId)
    {
        RigId = rigId;
    }

    public override void _Ready()
    {
        _gameData = GetNode<GameData>("/root/GameData");
        _navManager = GetNode<NavManager>("/root/NavManager");
        BuildUi();
    }

    private void BuildUi()
    {
        var rig = _gameData.GetRigById(RigId);
        if (rig == null || rig.Count == 0)
        {
            var errLabel = new Label();
            errLabel.Text = $"Rig '{RigId}' not found.";
            errLabel.AddThemeFontSizeOverride("font_size", 18);
            AddChild(errLabel);
            return;
        }

        string status = rig.ContainsKey("status") ? rig["status"].ToString() : "ok";
        string canopy = rig.ContainsKey("canopy") ? rig["canopy"].ToString() : "";
        string container = rig.ContainsKey("container") ? rig["container"].ToString() : "";
        string owner = rig.ContainsKey("owner") ? rig["owner"].ToString() : "";
        int packCount = rig.ContainsKey("packCount") ? Convert.ToInt32(rig["packCount"]) : 0;

        // Calculate days since last packed
        int daysSincePack = 0;
        if (rig.ContainsKey("lastPacked") && DateTime.TryParse(rig["lastPacked"].ToString(), out DateTime lastPacked))
        {
            daysSincePack = (DateTime.Now - lastPacked).Days;
        }
        int daysRemaining = Math.Max(0, GameData.REPACK_CYCLE_DAYS - daysSincePack);
        float repackFraction = Math.Clamp((float)daysSincePack / GameData.REPACK_CYCLE_DAYS, 0f, 1f);

        // Root background
        var bgRect = new ColorRect();
        bgRect.Color = Bg;
        bgRect.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(bgRect);

        var rootVbox = new VBoxContainer();
        rootVbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        rootVbox.AddThemeConstantOverride("separation", 0);
        AddChild(rootVbox);

        // ── Header with back button ─────────────────────────────────────
        rootVbox.AddChild(CreateHeader());

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
        content.AddThemeConstantOverride("separation", 20);
        pad.AddChild(content);

        // ── Large Rig ID + Status badge ─────────────────────────────────
        var idSection = new VBoxContainer();
        idSection.AddThemeConstantOverride("separation", 8);
        content.AddChild(idSection);

        var rigIdLabel = new Label();
        rigIdLabel.Text = RigId;
        rigIdLabel.AddThemeFontSizeOverride("font_size", 28);
        rigIdLabel.AddThemeColorOverride("font_color", TextClr);
        idSection.AddChild(rigIdLabel);

        var ownerLabel = new Label();
        ownerLabel.Text = $"Owner: {owner}";
        ownerLabel.AddThemeFontSizeOverride("font_size", 14);
        ownerLabel.AddThemeColorOverride("font_color", TextSec);
        idSection.AddChild(ownerLabel);

        idSection.AddChild(CreateStatusBadge(status));

        // ── Repack countdown progress bar ───────────────────────────────
        var repackSection = new VBoxContainer();
        repackSection.AddThemeConstantOverride("separation", 8);
        content.AddChild(repackSection);

        var repackTitle = new Label();
        repackTitle.Text = "Repack Cycle";
        repackTitle.AddThemeFontSizeOverride("font_size", 16);
        repackTitle.AddThemeColorOverride("font_color", TextClr);
        repackSection.AddChild(repackTitle);

        var progressBar = new ProgressBar();
        progressBar.MinValue = 0;
        progressBar.MaxValue = 1;
        progressBar.Value = repackFraction;
        progressBar.CustomMinimumSize = new Vector2(0, 20);
        progressBar.ShowPercentage = false;

        // Color the progress bar based on status
        var fillStyle = new StyleBoxFlat();
        fillStyle.BgColor = status == "overdue" ? Red : status == "warning" ? Amber : Green;
        fillStyle.CornerRadiusTopLeft = 4;
        fillStyle.CornerRadiusTopRight = 4;
        fillStyle.CornerRadiusBottomLeft = 4;
        fillStyle.CornerRadiusBottomRight = 4;
        progressBar.AddThemeStyleboxOverride("fill", fillStyle);

        var bgStyle = new StyleBoxFlat();
        bgStyle.BgColor = new Color(0.9f, 0.9f, 0.9f);
        bgStyle.CornerRadiusTopLeft = 4;
        bgStyle.CornerRadiusTopRight = 4;
        bgStyle.CornerRadiusBottomLeft = 4;
        bgStyle.CornerRadiusBottomRight = 4;
        progressBar.AddThemeStyleboxOverride("background", bgStyle);
        repackSection.AddChild(progressBar);

        var daysLabel = new Label();
        if (status == "overdue")
        {
            int overdueDays = daysSincePack - GameData.REPACK_CYCLE_DAYS;
            daysLabel.Text = $"OVERDUE by {overdueDays} days";
            daysLabel.AddThemeColorOverride("font_color", Red);
        }
        else
        {
            daysLabel.Text = $"{daysRemaining} of {GameData.REPACK_CYCLE_DAYS} days remaining";
            daysLabel.AddThemeColorOverride("font_color", TextSec);
        }
        daysLabel.AddThemeFontSizeOverride("font_size", 13);
        repackSection.AddChild(daysLabel);

        // ── Components list ─────────────────────────────────────────────
        var compSection = new VBoxContainer();
        compSection.AddThemeConstantOverride("separation", 8);
        content.AddChild(compSection);

        var compTitle = new Label();
        compTitle.Text = "Components";
        compTitle.AddThemeFontSizeOverride("font_size", 16);
        compTitle.AddThemeColorOverride("font_color", TextClr);
        compSection.AddChild(compTitle);

        string rigType = rig.ContainsKey("type") ? rig["type"].ToString() : "main";
        // Derive components from canopy, container, and type
        var components = new List<(string label, string value)>
        {
            ("Main Canopy", rigType == "reserve" ? "N/A" : canopy),
            ("Reserve", rigType == "reserve" ? canopy : FindReserveCanopy(owner)),
            ("AAD", "Cypres 2"),
            ("Container", container)
        };

        foreach (var (compLabel, compValue) in components)
        {
            compSection.AddChild(CreateComponentRow(compLabel, compValue));
        }

        // ── Pack history timeline (last 5 packs) ────────────────────────
        var histSection = new VBoxContainer();
        histSection.AddThemeConstantOverride("separation", 8);
        content.AddChild(histSection);

        var histTitle = new Label();
        histTitle.Text = "Pack History";
        histTitle.AddThemeFontSizeOverride("font_size", 16);
        histTitle.AddThemeColorOverride("font_color", TextClr);
        histSection.AddChild(histTitle);

        var logs = _gameData.PackLogs
            .Where(l => l.ContainsKey("rigId") && l["rigId"].ToString() == RigId)
            .OrderByDescending(l => l.ContainsKey("date") ? l["date"].ToString() : "")
            .Take(5)
            .ToList();

        if (logs.Count == 0)
        {
            var noLogs = new Label();
            noLogs.Text = "No pack history recorded.";
            noLogs.AddThemeFontSizeOverride("font_size", 13);
            noLogs.AddThemeColorOverride("font_color", TextSec);
            histSection.AddChild(noLogs);
        }
        else
        {
            foreach (var log in logs)
            {
                histSection.AddChild(CreateTimelineEntry(log));
            }
        }

        // ── Schedule Inspection button ──────────────────────────────────
        var inspectBtn = new Button();
        inspectBtn.Text = "Schedule Inspection";
        inspectBtn.CustomMinimumSize = new Vector2(0, 48);
        inspectBtn.AddThemeFontSizeOverride("font_size", 16);
        inspectBtn.AddThemeColorOverride("font_color", White);

        var btnStyle = new StyleBoxFlat();
        btnStyle.BgColor = Blue;
        btnStyle.CornerRadiusTopLeft = 10;
        btnStyle.CornerRadiusTopRight = 10;
        btnStyle.CornerRadiusBottomLeft = 10;
        btnStyle.CornerRadiusBottomRight = 10;
        btnStyle.ContentMarginTop = 12;
        btnStyle.ContentMarginBottom = 12;
        inspectBtn.AddThemeStyleboxOverride("normal", btnStyle);
        inspectBtn.AddThemeStyleboxOverride("hover", btnStyle);
        inspectBtn.AddThemeStyleboxOverride("pressed", btnStyle);
        content.AddChild(inspectBtn);
    }

    // ── Header with back button ─────────────────────────────────────────
    private PanelContainer CreateHeader()
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat();
        style.BgColor = Primary;
        style.ContentMarginLeft = 8;
        style.ContentMarginRight = 16;
        style.ContentMarginTop = 10;
        style.ContentMarginBottom = 10;
        panel.AddThemeStyleboxOverride("panel", style);

        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 8);
        panel.AddChild(hbox);

        var backBtn = new Button();
        backBtn.Text = "<";
        backBtn.Flat = true;
        backBtn.AddThemeFontSizeOverride("font_size", 20);
        backBtn.AddThemeColorOverride("font_color", White);
        backBtn.Pressed += () => _navManager.GoBack();
        hbox.AddChild(backBtn);

        var title = new Label();
        title.Text = "Rig Profile";
        title.AddThemeFontSizeOverride("font_size", 20);
        title.AddThemeColorOverride("font_color", White);
        title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        hbox.AddChild(title);

        return panel;
    }

    // ── Status badge pill ───────────────────────────────────────────────
    private PanelContainer CreateStatusBadge(string status)
    {
        Color badgeColor;
        string badgeText;
        switch (status)
        {
            case "warning":
                badgeColor = Amber;
                badgeText = "WARNING";
                break;
            case "overdue":
                badgeColor = Red;
                badgeText = "OVERDUE";
                break;
            default:
                badgeColor = Green;
                badgeText = "OK";
                break;
        }

        var panel = new PanelContainer();
        var style = new StyleBoxFlat();
        style.BgColor = badgeColor;
        style.CornerRadiusTopLeft = 12;
        style.CornerRadiusTopRight = 12;
        style.CornerRadiusBottomLeft = 12;
        style.CornerRadiusBottomRight = 12;
        style.ContentMarginLeft = 14;
        style.ContentMarginRight = 14;
        style.ContentMarginTop = 4;
        style.ContentMarginBottom = 4;
        panel.AddThemeStyleboxOverride("panel", style);
        panel.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;

        var label = new Label();
        label.Text = badgeText;
        label.AddThemeFontSizeOverride("font_size", 13);
        label.AddThemeColorOverride("font_color", White);
        panel.AddChild(label);

        return panel;
    }

    // ── Component row ───────────────────────────────────────────────────
    private PanelContainer CreateComponentRow(string compLabel, string compValue)
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

        var hbox = new HBoxContainer();
        panel.AddChild(hbox);

        var nameLabel = new Label();
        nameLabel.Text = compLabel;
        nameLabel.AddThemeFontSizeOverride("font_size", 14);
        nameLabel.AddThemeColorOverride("font_color", TextSec);
        nameLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        hbox.AddChild(nameLabel);

        var valLabel = new Label();
        valLabel.Text = compValue;
        valLabel.AddThemeFontSizeOverride("font_size", 14);
        valLabel.AddThemeColorOverride("font_color", TextClr);
        hbox.AddChild(valLabel);

        return panel;
    }

    // ── Timeline entry ──────────────────────────────────────────────────
    private PanelContainer CreateTimelineEntry(Godot.Collections.Dictionary log)
    {
        string packer = log.ContainsKey("packer") ? log["packer"].ToString() : "";
        string dateStr = log.ContainsKey("date") ? log["date"].ToString() : "";

        var panel = new PanelContainer();
        var style = new StyleBoxFlat();
        style.BgColor = White;
        style.BorderWidthLeft = 3;
        style.BorderColor = Primary;
        style.CornerRadiusTopLeft = 4;
        style.CornerRadiusTopRight = 4;
        style.CornerRadiusBottomLeft = 4;
        style.CornerRadiusBottomRight = 4;
        style.ContentMarginLeft = 14;
        style.ContentMarginRight = 12;
        style.ContentMarginTop = 10;
        style.ContentMarginBottom = 10;
        panel.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 2);
        panel.AddChild(vbox);

        var packerLabel = new Label();
        packerLabel.Text = $"Packed by {packer}";
        packerLabel.AddThemeFontSizeOverride("font_size", 14);
        packerLabel.AddThemeColorOverride("font_color", TextClr);
        vbox.AddChild(packerLabel);

        var dateLabel = new Label();
        dateLabel.Text = dateStr;
        dateLabel.AddThemeFontSizeOverride("font_size", 12);
        dateLabel.AddThemeColorOverride("font_color", TextSec);
        vbox.AddChild(dateLabel);

        return panel;
    }

    // ── Helpers ──────────────────────────────────────────────────────────
    private string FindReserveCanopy(string ownerName)
    {
        foreach (var rig in _gameData.Rigs)
        {
            if (rig.ContainsKey("owner") && rig["owner"].ToString() == ownerName
                && rig.ContainsKey("type") && rig["type"].ToString() == "reserve"
                && rig.ContainsKey("canopy"))
            {
                return rig["canopy"].ToString();
            }
        }
        return "N/A";
    }
}
