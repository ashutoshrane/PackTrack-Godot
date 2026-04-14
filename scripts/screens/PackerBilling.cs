using Godot;
using Godot.Collections;

/// <summary>
/// PackTrack - Packer Billing Screen (C#)
/// Shows the packer's earnings summary and per-skydiver billing status.
/// </summary>
public partial class PackerBilling : Control
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
    private VBoxContainer _skydiverListContainer;

    public override void _Ready()
    {
        Name = "PackerBilling";
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
        headerLabel.Text = "Billing";
        headerLabel.AddThemeFontSizeOverride("font_size", 24);
        headerLabel.AddThemeColorOverride("font_color", Colors.White);
        headerPanel.AddChild(headerLabel);

        // ── Scrollable content ──────────────────────────────────────────
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        rootVbox.AddChild(scroll);

        var scrollPadding = new MarginContainer();
        scrollPadding.AddThemeConstantOverride("margin_left",   16);
        scrollPadding.AddThemeConstantOverride("margin_right",  16);
        scrollPadding.AddThemeConstantOverride("margin_top",    16);
        scrollPadding.AddThemeConstantOverride("margin_bottom", 16);
        scrollPadding.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(scrollPadding);

        var contentVbox = new VBoxContainer();
        contentVbox.AddThemeConstantOverride("separation", 16);
        contentVbox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scrollPadding.AddChild(contentVbox);

        // ── Hero earnings card ──────────────────────────────────────────
        var billingData = GetBillingData();

        var heroCard = new PanelContainer();
        var heroStyle = new StyleBoxFlat
        {
            BgColor = Primary,
            CornerRadiusTopLeft     = 14,
            CornerRadiusTopRight    = 14,
            CornerRadiusBottomLeft  = 14,
            CornerRadiusBottomRight = 14,
            ContentMarginLeft   = 24,
            ContentMarginRight  = 24,
            ContentMarginTop    = 24,
            ContentMarginBottom = 24,
        };
        heroCard.AddThemeStyleboxOverride("panel", heroStyle);
        heroCard.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        contentVbox.AddChild(heroCard);

        var heroVbox = new VBoxContainer();
        heroVbox.AddThemeConstantOverride("separation", 4);
        heroCard.AddChild(heroVbox);

        var heroSubtitle = new Label();
        heroSubtitle.Text = "Today's Earnings";
        heroSubtitle.AddThemeFontSizeOverride("font_size", 14);
        heroSubtitle.AddThemeColorOverride("font_color", new Color(1, 1, 1, 0.7f));
        heroSubtitle.HorizontalAlignment = HorizontalAlignment.Center;
        heroVbox.AddChild(heroSubtitle);

        float todayEarnings = billingData.ContainsKey("today_earnings")
            ? (float)billingData["today_earnings"] : 0.0f;
        var heroAmount = new Label();
        heroAmount.Text = $"${todayEarnings:F2}";
        heroAmount.AddThemeFontSizeOverride("font_size", 42);
        heroAmount.AddThemeColorOverride("font_color", Colors.White);
        heroAmount.HorizontalAlignment = HorizontalAlignment.Center;
        heroVbox.AddChild(heroAmount);

        int todayCount = billingData.ContainsKey("today_count")
            ? (int)billingData["today_count"] : 0;
        var heroCount = new Label();
        heroCount.Text = $"{todayCount} packs today";
        heroCount.AddThemeFontSizeOverride("font_size", 14);
        heroCount.AddThemeColorOverride("font_color", new Color(1, 1, 1, 0.6f));
        heroCount.HorizontalAlignment = HorizontalAlignment.Center;
        heroVbox.AddChild(heroCount);

        // ── Per-skydiver section header ─────────────────────────────────
        var sectionLabel = new Label();
        sectionLabel.Text = "Per Skydiver";
        sectionLabel.AddThemeFontSizeOverride("font_size", 16);
        sectionLabel.AddThemeColorOverride("font_color", TextColor);
        contentVbox.AddChild(sectionLabel);

        // ── Skydiver list ───────────────────────────────────────────────
        _skydiverListContainer = new VBoxContainer();
        _skydiverListContainer.AddThemeConstantOverride("separation", 10);
        _skydiverListContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        contentVbox.AddChild(_skydiverListContainer);

        var skydivers = billingData.ContainsKey("skydivers")
            ? (Godot.Collections.Array)billingData["skydivers"]
            : new Godot.Collections.Array();

        foreach (var sdVar in skydivers)
        {
            var sd = sdVar.AsGodotDictionary();
            var card = CreateSkydiverCard(sd);
            _skydiverListContainer.AddChild(card);
        }

        // ── Settle All button ───────────────────────────────────────────
        var settleMargin = new MarginContainer();
        settleMargin.AddThemeConstantOverride("margin_top", 8);
        contentVbox.AddChild(settleMargin);

        var settleBtn = new Button();
        settleBtn.Text = "Settle All";
        settleBtn.CustomMinimumSize = new Vector2(0, 52);
        settleBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        settleBtn.AddThemeFontSizeOverride("font_size", 17);
        settleBtn.AddThemeColorOverride("font_color", Colors.White);
        settleBtn.MouseDefaultCursorShape = CursorShape.PointingHand;

        var settleNormal = CreateRoundedStyle(Orange, 12);
        settleBtn.AddThemeStyleboxOverride("normal", settleNormal);

        var settleHover = CreateRoundedStyle(Orange.Lightened(0.1f), 12);
        settleBtn.AddThemeStyleboxOverride("hover", settleHover);

        var settlePressed = CreateRoundedStyle(Orange.Darkened(0.1f), 12);
        settleBtn.AddThemeStyleboxOverride("pressed", settlePressed);

        settleBtn.Pressed += OnSettleAll;
        settleMargin.AddChild(settleBtn);

        // ── Bottom nav bar ──────────────────────────────────────────────
        BuildNavBar(rootVbox, 2);
    }

    // ── Skydiver card builder ────────────────────────────────────────────

    private PanelContainer CreateSkydiverCard(Dictionary sd)
    {
        var card = new PanelContainer();
        var cardStyle = new StyleBoxFlat
        {
            BgColor = CardColor,
            CornerRadiusTopLeft     = 10,
            CornerRadiusTopRight    = 10,
            CornerRadiusBottomLeft  = 10,
            CornerRadiusBottomRight = 10,
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
        hbox.AddThemeConstantOverride("separation", 10);
        card.AddChild(hbox);

        // Name and pack count (left column)
        var leftVbox = new VBoxContainer();
        leftVbox.AddThemeConstantOverride("separation", 2);
        leftVbox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        hbox.AddChild(leftVbox);

        string sdName = sd.ContainsKey("name") ? sd["name"].ToString() : "Unknown";
        var nameLabel = new Label();
        nameLabel.Text = sdName;
        nameLabel.AddThemeFontSizeOverride("font_size", 15);
        nameLabel.AddThemeColorOverride("font_color", TextColor);
        leftVbox.AddChild(nameLabel);

        int packCount = sd.ContainsKey("pack_count") ? (int)sd["pack_count"] : 0;
        var countLabel = new Label();
        countLabel.Text = $"{packCount} packs";
        countLabel.AddThemeFontSizeOverride("font_size", 12);
        countLabel.AddThemeColorOverride("font_color", TextSec);
        leftVbox.AddChild(countLabel);

        // Amount
        float amount = sd.ContainsKey("amount") ? (float)sd["amount"] : 0.0f;
        var amountLabel = new Label();
        amountLabel.Text = $"${amount:F2}";
        amountLabel.AddThemeFontSizeOverride("font_size", 16);
        amountLabel.AddThemeColorOverride("font_color", TextColor);
        hbox.AddChild(amountLabel);

        // Paid/Unpaid pill
        bool isPaid = sd.ContainsKey("paid") && (bool)sd["paid"];

        var pill = new PanelContainer();
        var pillStyle = new StyleBoxFlat
        {
            BgColor = isPaid ? Green : Red,
            CornerRadiusTopLeft     = 10,
            CornerRadiusTopRight    = 10,
            CornerRadiusBottomLeft  = 10,
            CornerRadiusBottomRight = 10,
            ContentMarginLeft   = 10,
            ContentMarginRight  = 10,
            ContentMarginTop    = 4,
            ContentMarginBottom = 4,
        };
        pill.AddThemeStyleboxOverride("panel", pillStyle);
        hbox.AddChild(pill);

        var pillLabel = new Label();
        pillLabel.Text = isPaid ? "Paid" : "Unpaid";
        pillLabel.AddThemeFontSizeOverride("font_size", 12);
        pillLabel.AddThemeColorOverride("font_color", Colors.White);
        pill.AddChild(pillLabel);

        return card;
    }

    // ── Style helper ─────────────────────────────────────────────────────

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

    // ── Settle all action ────────────────────────────────────────────────

    private void OnSettleAll()
    {
        var gameData = GetNodeOrNull<GameData>("/root/GameData");
        if (gameData != null)
        {
            foreach (var log in gameData.PackLogs)
            {
                if (log != null)
                {
                    log["settled"] = true;
                }
            }
        }

        GD.Print("[PackTrack] All charges settled");
        RefreshSkydiverList();
    }

    private void RefreshSkydiverList()
    {
        foreach (var child in _skydiverListContainer.GetChildren())
        {
            child.QueueFree();
        }

        var billingData = GetBillingData();
        var skydivers = billingData.ContainsKey("skydivers")
            ? (Godot.Collections.Array)billingData["skydivers"]
            : new Godot.Collections.Array();

        foreach (var sdVar in skydivers)
        {
            var sd = sdVar.AsGodotDictionary();
            // After settling, mark all as paid for display
            sd["paid"] = true;
            var card = CreateSkydiverCard(sd);
            _skydiverListContainer.AddChild(card);
        }
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

    private void OnNavTabPressed(string screenName)
    {
        if (screenName == "packer_billing") return;

        var nav = GetNodeOrNull<NavManager>("/root/NavManager");
        nav?.NavigateTo(screenName);
    }

    // ── Billing data aggregation ─────────────────────────────────────────

    private Dictionary GetBillingData()
    {
        float todayEarnings = 0.0f;
        int todayCount = 0;
        var skydiverMap = new Dictionary<string, Dictionary>();

        var gameData = GetNodeOrNull<GameData>("/root/GameData");
        if (gameData != null)
        {
            string currentPacker = GetCurrentPackerName();
            var packLogs = gameData.PackLogs;

            if (packLogs != null)
            {
                var todayDict = Time.GetDatetimeDictFromSystem();
                string today = $"{(int)todayDict["year"]:D4}-{(int)todayDict["month"]:D2}-{(int)todayDict["day"]:D2}";

                foreach (var log in packLogs)
                {
                    if (log == null) continue;

                    string packer = log.ContainsKey("packer_name") ? log["packer_name"].ToString() : "";
                    if (packer != currentPacker) continue;

                    float charge = log.ContainsKey("charge_amount") ? (float)log["charge_amount"] : 7.00f;
                    string skydiverName = log.ContainsKey("skydiver_name") ? log["skydiver_name"].ToString() : "Unknown";
                    string ts = log.ContainsKey("timestamp") ? log["timestamp"].ToString() : "";
                    bool settled = log.ContainsKey("settled") && (bool)log["settled"];

                    if (ts.StartsWith(today))
                    {
                        todayEarnings += charge;
                        todayCount += 1;
                    }

                    if (!skydiverMap.ContainsKey(skydiverName))
                    {
                        skydiverMap[skydiverName] = new Dictionary
                        {
                            { "name", skydiverName },
                            { "pack_count", 0 },
                            { "amount", 0.0f },
                            { "paid", settled },
                        };
                    }
                    skydiverMap[skydiverName]["pack_count"] = (int)skydiverMap[skydiverName]["pack_count"] + 1;
                    skydiverMap[skydiverName]["amount"] = (float)skydiverMap[skydiverName]["amount"] + charge;
                    // If any log is unsettled, mark as unpaid
                    if (!settled)
                    {
                        skydiverMap[skydiverName]["paid"] = false;
                    }
                }

                if (skydiverMap.Count > 0)
                {
                    var sdArray = new Godot.Collections.Array();
                    foreach (var kvp in skydiverMap)
                    {
                        sdArray.Add(kvp.Value);
                    }
                    return new Dictionary
                    {
                        { "today_earnings", todayEarnings },
                        { "today_count", todayCount },
                        { "skydivers", sdArray },
                    };
                }
            }
        }

        // Fallback sample data
        return GetSampleBillingData();
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

    private Dictionary GetSampleBillingData()
    {
        return new Dictionary
        {
            { "today_earnings", 45.00f },
            { "today_count", 6 },
            { "skydivers", new Godot.Collections.Array
                {
                    new Dictionary { { "name", "Sarah Chen" },       { "pack_count", 2 }, { "amount", 14.00f }, { "paid", false } },
                    new Dictionary { { "name", "Marcus Rodriguez" }, { "pack_count", 1 }, { "amount", 7.00f },  { "paid", false } },
                    new Dictionary { { "name", "Emily Foster" },     { "pack_count", 1 }, { "amount", 7.00f },  { "paid", true } },
                    new Dictionary { { "name", "Dylan Park" },       { "pack_count", 2 }, { "amount", 17.00f }, { "paid", false } },
                }
            },
        };
    }
}
