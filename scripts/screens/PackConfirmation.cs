using Godot;
using Godot.Collections;

/// <summary>
/// PackTrack - Pack Confirmation Screen (C#)
/// Success screen shown after a parachute pack is logged.
/// </summary>
public partial class PackConfirmation : Control
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

    // ── State ────────────────────────────────────────────────────────────
    private Dictionary _packData = new();
    private Timer _autoReturnTimer;

    /// <summary>
    /// Called before _Ready to set the pack data for display.
    /// </summary>
    public void Setup(Dictionary data)
    {
        _packData = data ?? new Dictionary();
    }

    public override void _Ready()
    {
        Name = "PackConfirmation";
        SetAnchorsPreset(LayoutPreset.FullRect);

        if (_packData.Count == 0)
        {
            _packData = GetSamplePackData();
        }

        // ── Background ──────────────────────────────────────────────────
        var bgPanel = new PanelContainer();
        bgPanel.SetAnchorsPreset(LayoutPreset.FullRect);
        var bgStyle = new StyleBoxFlat { BgColor = Bg };
        bgPanel.AddThemeStyleboxOverride("panel", bgStyle);
        AddChild(bgPanel);

        // ── Centered layout ─────────────────────────────────────────────
        var center = new CenterContainer();
        center.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(center);

        var contentVbox = new VBoxContainer();
        contentVbox.AddThemeConstantOverride("separation", 24);
        contentVbox.Alignment = BoxContainer.AlignmentMode.Center;
        contentVbox.CustomMinimumSize = new Vector2(320, 0);
        center.AddChild(contentVbox);

        // ── Green checkmark circle ──────────────────────────────────────
        var checkCenter = new CenterContainer();
        contentVbox.AddChild(checkCenter);

        var checkCircle = new PanelContainer();
        var circleStyle = new StyleBoxFlat
        {
            BgColor = Green,
            CornerRadiusTopLeft     = 48,
            CornerRadiusTopRight    = 48,
            CornerRadiusBottomLeft  = 48,
            CornerRadiusBottomRight = 48,
            ContentMarginLeft   = 24,
            ContentMarginRight  = 24,
            ContentMarginTop    = 16,
            ContentMarginBottom = 16,
        };
        checkCircle.AddThemeStyleboxOverride("panel", circleStyle);
        checkCircle.CustomMinimumSize = new Vector2(96, 96);
        checkCenter.AddChild(checkCircle);

        var checkInnerCenter = new CenterContainer();
        checkCircle.AddChild(checkInnerCenter);

        var checkLabel = new Label();
        checkLabel.Text = "\u2713";
        checkLabel.AddThemeFontSizeOverride("font_size", 48);
        checkLabel.AddThemeColorOverride("font_color", Colors.White);
        checkLabel.HorizontalAlignment = HorizontalAlignment.Center;
        checkLabel.VerticalAlignment = VerticalAlignment.Center;
        checkInnerCenter.AddChild(checkLabel);

        // ── "Pack Logged!" title ────────────────────────────────────────
        var titleLabel = new Label();
        titleLabel.Text = "Pack Logged!";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleLabel.AddThemeColorOverride("font_color", TextColor);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        contentVbox.AddChild(titleLabel);

        // ── Summary card ────────────────────────────────────────────────
        var summaryCard = new PanelContainer();
        var cardStyle = new StyleBoxFlat
        {
            BgColor = CardColor,
            CornerRadiusTopLeft     = 12,
            CornerRadiusTopRight    = 12,
            CornerRadiusBottomLeft  = 12,
            CornerRadiusBottomRight = 12,
            ShadowColor  = new Color(0, 0, 0, 0.06f),
            ShadowSize   = 3,
            ShadowOffset = new Vector2(0, 2),
            ContentMarginLeft   = 20,
            ContentMarginRight  = 20,
            ContentMarginTop    = 16,
            ContentMarginBottom = 16,
        };
        summaryCard.AddThemeStyleboxOverride("panel", cardStyle);
        contentVbox.AddChild(summaryCard);

        var summaryVbox = new VBoxContainer();
        summaryVbox.AddThemeConstantOverride("separation", 12);
        summaryCard.AddChild(summaryVbox);

        string rigId = GetStr("rig_id", "N/A");
        string packer = GetStr("packer_name", GetStr("packer", "N/A"));
        string timestamp = GetStr("timestamp", "");

        AddSummaryRow(summaryVbox, "Rig", rigId);
        AddSummaryRow(summaryVbox, "Packer", packer);
        AddSummaryRow(summaryVbox, "Time", FormatTime(timestamp));

        // Charge row with highlight
        var chargeRow = new HBoxContainer();
        chargeRow.AddThemeConstantOverride("separation", 8);
        summaryVbox.AddChild(chargeRow);

        var chargeLbl = new Label();
        chargeLbl.Text = "Charge";
        chargeLbl.AddThemeFontSizeOverride("font_size", 14);
        chargeLbl.AddThemeColorOverride("font_color", TextSec);
        chargeLbl.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        chargeRow.AddChild(chargeLbl);

        float chargeAmount = _packData.ContainsKey("charge_amount")
            ? (float)_packData["charge_amount"]
            : (_packData.ContainsKey("charge") ? (float)_packData["charge"] : 7.00f);
        string skydiverName = GetStr("skydiver_name", GetStr("owner", "owner"));

        var chargeVal = new Label();
        chargeVal.Text = $"${chargeAmount:F2} added to {skydiverName}'s tab";
        chargeVal.AddThemeFontSizeOverride("font_size", 14);
        chargeVal.AddThemeColorOverride("font_color", Orange);
        chargeRow.AddChild(chargeVal);

        // ── "Back to Queue" button ──────────────────────────────────────
        var btnCenter = new CenterContainer();
        contentVbox.AddChild(btnCenter);

        var backBtn = new Button();
        backBtn.Text = "Back to Queue";
        backBtn.CustomMinimumSize = new Vector2(220, 48);
        backBtn.AddThemeFontSizeOverride("font_size", 16);
        backBtn.AddThemeColorOverride("font_color", Primary);
        backBtn.MouseDefaultCursorShape = CursorShape.PointingHand;

        var btnNormal = new StyleBoxFlat
        {
            BgColor = Colors.Transparent,
            BorderColor = Primary,
            BorderWidthLeft   = 2,
            BorderWidthRight  = 2,
            BorderWidthTop    = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft     = 10,
            CornerRadiusTopRight    = 10,
            CornerRadiusBottomLeft  = 10,
            CornerRadiusBottomRight = 10,
        };
        backBtn.AddThemeStyleboxOverride("normal", btnNormal);

        var btnHover = new StyleBoxFlat
        {
            BgColor = Primary.Lightened(0.9f),
            BorderColor = Primary,
            BorderWidthLeft   = 2,
            BorderWidthRight  = 2,
            BorderWidthTop    = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft     = 10,
            CornerRadiusTopRight    = 10,
            CornerRadiusBottomLeft  = 10,
            CornerRadiusBottomRight = 10,
        };
        backBtn.AddThemeStyleboxOverride("hover", btnHover);

        backBtn.Pressed += OnBackToQueue;
        btnCenter.AddChild(backBtn);

        // ── Auto-return timer (3 seconds) ───────────────────────────────
        _autoReturnTimer = new Timer();
        _autoReturnTimer.WaitTime = 3.0;
        _autoReturnTimer.OneShot = true;
        _autoReturnTimer.Timeout += OnBackToQueue;
        AddChild(_autoReturnTimer);
        _autoReturnTimer.Start();
    }

    // ── Summary row builder ──────────────────────────────────────────────

    private void AddSummaryRow(VBoxContainer parent, string labelText, string valueText)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 8);
        parent.AddChild(row);

        var lbl = new Label();
        lbl.Text = labelText;
        lbl.AddThemeFontSizeOverride("font_size", 14);
        lbl.AddThemeColorOverride("font_color", TextSec);
        lbl.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        row.AddChild(lbl);

        var val = new Label();
        val.Text = valueText;
        val.AddThemeFontSizeOverride("font_size", 14);
        val.AddThemeColorOverride("font_color", TextColor);
        row.AddChild(val);
    }

    // ── Time formatting ──────────────────────────────────────────────────

    private string FormatTime(string timestamp)
    {
        if (string.IsNullOrEmpty(timestamp))
            return Time.GetTimeStringFromSystem();
        if (timestamp.Length >= 19)
            return timestamp.Substring(11, 8);
        if (timestamp.Length >= 16)
            return timestamp.Substring(11, 5);
        return timestamp;
    }

    // ── Navigation ───────────────────────────────────────────────────────

    private void OnBackToQueue()
    {
        if (_autoReturnTimer != null && !_autoReturnTimer.IsStopped())
        {
            _autoReturnTimer.Stop();
        }

        var nav = GetNodeOrNull<NavManager>("/root/NavManager");
        nav?.NavigateTo("packer_queue");
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private string GetStr(string key, string fallback)
    {
        return _packData.ContainsKey(key) ? _packData[key].ToString() : fallback;
    }

    // ── Sample data ──────────────────────────────────────────────────────

    private Dictionary GetSamplePackData()
    {
        return new Dictionary
        {
            { "rig_id", "N4521-Main" },
            { "packer_name", "Jake Mitchell" },
            { "timestamp", Time.GetDatetimeStringFromSystem() },
            { "charge_amount", 7.00f },
            { "skydiver_name", "Sarah Chen" },
        };
    }
}
