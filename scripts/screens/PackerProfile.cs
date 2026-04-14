using Godot;

public partial class PackerProfile : Control
{
    private static readonly Color Primary = new("#1B3A5C");
    private static readonly Color Orange = new("#E87B35");
    private static readonly Color Green = new("#2D9F5C");
    private static readonly Color Bg = new("#F5F5F5");
    private static readonly Color TextClr = new("#1E1E1E");
    private static readonly Color TextSec = new("#4A4A4A");
    private static readonly Color TextTer = new("#9B9B9B");
    private static readonly Color Border = new("#E8E8E8");

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        var bg = new ColorRect { Color = Bg };
        bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        var root = new VBoxContainer();
        root.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        root.AddThemeConstantOverride("separation", 0);
        AddChild(root);

        // Header
        var header = new PanelContainer();
        var hs = new StyleBoxFlat { BgColor = Primary };
        hs.ContentMarginLeft = 20; hs.ContentMarginRight = 20;
        hs.ContentMarginTop = 16; hs.ContentMarginBottom = 16;
        header.AddThemeStyleboxOverride("panel", hs);
        root.AddChild(header);

        var title = new Label { Text = "Profile" };
        title.AddThemeFontSizeOverride("font_size", 24);
        title.AddThemeColorOverride("font_color", Colors.White);
        header.AddChild(title);

        // Scroll content
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        root.AddChild(scroll);

        var content = new VBoxContainer();
        content.AddThemeConstantOverride("separation", 16);
        content.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(content);

        var padding = new MarginContainer();
        padding.AddThemeConstantOverride("margin_left", 20);
        padding.AddThemeConstantOverride("margin_right", 20);
        padding.AddThemeConstantOverride("margin_top", 24);
        padding.AddThemeConstantOverride("margin_bottom", 24);
        padding.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        content.AddChild(padding);

        var inner = new VBoxContainer();
        inner.AddThemeConstantOverride("separation", 20);
        inner.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        padding.AddChild(inner);

        // Avatar + Name
        var gameData = GetNodeOrNull<GameData>("/root/GameData");
        string userName = "Jake Morrison";
        string userRole = "Packer";
        if (gameData?.CurrentUser?.ContainsKey("name") == true)
        {
            var n = gameData.CurrentUser["name"];
            if (n.VariantType != Variant.Type.Nil) userName = n.ToString();
        }
        if (gameData?.CurrentUser?.ContainsKey("role") == true)
        {
            var r = gameData.CurrentUser["role"];
            if (r.VariantType != Variant.Type.Nil) userRole = r.ToString();
        }

        var avatarRow = new VBoxContainer();
        avatarRow.AddThemeConstantOverride("separation", 8);
        avatarRow.Alignment = BoxContainer.AlignmentMode.Center;
        inner.AddChild(avatarRow);

        // Avatar circle
        var avatarPanel = new PanelContainer();
        var avatarStyle = new StyleBoxFlat
        {
            BgColor = Primary,
            CornerRadiusTopLeft = 40, CornerRadiusTopRight = 40,
            CornerRadiusBottomLeft = 40, CornerRadiusBottomRight = 40,
        };
        avatarStyle.ContentMarginLeft = 20; avatarStyle.ContentMarginRight = 20;
        avatarStyle.ContentMarginTop = 14; avatarStyle.ContentMarginBottom = 14;
        avatarPanel.AddThemeStyleboxOverride("panel", avatarStyle);
        avatarPanel.CustomMinimumSize = new Vector2(80, 80);
        avatarPanel.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        avatarRow.AddChild(avatarPanel);

        string initials = "";
        var parts = userName.Split(' ');
        foreach (var p in parts) { if (p.Length > 0) initials += p[0]; }
        var initialsLabel = new Label { Text = initials, HorizontalAlignment = HorizontalAlignment.Center };
        initialsLabel.AddThemeFontSizeOverride("font_size", 28);
        initialsLabel.AddThemeColorOverride("font_color", Colors.White);
        avatarPanel.AddChild(initialsLabel);

        var nameLabel = new Label { Text = userName, HorizontalAlignment = HorizontalAlignment.Center };
        nameLabel.AddThemeFontSizeOverride("font_size", 22);
        nameLabel.AddThemeColorOverride("font_color", TextClr);
        avatarRow.AddChild(nameLabel);

        var roleLabel = new Label { Text = userRole.ToUpper(), HorizontalAlignment = HorizontalAlignment.Center };
        roleLabel.AddThemeFontSizeOverride("font_size", 13);
        roleLabel.AddThemeColorOverride("font_color", TextTer);
        avatarRow.AddChild(roleLabel);

        // Stats row
        var statsRow = new HBoxContainer();
        statsRow.AddThemeConstantOverride("separation", 12);
        inner.AddChild(statsRow);

        int totalPacks = 0;
        float totalEarnings = 0;
        if (gameData != null)
        {
            foreach (var log in gameData.PackLogs)
            {
                var packer = log.ContainsKey("packer_name") ? log["packer_name"].ToString() : "";
                if (packer == userName)
                {
                    totalPacks++;
                    if (log.ContainsKey("charge_amount"))
                        totalEarnings += (float)log["charge_amount"];
                }
            }
        }

        AddStatCard(statsRow, totalPacks.ToString(), "Total Packs", Primary);
        AddStatCard(statsRow, $"${totalEarnings:F0}", "Earnings", Orange);
        AddStatCard(statsRow, "4.9", "Rating", Green);

        // Info cards
        AddInfoSection(inner, "Personal Information", new string[,]
        {
            { "Name", userName },
            { "Role", userRole },
            { "DZ", "SkyHigh Drop Zone" },
            { "Member Since", "2024-01-15" },
        });

        AddInfoSection(inner, "Certifications", new string[,]
        {
            { "USPA License", "D-12847" },
            { "Packer Rating", "Senior" },
            { "Tandem Certified", "Yes" },
        });

        // Bottom nav
        var nav = new PanelContainer();
        var navStyle = new StyleBoxFlat { BgColor = Colors.White };
        navStyle.BorderColor = new Color(0, 0, 0, 0.1f);
        navStyle.BorderWidthTop = 1;
        navStyle.ContentMarginTop = 8; navStyle.ContentMarginBottom = 8;
        navStyle.ContentMarginLeft = 8; navStyle.ContentMarginRight = 8;
        nav.AddThemeStyleboxOverride("panel", navStyle);
        root.AddChild(nav);

        var navRow = new HBoxContainer();
        navRow.Alignment = BoxContainer.AlignmentMode.Center;
        nav.AddChild(navRow);

        string[] tabs = { "Queue", "History", "Billing", "Profile" };
        string[] screens = { "packer_queue", "packer_history", "packer_billing", "packer_profile" };
        for (int i = 0; i < tabs.Length; i++)
        {
            var btn = new Button { Text = tabs[i], Flat = true };
            btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            btn.AddThemeFontSizeOverride("font_size", 13);
            btn.AddThemeColorOverride("font_color", i == 3 ? Orange : TextSec);
            string target = screens[i];
            btn.Pressed += () =>
            {
                if (target != "packer_profile")
                {
                    var nm = GetNodeOrNull<NavManager>("/root/NavManager");
                    nm?.NavigateTo(target);
                }
            };
            navRow.AddChild(btn);
        }
    }

    private void AddStatCard(HBoxContainer parent, string value, string label, Color accent)
    {
        var card = new PanelContainer();
        card.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var style = new StyleBoxFlat
        {
            BgColor = Colors.White,
            CornerRadiusTopLeft = 12, CornerRadiusTopRight = 12,
            CornerRadiusBottomLeft = 12, CornerRadiusBottomRight = 12,
            ShadowColor = new Color(0, 0, 0, 0.06f), ShadowSize = 3,
        };
        style.ContentMarginTop = 16; style.ContentMarginBottom = 16;
        style.ContentMarginLeft = 12; style.ContentMarginRight = 12;
        card.AddThemeStyleboxOverride("panel", style);
        parent.AddChild(card);

        var vbox = new VBoxContainer();
        vbox.Alignment = BoxContainer.AlignmentMode.Center;
        vbox.AddThemeConstantOverride("separation", 4);
        card.AddChild(vbox);

        var valLabel = new Label { Text = value, HorizontalAlignment = HorizontalAlignment.Center };
        valLabel.AddThemeFontSizeOverride("font_size", 28);
        valLabel.AddThemeColorOverride("font_color", accent);
        vbox.AddChild(valLabel);

        var lblLabel = new Label { Text = label, HorizontalAlignment = HorizontalAlignment.Center };
        lblLabel.AddThemeFontSizeOverride("font_size", 12);
        lblLabel.AddThemeColorOverride("font_color", TextTer);
        vbox.AddChild(lblLabel);
    }

    private void AddInfoSection(VBoxContainer parent, string sectionTitle, string[,] rows)
    {
        var card = new PanelContainer();
        card.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var style = new StyleBoxFlat
        {
            BgColor = Colors.White,
            CornerRadiusTopLeft = 12, CornerRadiusTopRight = 12,
            CornerRadiusBottomLeft = 12, CornerRadiusBottomRight = 12,
            ShadowColor = new Color(0, 0, 0, 0.06f), ShadowSize = 3,
        };
        style.ContentMarginTop = 16; style.ContentMarginBottom = 16;
        style.ContentMarginLeft = 16; style.ContentMarginRight = 16;
        card.AddThemeStyleboxOverride("panel", style);
        parent.AddChild(card);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 8);
        card.AddChild(vbox);

        var titleLbl = new Label { Text = sectionTitle };
        titleLbl.AddThemeFontSizeOverride("font_size", 16);
        titleLbl.AddThemeColorOverride("font_color", TextClr);
        vbox.AddChild(titleLbl);

        for (int i = 0; i < rows.GetLength(0); i++)
        {
            var row = new HBoxContainer();
            vbox.AddChild(row);

            var key = new Label { Text = rows[i, 0] };
            key.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            key.AddThemeFontSizeOverride("font_size", 14);
            key.AddThemeColorOverride("font_color", TextSec);
            row.AddChild(key);

            var val = new Label { Text = rows[i, 1] };
            val.AddThemeFontSizeOverride("font_size", 14);
            val.AddThemeColorOverride("font_color", TextClr);
            row.AddChild(val);
        }
    }
}
