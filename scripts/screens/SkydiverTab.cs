using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Skydiver Tab — "My Tab" screen showing the skydiver's billing summary.
/// Displays a balance card with total owed and unpaid count, an itemized list
/// of charges, paid history, and a fixed "Pay Now" button at the bottom.
/// </summary>
public partial class SkydiverTab : Control
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
        string userName = "";
        if (_gameData.CurrentUser.ContainsKey("name"))
            userName = _gameData.CurrentUser["name"].ToString();

        // Get all charges for this skydiver
        var myCharges = _gameData.PackLogs
            .Where(l => l.ContainsKey("skydiver") && l["skydiver"].ToString() == userName)
            .OrderByDescending(l => l.ContainsKey("date") ? l["date"].ToString() : "")
            .ToList();

        var unpaid = myCharges.Where(l => l.ContainsKey("settled") && !(bool)l["settled"]).ToList();
        var paid = myCharges.Where(l => l.ContainsKey("settled") && (bool)l["settled"]).ToList();

        float totalOwed = unpaid
            .Where(l => l.ContainsKey("amount"))
            .Sum(l => Convert.ToSingle(l["amount"]));

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
        pad.AddThemeConstantOverride("margin_top", 16);
        pad.AddThemeConstantOverride("margin_bottom", 16);
        scroll.AddChild(pad);

        var content = new VBoxContainer();
        content.AddThemeConstantOverride("separation", 16);
        pad.AddChild(content);

        // ── Balance card ────────────────────────────────────────────────
        content.AddChild(CreateBalanceCard(totalOwed, unpaid.Count));

        // ── Unpaid charges ──────────────────────────────────────────────
        if (unpaid.Count > 0)
        {
            var unpaidTitle = new Label();
            unpaidTitle.Text = "Unpaid Charges";
            unpaidTitle.AddThemeFontSizeOverride("font_size", 16);
            unpaidTitle.AddThemeColorOverride("font_color", TextClr);
            content.AddChild(unpaidTitle);

            var unpaidList = new VBoxContainer();
            unpaidList.AddThemeConstantOverride("separation", 8);
            content.AddChild(unpaidList);

            foreach (var charge in unpaid)
            {
                unpaidList.AddChild(CreateChargeRow(charge, false));
            }
        }

        // ── Paid history ────────────────────────────────────────────────
        if (paid.Count > 0)
        {
            var paidTitle = new Label();
            paidTitle.Text = "Paid";
            paidTitle.AddThemeFontSizeOverride("font_size", 16);
            paidTitle.AddThemeColorOverride("font_color", TextClr);
            content.AddChild(paidTitle);

            var paidList = new VBoxContainer();
            paidList.AddThemeConstantOverride("separation", 8);
            content.AddChild(paidList);

            foreach (var charge in paid)
            {
                paidList.AddChild(CreateChargeRow(charge, true));
            }
        }

        // ── Fixed bottom: Pay Now button ────────────────────────────────
        if (unpaid.Count > 0)
        {
            var payPanel = new PanelContainer();
            var payPanelStyle = new StyleBoxFlat();
            payPanelStyle.BgColor = White;
            payPanelStyle.BorderWidthTop = 1;
            payPanelStyle.BorderColor = new Color(0.85f, 0.85f, 0.85f);
            payPanelStyle.ContentMarginLeft = 16;
            payPanelStyle.ContentMarginRight = 16;
            payPanelStyle.ContentMarginTop = 10;
            payPanelStyle.ContentMarginBottom = 10;
            payPanel.AddThemeStyleboxOverride("panel", payPanelStyle);

            var payBtn = new Button();
            payBtn.Text = $"Pay Now — ${totalOwed:F2}";
            payBtn.CustomMinimumSize = new Vector2(0, 56);
            payBtn.AddThemeFontSizeOverride("font_size", 18);
            payBtn.AddThemeColorOverride("font_color", White);
            payBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;

            var payBtnStyle = new StyleBoxFlat();
            payBtnStyle.BgColor = Orange;
            payBtnStyle.CornerRadiusTopLeft = 12;
            payBtnStyle.CornerRadiusTopRight = 12;
            payBtnStyle.CornerRadiusBottomLeft = 12;
            payBtnStyle.CornerRadiusBottomRight = 12;
            payBtnStyle.ContentMarginTop = 12;
            payBtnStyle.ContentMarginBottom = 12;
            payBtn.AddThemeStyleboxOverride("normal", payBtnStyle);
            payBtn.AddThemeStyleboxOverride("hover", payBtnStyle);
            payBtn.AddThemeStyleboxOverride("pressed", payBtnStyle);

            // Capture unpaid list for settlement
            var unpaidIds = unpaid
                .Where(l => l.ContainsKey("id"))
                .Select(l => l["id"].ToString())
                .ToList();

            payBtn.Pressed += () => OnPayPressed(unpaidIds);
            payPanel.AddChild(payBtn);
            rootVbox.AddChild(payPanel);
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
        title.Text = "My Tab";
        title.AddThemeFontSizeOverride("font_size", 22);
        title.AddThemeColorOverride("font_color", White);
        panel.AddChild(title);

        return panel;
    }

    // ── Balance card ────────────────────────────────────────────────────
    private PanelContainer CreateBalanceCard(float totalOwed, int unpaidCount)
    {
        var panel = new PanelContainer();
        panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var style = new StyleBoxFlat();
        style.BgColor = White;
        style.CornerRadiusTopLeft = 12;
        style.CornerRadiusTopRight = 12;
        style.CornerRadiusBottomLeft = 12;
        style.CornerRadiusBottomRight = 12;
        style.ContentMarginLeft = 24;
        style.ContentMarginRight = 24;
        style.ContentMarginTop = 24;
        style.ContentMarginBottom = 24;
        panel.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 8);
        vbox.Alignment = BoxContainer.AlignmentMode.Center;
        panel.AddChild(vbox);

        var balanceLabel = new Label();
        balanceLabel.Text = $"${totalOwed:F2}";
        balanceLabel.AddThemeFontSizeOverride("font_size", 36);
        balanceLabel.AddThemeColorOverride("font_color", totalOwed > 0 ? Orange : Green);
        balanceLabel.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(balanceLabel);

        var countLabel = new Label();
        countLabel.Text = unpaidCount > 0
            ? $"{unpaidCount} unpaid pack{(unpaidCount != 1 ? "s" : "")}"
            : "All settled!";
        countLabel.AddThemeFontSizeOverride("font_size", 14);
        countLabel.AddThemeColorOverride("font_color", TextSec);
        countLabel.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(countLabel);

        return panel;
    }

    // ── Charge row ──────────────────────────────────────────────────────
    private PanelContainer CreateChargeRow(Godot.Collections.Dictionary charge, bool isPaid)
    {
        string rigId = charge.ContainsKey("rigId") ? charge["rigId"].ToString() : "";
        string dateStr = charge.ContainsKey("date") ? charge["date"].ToString() : "";
        float amount = charge.ContainsKey("amount") ? Convert.ToSingle(charge["amount"]) : 0;
        string packer = charge.ContainsKey("packer") ? charge["packer"].ToString() : "";

        var panel = new PanelContainer();
        panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
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
        hbox.AddThemeConstantOverride("separation", 10);
        panel.AddChild(hbox);

        // Paid checkmark or unpaid dot
        if (isPaid)
        {
            var check = new Label();
            check.Text = "✓";
            check.AddThemeFontSizeOverride("font_size", 18);
            check.AddThemeColorOverride("font_color", Green);
            hbox.AddChild(check);
        }
        else
        {
            var dot = new ColorRect();
            dot.CustomMinimumSize = new Vector2(8, 8);
            dot.Color = Orange;
            var dotCenter = new CenterContainer();
            dotCenter.AddChild(dot);
            hbox.AddChild(dotCenter);
        }

        // Details
        var detailVbox = new VBoxContainer();
        detailVbox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        detailVbox.AddThemeConstantOverride("separation", 2);
        hbox.AddChild(detailVbox);

        var rigLabel = new Label();
        rigLabel.Text = $"{rigId} — packed by {packer}";
        rigLabel.AddThemeFontSizeOverride("font_size", 14);
        rigLabel.AddThemeColorOverride("font_color", TextClr);
        detailVbox.AddChild(rigLabel);

        var dateLabel = new Label();
        dateLabel.Text = dateStr;
        dateLabel.AddThemeFontSizeOverride("font_size", 12);
        dateLabel.AddThemeColorOverride("font_color", TextSec);
        detailVbox.AddChild(dateLabel);

        // Amount
        var amountLabel = new Label();
        amountLabel.Text = $"${amount:F2}";
        amountLabel.AddThemeFontSizeOverride("font_size", 16);
        amountLabel.AddThemeColorOverride("font_color", isPaid ? TextSec : Orange);
        hbox.AddChild(amountLabel);

        return panel;
    }

    // ── Pay Now handler ─────────────────────────────────────────────────
    private void OnPayPressed(List<string> logIds)
    {
        foreach (string id in logIds)
        {
            _gameData.SettleCharge(id);
        }

        // Rebuild the UI to reflect the settled charges
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }
        // Defer rebuild to next frame so queue_free completes
        CallDeferred(nameof(BuildUi));
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

            if (i == 2) // "My Tab" is active
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
