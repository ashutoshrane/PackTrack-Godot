using Godot;
using Godot.Collections;

/// <summary>
/// PackTrack - Rig Detail Screen (C#)
/// Shows full details for a specific rig with Pack Complete action.
/// </summary>
public partial class RigDetail : Control
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

    // ── Public property for rig identification ───────────────────────────
    public string RigId { get; set; } = "";

    // ── Internal state ───────────────────────────────────────────────────
    private Dictionary _rigData = new();

    /// <summary>
    /// Called before _Ready to load rig data from GameData.
    /// </summary>
    public void Setup(string rigId)
    {
        RigId = rigId;

        var gameData = GetNodeOrNull<GameData>("/root/GameData");
        if (gameData != null)
        {
            var rig = gameData.GetRigById(rigId);
            if (rig != null && rig.Count > 0)
            {
                _rigData = rig;
                return;
            }
        }

        // Fallback sample data
        _rigData = GetSampleRig(rigId);
    }

    public override void _Ready()
    {
        Name = "RigDetail";
        SetAnchorsPreset(LayoutPreset.FullRect);

        if (_rigData.Count == 0)
        {
            _rigData = GetSampleRig(RigId.Length > 0 ? RigId : "N4521-Main");
        }

        // ── Background ──────────────────────────────────────────────────
        var bgPanel = new PanelContainer();
        bgPanel.SetAnchorsPreset(LayoutPreset.FullRect);
        var bgStyle = new StyleBoxFlat { BgColor = Bg };
        bgPanel.AddThemeStyleboxOverride("panel", bgStyle);
        AddChild(bgPanel);

        // ── Root vertical layout ────────────────────────────────────────
        var rootVbox = new VBoxContainer();
        rootVbox.SetAnchorsPreset(LayoutPreset.FullRect);
        rootVbox.AddThemeConstantOverride("separation", 0);
        AddChild(rootVbox);

        // ── Header ──────────────────────────────────────────────────────
        var headerPanel = new PanelContainer();
        var headerStyle = new StyleBoxFlat
        {
            BgColor = Primary,
            ContentMarginLeft   = 16,
            ContentMarginRight  = 16,
            ContentMarginTop    = 12,
            ContentMarginBottom = 12,
        };
        headerPanel.AddThemeStyleboxOverride("panel", headerStyle);
        rootVbox.AddChild(headerPanel);

        var headerHbox = new HBoxContainer();
        headerHbox.AddThemeConstantOverride("separation", 12);
        headerPanel.AddChild(headerHbox);

        var backBtn = new Button();
        backBtn.Text = "<";
        backBtn.Flat = true;
        backBtn.AddThemeFontSizeOverride("font_size", 20);
        backBtn.AddThemeColorOverride("font_color", Colors.White);
        backBtn.MouseDefaultCursorShape = CursorShape.PointingHand;
        backBtn.Pressed += OnBackPressed;
        headerHbox.AddChild(backBtn);

        var headerTitle = new Label();
        headerTitle.Text = "Rig Detail";
        headerTitle.AddThemeFontSizeOverride("font_size", 20);
        headerTitle.AddThemeColorOverride("font_color", Colors.White);
        headerTitle.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        headerHbox.AddChild(headerTitle);

        // ── Scrollable content ──────────────────────────────────────────
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        rootVbox.AddChild(scroll);

        var contentMargin = new MarginContainer();
        contentMargin.AddThemeConstantOverride("margin_left",   16);
        contentMargin.AddThemeConstantOverride("margin_right",  16);
        contentMargin.AddThemeConstantOverride("margin_top",    20);
        contentMargin.AddThemeConstantOverride("margin_bottom", 20);
        contentMargin.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(contentMargin);

        var contentVbox = new VBoxContainer();
        contentVbox.AddThemeConstantOverride("separation", 16);
        contentVbox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        contentMargin.AddChild(contentVbox);

        // ── Large Rig ID ────────────────────────────────────────────────
        string rigIdText = GetString(_rigData, "rig_id", "Unknown");

        var rigIdDisplay = new Label();
        rigIdDisplay.Text = rigIdText;
        rigIdDisplay.AddThemeFontSizeOverride("font_size", 28);
        rigIdDisplay.AddThemeColorOverride("font_color", TextColor);
        rigIdDisplay.HorizontalAlignment = HorizontalAlignment.Center;
        contentVbox.AddChild(rigIdDisplay);

        // ── Status badge (colored pill) ─────────────────────────────────
        string status = GetString(_rigData, "status", "ok");
        Color statusColor = GetStatusColor(status);

        var badgeCenter = new CenterContainer();
        contentVbox.AddChild(badgeCenter);

        var statusBadge = new PanelContainer();
        var badgeStyle = new StyleBoxFlat
        {
            BgColor = statusColor,
            CornerRadiusTopLeft     = 12,
            CornerRadiusTopRight    = 12,
            CornerRadiusBottomLeft  = 12,
            CornerRadiusBottomRight = 12,
            ContentMarginLeft   = 16,
            ContentMarginRight  = 16,
            ContentMarginTop    = 6,
            ContentMarginBottom = 6,
        };
        statusBadge.AddThemeStyleboxOverride("panel", badgeStyle);
        badgeCenter.AddChild(statusBadge);

        var statusBadgeLabel = new Label();
        statusBadgeLabel.Text = StatusDisplayText(status);
        statusBadgeLabel.AddThemeFontSizeOverride("font_size", 14);
        statusBadgeLabel.AddThemeColorOverride("font_color", Colors.White);
        statusBadge.AddChild(statusBadgeLabel);

        // ── Repack progress card ────────────────────────────────────────
        var progressCard = CreateCard();
        contentVbox.AddChild(progressCard);

        var progressVbox = new VBoxContainer();
        progressVbox.AddThemeConstantOverride("separation", 8);
        progressCard.AddChild(progressVbox);

        var progressTitle = new Label();
        progressTitle.Text = "Repack Cycle";
        progressTitle.AddThemeFontSizeOverride("font_size", 14);
        progressTitle.AddThemeColorOverride("font_color", TextSec);
        progressVbox.AddChild(progressTitle);

        // Calculate days since last pack
        int daysSincePack = CalculateDaysSincePack();
        int repackCycle = 180;

        var repackProgress = new ProgressBar();
        repackProgress.MinValue = 0;
        repackProgress.MaxValue = repackCycle;
        repackProgress.Value = Mathf.Min(daysSincePack, repackCycle);
        repackProgress.ShowPercentage = false;
        repackProgress.CustomMinimumSize = new Vector2(0, 12);

        var progressBg = new StyleBoxFlat
        {
            BgColor = new Color(0.9f, 0.9f, 0.9f),
            CornerRadiusTopLeft     = 6,
            CornerRadiusTopRight    = 6,
            CornerRadiusBottomLeft  = 6,
            CornerRadiusBottomRight = 6,
        };
        repackProgress.AddThemeStyleboxOverride("background", progressBg);

        var progressFill = new StyleBoxFlat
        {
            BgColor = statusColor,
            CornerRadiusTopLeft     = 6,
            CornerRadiusTopRight    = 6,
            CornerRadiusBottomLeft  = 6,
            CornerRadiusBottomRight = 6,
        };
        repackProgress.AddThemeStyleboxOverride("fill", progressFill);

        progressVbox.AddChild(repackProgress);

        var repackLabel = new Label();
        repackLabel.Text = $"{daysSincePack} / {repackCycle} days";
        repackLabel.AddThemeFontSizeOverride("font_size", 12);
        repackLabel.AddThemeColorOverride("font_color", TextSec);
        repackLabel.HorizontalAlignment = HorizontalAlignment.Right;
        progressVbox.AddChild(repackLabel);

        // ── Rig info card ───────────────────────────────────────────────
        var infoCard = CreateCard();
        contentVbox.AddChild(infoCard);

        var infoVbox = new VBoxContainer();
        infoVbox.AddThemeConstantOverride("separation", 10);
        infoCard.AddChild(infoVbox);

        var infoTitle = new Label();
        infoTitle.Text = "Rig Information";
        infoTitle.AddThemeFontSizeOverride("font_size", 16);
        infoTitle.AddThemeColorOverride("font_color", TextColor);
        infoVbox.AddChild(infoTitle);

        AddInfoRow(infoVbox, "Owner",       GetString(_rigData, "owner_name", "N/A"));
        AddInfoRow(infoVbox, "Make / Model", GetString(_rigData, "make_model", "N/A"));
        AddInfoRow(infoVbox, "Serial",       GetString(_rigData, "serial", "N/A"));
        AddInfoRow(infoVbox, "Last Packed",  GetString(_rigData, "last_packed_date", "N/A"));
        AddInfoRow(infoVbox, "Total Packs",  GetInt(_rigData, "total_packs", 0).ToString());

        // ── Recent pack history card ────────────────────────────────────
        var historyCard = CreateCard();
        contentVbox.AddChild(historyCard);

        var historyVbox = new VBoxContainer();
        historyVbox.AddThemeConstantOverride("separation", 8);
        historyCard.AddChild(historyVbox);

        var historyTitle = new Label();
        historyTitle.Text = "Recent Pack History";
        historyTitle.AddThemeFontSizeOverride("font_size", 16);
        historyTitle.AddThemeColorOverride("font_color", TextColor);
        historyVbox.AddChild(historyTitle);

        var recentPacks = GetRecentPackLogs(rigIdText, 4);
        foreach (Dictionary pack in recentPacks)
        {
            var packRow = new HBoxContainer();
            packRow.AddThemeConstantOverride("separation", 8);
            historyVbox.AddChild(packRow);

            var dateLabel = new Label();
            string ts = GetString(pack, "timestamp", "");
            dateLabel.Text = ts.Length >= 10 ? ts.Substring(0, 10) : ts;
            dateLabel.AddThemeFontSizeOverride("font_size", 13);
            dateLabel.AddThemeColorOverride("font_color", TextSec);
            dateLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            packRow.AddChild(dateLabel);

            var packerLabel = new Label();
            packerLabel.Text = GetString(pack, "packer_name", "");
            packerLabel.AddThemeFontSizeOverride("font_size", 13);
            packerLabel.AddThemeColorOverride("font_color", TextColor);
            packRow.AddChild(packerLabel);
        }

        // ── Bottom: Pack Complete button ────────────────────────────────
        var bottomPanel = new PanelContainer();
        var bottomStyle = new StyleBoxFlat
        {
            BgColor = Colors.White,
            BorderColor = new Color(0, 0, 0, 0.08f),
            BorderWidthTop = 1,
            ContentMarginLeft   = 16,
            ContentMarginRight  = 16,
            ContentMarginTop    = 12,
            ContentMarginBottom = 12,
        };
        bottomPanel.AddThemeStyleboxOverride("panel", bottomStyle);
        rootVbox.AddChild(bottomPanel);

        var packBtn = new Button();
        packBtn.Text = "PACK COMPLETE";
        packBtn.CustomMinimumSize = new Vector2(0, 56);
        packBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        packBtn.AddThemeFontSizeOverride("font_size", 18);
        packBtn.AddThemeColorOverride("font_color", Colors.White);
        packBtn.MouseDefaultCursorShape = CursorShape.PointingHand;

        var btnNormal = CreateRoundedStyle(Orange, 12);
        packBtn.AddThemeStyleboxOverride("normal", btnNormal);

        var btnHover = CreateRoundedStyle(Orange.Lightened(0.1f), 12);
        packBtn.AddThemeStyleboxOverride("hover", btnHover);

        var btnPressed = CreateRoundedStyle(Orange.Darkened(0.1f), 12);
        packBtn.AddThemeStyleboxOverride("pressed", btnPressed);

        packBtn.Pressed += OnPackComplete;
        bottomPanel.AddChild(packBtn);
    }

    // ── Card factory ─────────────────────────────────────────────────────

    private PanelContainer CreateCard()
    {
        var card = new PanelContainer();
        var style = new StyleBoxFlat
        {
            BgColor = CardColor,
            CornerRadiusTopLeft     = 10,
            CornerRadiusTopRight    = 10,
            CornerRadiusBottomLeft  = 10,
            CornerRadiusBottomRight = 10,
            ShadowColor  = new Color(0, 0, 0, 0.06f),
            ShadowSize   = 3,
            ShadowOffset = new Vector2(0, 2),
            ContentMarginLeft   = 16,
            ContentMarginRight  = 16,
            ContentMarginTop    = 14,
            ContentMarginBottom = 14,
        };
        card.AddThemeStyleboxOverride("panel", style);
        card.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        return card;
    }

    private static StyleBoxFlat CreateRoundedStyle(Color bgColor, int radius)
    {
        return new StyleBoxFlat
        {
            BgColor = bgColor,
            CornerRadiusTopLeft     = radius,
            CornerRadiusTopRight    = radius,
            CornerRadiusBottomLeft  = radius,
            CornerRadiusBottomRight = radius,
        };
    }

    // ── Info row builder ─────────────────────────────────────────────────

    private void AddInfoRow(VBoxContainer parent, string labelText, string valueText)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 8);
        parent.AddChild(row);

        var lbl = new Label();
        lbl.Text = labelText;
        lbl.AddThemeFontSizeOverride("font_size", 13);
        lbl.AddThemeColorOverride("font_color", TextSec);
        lbl.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        row.AddChild(lbl);

        var val = new Label();
        val.Text = valueText;
        val.AddThemeFontSizeOverride("font_size", 13);
        val.AddThemeColorOverride("font_color", TextColor);
        row.AddChild(val);
    }

    // ── Status helpers ───────────────────────────────────────────────────

    private static Color GetStatusColor(string status)
    {
        return status switch
        {
            "ok" or "green"        => Green,
            "warning" or "amber"   => Amber,
            "overdue" or "red"     => Red,
            _                      => Green,
        };
    }

    private static string StatusDisplayText(string status)
    {
        return status switch
        {
            "ok"      => "Current",
            "warning" => "Due Soon",
            "overdue" => "OVERDUE",
            _         => status,
        };
    }

    // ── Days since last pack calculation ─────────────────────────────────

    private int CalculateDaysSincePack()
    {
        string lastPacked = GetString(_rigData, "last_packed_date", "");
        if (lastPacked.Length < 10) return 0;

        // Use GameData helper if available
        var gameData = GetNodeOrNull<GameData>("/root/GameData");
        if (gameData != null)
        {
            // Use CheckRepackStatus for status info; calculate days from lastPacked directly
        }

        // Manual calculation
        long lastUnix = Time.GetUnixTimeFromDatetimeString($"{lastPacked}T00:00:00");
        var todayDict = Time.GetDatetimeDictFromSystem();
        string todayStr = $"{(int)todayDict["year"]:D4}-{(int)todayDict["month"]:D2}-{(int)todayDict["day"]:D2}";
        long todayUnix = Time.GetUnixTimeFromDatetimeString($"{todayStr}T00:00:00");
        return (int)((todayUnix - lastUnix) / 86400);
    }

    // ── Recent pack logs for this rig ────────────────────────────────────

    private Godot.Collections.Array GetRecentPackLogs(string rigId, int limit)
    {
        var result = new Godot.Collections.Array();

        var gameData = GetNodeOrNull<GameData>("/root/GameData");
        if (gameData != null)
        {
            var allLogs = gameData.PackLogs;
            if (allLogs != null)
            {
                // Collect matching logs (newest first)
                var matching = new Godot.Collections.Array();
                for (int i = allLogs.Count - 1; i >= 0 && matching.Count < limit; i--)
                {
                    var log = allLogs[i];
                    if (log != null && GetString(log, "rig_id", "") == rigId)
                    {
                        matching.Add(log);
                    }
                }
                return matching;
            }
        }

        // Sample fallback
        return new Godot.Collections.Array
        {
            new Dictionary { { "timestamp", "2026-04-14" }, { "packer_name", "Jake Mitchell" } },
            new Dictionary { { "timestamp", "2026-04-12" }, { "packer_name", "Jake Mitchell" } },
            new Dictionary { { "timestamp", "2026-04-08" }, { "packer_name", "Carlos Vega" } },
            new Dictionary { { "timestamp", "2026-04-03" }, { "packer_name", "Jake Mitchell" } },
        };
    }

    // ── Pack complete action ─────────────────────────────────────────────

    private void OnPackComplete()
    {
        string rigId    = GetString(_rigData, "rig_id", "");
        string packer   = GetCurrentPackerName();
        string skydiver = GetString(_rigData, "owner_name", "");
        float amount    = 7.00f;

        var gameData = GetNodeOrNull<GameData>("/root/GameData");
        gameData?.AddPackLog(rigId, packer, skydiver, amount);

        // Navigate to confirmation
        var nav = GetNodeOrNull<NavManager>("/root/NavManager");
        nav?.NavigateTo("pack_confirmation");
    }

    private void OnBackPressed()
    {
        var nav = GetNodeOrNull<NavManager>("/root/NavManager");
        nav?.NavigateTo("packer_queue");
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private string GetCurrentPackerName()
    {
        var gameData = GetNodeOrNull<GameData>("/root/GameData");
        if (gameData != null)
        {
            var currentUser = gameData.CurrentUser;
            if (currentUser != null && currentUser.ContainsKey("name"))
            {
                return (string)currentUser["name"];
            }
        }
        return "Jake Mitchell";
    }

    private static string GetString(Dictionary dict, string key, string fallback)
    {
        return dict.ContainsKey(key) ? dict[key].ToString() : fallback;
    }

    private static int GetInt(Dictionary dict, string key, int fallback)
    {
        return dict.ContainsKey(key) ? (int)dict[key] : fallback;
    }

    // ── Sample data ──────────────────────────────────────────────────────

    private Dictionary GetSampleRig(string rigId)
    {
        return new Dictionary
        {
            { "rig_id", rigId },
            { "owner_name", "Sarah Chen" },
            { "status", "ok" },
            { "last_packed_date", "2026-04-14" },
            { "total_packs", 287 },
            { "make_model", "Javelin J4 / Sabre 170" },
            { "serial", "J4-28451" },
            { "repack_date", "2026-09-15" },
        };
    }
}
