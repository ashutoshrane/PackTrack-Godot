using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Skydiver Rig — the skydiver's "My Rig" home screen.
/// Shows a large rig card with ID, make/model, status badge, repack progress bar,
/// last packed info, total packs, a "View Full History" button, and bottom nav.
/// </summary>
public partial class SkydiverRig : Control
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
        // Find this skydiver's main rig by matching CurrentUser name to rig owner
        string userName = "";
        if (_gameData.CurrentUser.ContainsKey("name"))
            userName = _gameData.CurrentUser["name"].ToString();

        Godot.Collections.Dictionary myRig = null;
        foreach (var rig in _gameData.Rigs)
        {
            if (rig.ContainsKey("owner") && rig["owner"].ToString() == userName
                && rig.ContainsKey("type") && rig["type"].ToString() == "main")
            {
                myRig = rig;
                break;
            }
        }

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

        // ── Scrollable body ─────────────────────────────────────────────
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        rootVbox.AddChild(scroll);

        var pad = new MarginContainer();
        pad.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        pad.AddThemeConstantOverride("margin_left", 16);
        pad.AddThemeConstantOverride("margin_right", 16);
        pad.AddThemeConstantOverride("margin_top", 20);
        pad.AddThemeConstantOverride("margin_bottom", 20);
        scroll.AddChild(pad);

        var content = new VBoxContainer();
        content.AddThemeConstantOverride("separation", 20);
        pad.AddChild(content);

        if (myRig == null || myRig.Count == 0)
        {
            var noRig = new Label();
            noRig.Text = "No rig found for your account.";
            noRig.AddThemeFontSizeOverride("font_size", 16);
            noRig.AddThemeColorOverride("font_color", TextSec);
            noRig.HorizontalAlignment = HorizontalAlignment.Center;
            content.AddChild(noRig);
        }
        else
        {
            content.AddChild(CreateRigCard(myRig));

            // View Full History button
            var histBtn = new Button();
            histBtn.Text = "View Full History";
            histBtn.CustomMinimumSize = new Vector2(0, 48);
            histBtn.AddThemeFontSizeOverride("font_size", 16);
            histBtn.AddThemeColorOverride("font_color", White);

            var btnStyle = new StyleBoxFlat();
            btnStyle.BgColor = Primary;
            btnStyle.CornerRadiusTopLeft = 10;
            btnStyle.CornerRadiusTopRight = 10;
            btnStyle.CornerRadiusBottomLeft = 10;
            btnStyle.CornerRadiusBottomRight = 10;
            btnStyle.ContentMarginTop = 12;
            btnStyle.ContentMarginBottom = 12;
            histBtn.AddThemeStyleboxOverride("normal", btnStyle);
            histBtn.AddThemeStyleboxOverride("hover", btnStyle);
            histBtn.AddThemeStyleboxOverride("pressed", btnStyle);

            string rigId = myRig.ContainsKey("id") ? myRig["id"].ToString() : "";
            histBtn.Pressed += () => _navManager.NavigateTo($"rig_profile:{rigId}");
            content.AddChild(histBtn);
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

        var title = new Label();
        title.Text = "My Rig";
        title.AddThemeFontSizeOverride("font_size", 22);
        title.AddThemeColorOverride("font_color", White);
        panel.AddChild(title);

        return panel;
    }

    // ── Large rig card ──────────────────────────────────────────────────
    private PanelContainer CreateRigCard(Godot.Collections.Dictionary rig)
    {
        string rigId = rig.ContainsKey("id") ? rig["id"].ToString() : "";
        string canopy = rig.ContainsKey("canopy") ? rig["canopy"].ToString() : "";
        string container = rig.ContainsKey("container") ? rig["container"].ToString() : "";
        string makeModel = $"{canopy} / {container}";
        string status = rig.ContainsKey("status") ? rig["status"].ToString() : "ok";
        int packCount = rig.ContainsKey("packCount") ? Convert.ToInt32(rig["packCount"]) : 0;

        // Days calculation
        int daysSincePack = 0;
        string lastPackedStr = "";
        if (rig.ContainsKey("lastPacked"))
        {
            lastPackedStr = rig["lastPacked"].ToString();
            if (DateTime.TryParse(lastPackedStr, out DateTime lastPacked))
            {
                daysSincePack = (DateTime.Now - lastPacked).Days;
            }
        }
        int daysRemaining = Math.Max(0, GameData.REPACK_CYCLE_DAYS - daysSincePack);
        float repackFraction = Math.Clamp((float)daysSincePack / GameData.REPACK_CYCLE_DAYS, 0f, 1f);

        // Find who packed it last
        string lastPackedBy = "";
        var lastLog = _gameData.PackLogs
            .Where(l => l.ContainsKey("rigId") && l["rigId"].ToString() == rigId)
            .OrderByDescending(l => l.ContainsKey("date") ? l["date"].ToString() : "")
            .FirstOrDefault();
        if (lastLog != null && lastLog.ContainsKey("packer"))
            lastPackedBy = lastLog["packer"].ToString();

        var panel = new PanelContainer();
        panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var cardStyle = new StyleBoxFlat();
        cardStyle.BgColor = White;
        cardStyle.CornerRadiusTopLeft = 12;
        cardStyle.CornerRadiusTopRight = 12;
        cardStyle.CornerRadiusBottomLeft = 12;
        cardStyle.CornerRadiusBottomRight = 12;
        cardStyle.ContentMarginLeft = 20;
        cardStyle.ContentMarginRight = 20;
        cardStyle.ContentMarginTop = 20;
        cardStyle.ContentMarginBottom = 20;
        panel.AddThemeStyleboxOverride("panel", cardStyle);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 12);
        panel.AddChild(vbox);

        // Large rig ID
        var idLabel = new Label();
        idLabel.Text = rigId;
        idLabel.AddThemeFontSizeOverride("font_size", 28);
        idLabel.AddThemeColorOverride("font_color", TextClr);
        vbox.AddChild(idLabel);

        // Make/model
        var modelLabel = new Label();
        modelLabel.Text = makeModel;
        modelLabel.AddThemeFontSizeOverride("font_size", 15);
        modelLabel.AddThemeColorOverride("font_color", TextSec);
        vbox.AddChild(modelLabel);

        // Status badge
        vbox.AddChild(CreateStatusBadge(status));

        // Repack progress bar
        var progressSection = new VBoxContainer();
        progressSection.AddThemeConstantOverride("separation", 6);
        vbox.AddChild(progressSection);

        var progressBar = new ProgressBar();
        progressBar.MinValue = 0;
        progressBar.MaxValue = 1;
        progressBar.Value = repackFraction;
        progressBar.CustomMinimumSize = new Vector2(0, 16);
        progressBar.ShowPercentage = false;

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
        progressSection.AddChild(progressBar);

        var daysLabel = new Label();
        if (status == "overdue")
        {
            int overdueDays = daysSincePack - GameData.REPACK_CYCLE_DAYS;
            daysLabel.Text = $"OVERDUE by {overdueDays} days";
            daysLabel.AddThemeColorOverride("font_color", Red);
        }
        else
        {
            daysLabel.Text = $"{daysRemaining} days until repack";
            daysLabel.AddThemeColorOverride("font_color", TextSec);
        }
        daysLabel.AddThemeFontSizeOverride("font_size", 13);
        progressSection.AddChild(daysLabel);

        // Last packed info
        var lastPackedLabel = new Label();
        lastPackedLabel.Text = $"Last packed: {lastPackedStr}";
        if (!string.IsNullOrEmpty(lastPackedBy))
            lastPackedLabel.Text += $" by {lastPackedBy}";
        lastPackedLabel.AddThemeFontSizeOverride("font_size", 13);
        lastPackedLabel.AddThemeColorOverride("font_color", TextSec);
        lastPackedLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        vbox.AddChild(lastPackedLabel);

        // Total packs
        var totalLabel = new Label();
        totalLabel.Text = $"Total packs: {packCount}";
        totalLabel.AddThemeFontSizeOverride("font_size", 13);
        totalLabel.AddThemeColorOverride("font_color", TextSec);
        vbox.AddChild(totalLabel);

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

        string[] tabs = { "My Rig", "History", "My Tab", "Profile" };
        string[] screens = { "skydiver_rig", "packer_history", "skydiver_tab", "settings" };

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
}
