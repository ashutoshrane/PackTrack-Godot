using Godot;

/// <summary>
/// Reusable card component that displays a rig's key info at a glance.
/// Shows a colored status bar on the left edge, rig ID, owner, canopy,
/// status badge, last-packed date, and total pack count.
/// Emits <see cref="CardPressed"/> when tapped.
/// </summary>
public partial class RigCard : PanelContainer
{
	// ── Signals ──────────────────────────────────────────────────────────────────
	[Signal] public delegate void CardPressedEventHandler(string rigId);

	// ── State ────────────────────────────────────────────────────────────────────

	/// <summary>The raw rig dictionary this card represents.</summary>
	public Godot.Collections.Dictionary RigData { get; private set; }

	private string _rigId = "";

	// ── Public API ───────────────────────────────────────────────────────────────

	/// <summary>
	/// Populate the card from a rig dictionary and build the UI tree.
	/// Call once after instantiation.
	/// </summary>
	public void Setup(Godot.Collections.Dictionary data)
	{
		RigData = data;
		_rigId = data.ContainsKey("id") ? data["id"].ToString() : "???";

		string owner    = data.ContainsKey("owner")      ? data["owner"].ToString()      : "";
		string canopy   = data.ContainsKey("canopy")     ? data["canopy"].ToString()     : "";
		string container = data.ContainsKey("container") ? data["container"].ToString()  : "";
		string status   = data.ContainsKey("status")     ? data["status"].ToString()     : "ok";
		string lastPacked = data.ContainsKey("lastPacked") ? data["lastPacked"].ToString() : "—";
		int packCount   = data.ContainsKey("packCount")  ? (int)data["packCount"]        : 0;

		BuildUI(_rigId, owner, canopy, container, status, lastPacked, packCount);
	}

	// ── UI Construction ──────────────────────────────────────────────────────────

	private void BuildUI(string rigId, string owner, string canopy,
						  string container, string status, string lastPacked, int packCount)
	{
		// Card background
		AddThemeStyleboxOverride("panel", ThemeManager.CreateCardStyle());
		CustomMinimumSize = new Vector2(0, 90);

		// Outer horizontal: status bar | content
		var hbox = new HBoxContainer();
		hbox.AddThemeConstantOverride("separation", 12);
		AddChild(hbox);

		// ── Status bar (colored left edge) ───────────────────────────────────
		var statusBar = new Panel();
		statusBar.CustomMinimumSize = new Vector2(6, 0);
		statusBar.SizeFlagsVertical = SizeFlags.Fill;
		var barStyle = new StyleBoxFlat();
		barStyle.BgColor = GetStatusColor(status);
		barStyle.CornerRadiusTopLeft     = 3;
		barStyle.CornerRadiusBottomLeft  = 3;
		barStyle.CornerRadiusTopRight    = 3;
		barStyle.CornerRadiusBottomRight = 3;
		statusBar.AddThemeStyleboxOverride("panel", barStyle);
		hbox.AddChild(statusBar);

		// ── Content column ───────────────────────────────────────────────────
		var vbox = new VBoxContainer();
		vbox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		vbox.AddThemeConstantOverride("separation", 4);
		hbox.AddChild(vbox);

		// Row 1: Rig ID (bold) + status badge
		var topRow = new HBoxContainer();
		vbox.AddChild(topRow);

		var rigLabel = new Label();
		rigLabel.Text = rigId;
		rigLabel.AddThemeFontSizeOverride("font_size", 18);
		rigLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		// Bold via LabelSettings
		var boldSettings = new LabelSettings();
		boldSettings.FontSize = 18;
		boldSettings.FontColor = ThemeManager.Text;
		rigLabel.LabelSettings = boldSettings;
		topRow.AddChild(rigLabel);

		// Status badge
		var badge = new StatusBadge();
		int daysRemaining = 0;
		if (RigData != null && RigData.ContainsKey("lastPacked"))
		{
			if (System.DateTime.TryParse(RigData["lastPacked"].ToString(), out var lp))
			{
				daysRemaining = GameData.REPACK_CYCLE_DAYS - (System.DateTime.Now - lp).Days;
				if (daysRemaining < 0) daysRemaining = 0;
			}
		}
		badge.Setup(status, daysRemaining);
		topRow.AddChild(badge);

		// Row 2: Owner — Canopy / Container
		var detailLabel = new Label();
		detailLabel.Text = $"{owner}  ·  {canopy} / {container}";
		detailLabel.AddThemeFontSizeOverride("font_size", 14);
		detailLabel.AddThemeColorOverride("font_color", ThemeManager.TextSec);
		vbox.AddChild(detailLabel);

		// Row 3: Last packed + pack count
		var bottomRow = new HBoxContainer();
		vbox.AddChild(bottomRow);

		var lastPackedLabel = new Label();
		lastPackedLabel.Text = $"Last packed: {lastPacked}";
		lastPackedLabel.AddThemeFontSizeOverride("font_size", 13);
		lastPackedLabel.AddThemeColorOverride("font_color", ThemeManager.TextTer);
		lastPackedLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		bottomRow.AddChild(lastPackedLabel);

		var countLabel = new Label();
		countLabel.Text = $"{packCount} packs";
		countLabel.AddThemeFontSizeOverride("font_size", 13);
		countLabel.AddThemeColorOverride("font_color", ThemeManager.TextTer);
		bottomRow.AddChild(countLabel);

		// ── Click detection ──────────────────────────────────────────────────
		MouseFilter = MouseFilterEnum.Stop;
		GuiInput += OnGuiInput;
	}

	private void OnGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
		{
			EmitSignal(SignalName.CardPressed, _rigId);
		}
	}

	// ── Helpers ──────────────────────────────────────────────────────────────────

	/// <summary>Map a status string to the appropriate theme color.</summary>
	public static Color GetStatusColor(string status)
	{
		return status switch
		{
			"warning" => ThemeManager.Amber,
			"overdue" => ThemeManager.Red,
			_         => ThemeManager.Green
		};
	}
}
