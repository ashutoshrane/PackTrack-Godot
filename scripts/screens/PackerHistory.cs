using Godot;
using Godot.Collections;

/// <summary>
/// PackTrack - Packer History Screen (C#)
/// Displays the packer's completed pack log with date filtering.
/// </summary>
public partial class PackerHistory : Control
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
    private string _activeFilter = "today";
    private Dictionary<string, Button> _filterButtons = new();
    private VBoxContainer _logListContainer;

    public override void _Ready()
    {
        Name = "PackerHistory";
        SetAnchorsPreset(LayoutPreset.FullRect);

        // ── Background ──────────────────────────────────────────────────
        var bgPanel = new PanelContainer();
        bgPanel.SetAnchorsPreset(LayoutPreset.FullRect);
        var bgStyle = new StyleBoxFlat { BgColor = Bg };
        bgPanel.AddThemeStyleboxOverride("panel", bgStyle);
        AddChild(bgPanel);

        // ── Root layout ─────────────────────────────────────────────────
        var rootVbox = new VBoxContainer();
        rootVbox.SetAnchorsPreset(LayoutPreset.FullRect);
        rootVbox.AddThemeConstantOverride("separation", 0);
        AddChild(rootVbox);

        // ── Header ──────────────────────────────────────────────────────
        var headerPanel = new PanelContainer();
        var headerStyle = new StyleBoxFlat
        {
            BgColor = Primary,
            ContentMarginLeft   = 20,
            ContentMarginRight  = 20,
            ContentMarginTop    = 16,
            ContentMarginBottom = 16,
        };
        headerPanel.AddThemeStyleboxOverride("panel", headerStyle);
        rootVbox.AddChild(headerPanel);

        var headerLabel = new Label();
        headerLabel.Text = "History";
        headerLabel.AddThemeFontSizeOverride("font_size", 24);
        headerLabel.AddThemeColorOverride("font_color", Colors.White);
        headerPanel.AddChild(headerLabel);

        // ── Filter chips ────────────────────────────────────────────────
        var filterMargin = new MarginContainer();
        filterMargin.AddThemeConstantOverride("margin_left",   16);
        filterMargin.AddThemeConstantOverride("margin_right",  16);
        filterMargin.AddThemeConstantOverride("margin_top",    12);
        filterMargin.AddThemeConstantOverride("margin_bottom", 4);
        rootVbox.AddChild(filterMargin);

        var filterHbox = new HBoxContainer();
        filterHbox.AddThemeConstantOverride("separation", 8);
        filterMargin.AddChild(filterHbox);

        string[][] filters =
        {
            new[] { "today", "Today" },
            new[] { "week",  "This Week" },
            new[] { "all",   "All" },
        };

        foreach (var f in filters)
        {
            string key   = f[0];
            string label = f[1];

            var chip = new Button();
            chip.Text = label;
            chip.AddThemeFontSizeOverride("font_size", 13);
            chip.MouseDefaultCursorShape = CursorShape.PointingHand;
            chip.CustomMinimumSize = new Vector2(0, 34);
            chip.Pressed += () => OnFilterPressed(key);
            filterHbox.AddChild(chip);
            _filterButtons[key] = chip;
        }

        UpdateFilterStyles();

        // ── Scrollable log list ─────────────────────────────────────────
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        rootVbox.AddChild(scroll);

        var scrollPadding = new MarginContainer();
        scrollPadding.AddThemeConstantOverride("margin_left",   16);
        scrollPadding.AddThemeConstantOverride("margin_right",  16);
        scrollPadding.AddThemeConstantOverride("margin_top",    8);
        scrollPadding.AddThemeConstantOverride("margin_bottom", 12);
        scrollPadding.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(scrollPadding);

        _logListContainer = new VBoxContainer();
        _logListContainer.AddThemeConstantOverride("separation", 8);
        _logListContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scrollPadding.AddChild(_logListContainer);

        // ── Bottom nav bar ──────────────────────────────────────────────
        BuildNavBar(rootVbox, 1);

        // ── Populate logs ───────────────────────────────────────────────
        PopulateLogs();
    }

    // ── Filter chip styling ──────────────────────────────────────────────

    private void UpdateFilterStyles()
    {
        foreach (var kvp in _filterButtons)
        {
            string key = kvp.Key;
            Button btn = kvp.Value;

            var style = new StyleBoxFlat
            {
                CornerRadiusTopLeft     = 17,
                CornerRadiusTopRight    = 17,
                CornerRadiusBottomLeft  = 17,
                CornerRadiusBottomRight = 17,
                ContentMarginLeft   = 16,
                ContentMarginRight  = 16,
                ContentMarginTop    = 6,
                ContentMarginBottom = 6,
            };

            if (key == _activeFilter)
            {
                style.BgColor = Primary;
                btn.AddThemeColorOverride("font_color", Colors.White);
            }
            else
            {
                style.BgColor = Colors.White;
                style.BorderColor       = new Color(0, 0, 0, 0.15f);
                style.BorderWidthLeft   = 1;
                style.BorderWidthRight  = 1;
                style.BorderWidthTop    = 1;
                style.BorderWidthBottom = 1;
                btn.AddThemeColorOverride("font_color", TextSec);
            }

            btn.AddThemeStyleboxOverride("normal", style);

            var hoverStyle = (StyleBoxFlat)style.Duplicate();
            if (key != _activeFilter)
            {
                hoverStyle.BgColor = new Color(0.95f, 0.95f, 0.95f);
            }
            btn.AddThemeStyleboxOverride("hover", hoverStyle);
        }
    }

    // ── Log population ───────────────────────────────────────────────────

    private void PopulateLogs()
    {
        foreach (var child in _logListContainer.GetChildren())
        {
            child.QueueFree();
        }

        var packLogs = GetFilteredLogs();

        if (packLogs.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "No packs found for this period.";
            emptyLabel.AddThemeFontSizeOverride("font_size", 14);
            emptyLabel.AddThemeColorOverride("font_color", TextSec);
            emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _logListContainer.AddChild(emptyLabel);
            return;
        }

        for (int i = 0; i < packLogs.Count; i++)
        {
            var logEntry = packLogs[i].AsGodotDictionary();
            var card = CreateLogCard(logEntry, i + 1);
            _logListContainer.AddChild(card);
        }
    }

    private PanelContainer CreateLogCard(Dictionary entry, int index)
    {
        var card = new PanelContainer();
        var cardStyle = new StyleBoxFlat
        {
            BgColor = CardColor,
            CornerRadiusTopLeft     = 8,
            CornerRadiusTopRight    = 8,
            CornerRadiusBottomLeft  = 8,
            CornerRadiusBottomRight = 8,
            ShadowColor  = new Color(0, 0, 0, 0.05f),
            ShadowSize   = 2,
            ShadowOffset = new Vector2(0, 1),
            ContentMarginLeft   = 14,
            ContentMarginRight  = 14,
            ContentMarginTop    = 12,
            ContentMarginBottom = 12,
        };
        card.AddThemeStyleboxOverride("panel", cardStyle);
        card.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 12);
        card.AddChild(hbox);

        // Pack number
        var numLabel = new Label();
        numLabel.Text = $"#{index}";
        numLabel.AddThemeFontSizeOverride("font_size", 14);
        numLabel.AddThemeColorOverride("font_color", TextSec);
        numLabel.CustomMinimumSize = new Vector2(32, 0);
        hbox.AddChild(numLabel);

        // Details column
        var detailVbox = new VBoxContainer();
        detailVbox.AddThemeConstantOverride("separation", 2);
        detailVbox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        hbox.AddChild(detailVbox);

        string rigId = entry.ContainsKey("rig_id") ? entry["rig_id"].ToString() : "N/A";
        var rigLabel = new Label();
        rigLabel.Text = rigId;
        rigLabel.AddThemeFontSizeOverride("font_size", 15);
        rigLabel.AddThemeColorOverride("font_color", TextColor);
        detailVbox.AddChild(rigLabel);

        string owner = entry.ContainsKey("skydiver_name") ? entry["skydiver_name"].ToString()
                     : (entry.ContainsKey("owner") ? entry["owner"].ToString() : "N/A");
        var ownerLabel = new Label();
        ownerLabel.Text = owner;
        ownerLabel.AddThemeFontSizeOverride("font_size", 12);
        ownerLabel.AddThemeColorOverride("font_color", TextSec);
        detailVbox.AddChild(ownerLabel);

        // Right side: time + amount
        var rightVbox = new VBoxContainer();
        rightVbox.AddThemeConstantOverride("separation", 2);
        hbox.AddChild(rightVbox);

        string timestamp = entry.ContainsKey("timestamp") ? entry["timestamp"].ToString() : "";
        var timeLabel = new Label();
        timeLabel.Text = FormatTime(timestamp);
        timeLabel.AddThemeFontSizeOverride("font_size", 12);
        timeLabel.AddThemeColorOverride("font_color", TextSec);
        timeLabel.HorizontalAlignment = HorizontalAlignment.Right;
        rightVbox.AddChild(timeLabel);

        float charge = entry.ContainsKey("charge_amount") ? (float)entry["charge_amount"]
                     : (entry.ContainsKey("charge") ? (float)entry["charge"] : 7.00f);
        var amountLabel = new Label();
        amountLabel.Text = $"${charge:F2}";
        amountLabel.AddThemeFontSizeOverride("font_size", 14);
        amountLabel.AddThemeColorOverride("font_color", Green);
        amountLabel.HorizontalAlignment = HorizontalAlignment.Right;
        rightVbox.AddChild(amountLabel);

        return card;
    }

    // ── Filtering ────────────────────────────────────────────────────────

    private Godot.Collections.Array GetFilteredLogs()
    {
        var allLogs = new Godot.Collections.Array();
        string currentPacker = GetCurrentPackerName();

        var gameData = GetNodeOrNull<GameData>("/root/GameData");
        if (gameData != null)
        {
            foreach (var log in gameData.PackLogs)
            {
                string packer = log.ContainsKey("packer_name") ? log["packer_name"].ToString() : "";
                if (packer == currentPacker)
                {
                    allLogs.Add(log);
                }
            }
        }

        if (allLogs.Count == 0)
        {
            allLogs = GetSampleLogs();
        }

        // Apply date filter
        var filtered = new Godot.Collections.Array();

        switch (_activeFilter)
        {
            case "today":
            {
                var todayDict = Time.GetDatetimeDictFromSystem();
                string today = $"{(int)todayDict["year"]:D4}-{(int)todayDict["month"]:D2}-{(int)todayDict["day"]:D2}";
                foreach (var logVar in allLogs)
                {
                    var log = logVar.AsGodotDictionary();
                    string ts = log.ContainsKey("timestamp") ? log["timestamp"].ToString() : "";
                    if (ts.StartsWith(today))
                        filtered.Add(log);
                }
                return filtered;
            }
            case "week":
            {
                long nowUnix = (long)Time.GetUnixTimeFromSystem();
                long weekAgoUnix = nowUnix - (7 * 86400);
                foreach (var logVar in allLogs)
                {
                    var log = logVar.AsGodotDictionary();
                    string ts = log.ContainsKey("timestamp") ? log["timestamp"].ToString() : "";
                    if (ts.Length >= 10)
                    {
                        string dateStr = ts.Length >= 19 ? ts : ts + "T00:00:00";
                        long entryUnix = (long)Time.GetUnixTimeFromDatetimeString(dateStr);
                        if (entryUnix >= weekAgoUnix)
                            filtered.Add(log);
                    }
                }
                return filtered;
            }
            case "all":
            default:
                return allLogs;
        }
    }

    // ── Time formatting ──────────────────────────────────────────────────

    private string FormatTime(string timestamp)
    {
        if (string.IsNullOrEmpty(timestamp)) return "N/A";
        if (timestamp.Length >= 16) return timestamp.Substring(11, 5);
        return timestamp;
    }

    // ── Event handlers ───────────────────────────────────────────────────

    private void OnFilterPressed(string filterKey)
    {
        _activeFilter = filterKey;
        UpdateFilterStyles();
        PopulateLogs();
    }

    private void OnNavTabPressed(string screenName)
    {
        if (screenName == "packer_history") return;

        var nav = GetNodeOrNull<NavManager>("/root/NavManager");
        nav?.NavigateTo(screenName);
    }

    // ── Bottom nav bar ───────────────────────────────────────────────────

    private void BuildNavBar(VBoxContainer parent, int activeIndex)
    {
        var navPanel = new PanelContainer();
        var navStyle = new StyleBoxFlat
        {
            BgColor = Colors.White,
            BorderColor = new Color(0, 0, 0, 0.1f),
            BorderWidthTop = 1,
            ContentMarginTop    = 8,
            ContentMarginBottom = 8,
            ContentMarginLeft   = 8,
            ContentMarginRight  = 8,
        };
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
        }
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
                return currentUser["name"].ToString();
            }
        }
        return "Jake Mitchell";
    }

    // ── Sample data ──────────────────────────────────────────────────────

    private Godot.Collections.Array GetSampleLogs()
    {
        return new Godot.Collections.Array
        {
            new Dictionary
            {
                { "rig_id", "N4521-Main" },
                { "skydiver_name", "Sarah Chen" },
                { "packer_name", "Jake Mitchell" },
                { "timestamp", "2026-04-14T08:12:00" },
                { "charge_amount", 7.00f }
            },
            new Dictionary
            {
                { "rig_id", "N7833-Main" },
                { "skydiver_name", "Marcus Rodriguez" },
                { "packer_name", "Jake Mitchell" },
                { "timestamp", "2026-04-14T08:45:00" },
                { "charge_amount", 7.00f }
            },
            new Dictionary
            {
                { "rig_id", "N5117-Main" },
                { "skydiver_name", "Dylan Park" },
                { "packer_name", "Jake Mitchell" },
                { "timestamp", "2026-04-14T09:20:00" },
                { "charge_amount", 7.00f }
            },
            new Dictionary
            {
                { "rig_id", "N6290-Main" },
                { "skydiver_name", "Emily Foster" },
                { "packer_name", "Jake Mitchell" },
                { "timestamp", "2026-04-13T14:22:00" },
                { "charge_amount", 7.00f }
            },
            new Dictionary
            {
                { "rig_id", "N4521-Main" },
                { "skydiver_name", "Sarah Chen" },
                { "packer_name", "Jake Mitchell" },
                { "timestamp", "2026-04-12T08:55:00" },
                { "charge_amount", 7.00f }
            },
            new Dictionary
            {
                { "rig_id", "N8842-Tandem" },
                { "skydiver_name", "Dylan Park" },
                { "packer_name", "Jake Mitchell" },
                { "timestamp", "2026-04-10T16:30:00" },
                { "charge_amount", 10.00f }
            },
            new Dictionary
            {
                { "rig_id", "N7833-Main" },
                { "skydiver_name", "Marcus Rodriguez" },
                { "packer_name", "Jake Mitchell" },
                { "timestamp", "2026-04-08T12:05:00" },
                { "charge_amount", 7.00f }
            },
        };
    }
}
