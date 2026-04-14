using Godot;

/// <summary>
/// Eye-catching orange "Pack" button used in rig detail / queue screens.
/// After being pressed it briefly flashes green with a "Packed!" label
/// and emits <see cref="PackCompleted"/> so parent screens can record the job.
/// </summary>
public partial class PackButton : Button
{
	// ── Signals ──────────────────────────────────────────────────────────────────
	[Signal] public delegate void PackCompletedEventHandler(string rigId);

	// ── Properties ───────────────────────────────────────────────────────────────

	/// <summary>The rig this button will record a pack for.</summary>
	public string RigId { get; private set; } = "";

	// ── Internal ─────────────────────────────────────────────────────────────────
	private StyleBoxFlat _normalStyle;
	private StyleBoxFlat _flashStyle;
	private bool _flashing = false;

	// ── Public API ───────────────────────────────────────────────────────────────

	/// <summary>
	/// Assign the rig ID and update the label.
	/// </summary>
	public void Setup(string id)
	{
		RigId = id;
		Text = "Pack";
	}

	// ── Lifecycle ────────────────────────────────────────────────────────────────

	public override void _Ready()
	{
		// ── Normal style (orange) ────────────────────────────────────────────
		_normalStyle = new StyleBoxFlat();
		_normalStyle.BgColor = ThemeManager.Orange;
		_normalStyle.CornerRadiusTopLeft     = 12;
		_normalStyle.CornerRadiusTopRight    = 12;
		_normalStyle.CornerRadiusBottomLeft  = 12;
		_normalStyle.CornerRadiusBottomRight = 12;
		_normalStyle.ContentMarginLeft   = 24;
		_normalStyle.ContentMarginRight  = 24;
		_normalStyle.ContentMarginTop    = 14;
		_normalStyle.ContentMarginBottom = 14;

		// ── Flash style (green confirmation) ─────────────────────────────────
		_flashStyle = new StyleBoxFlat();
		_flashStyle.BgColor = ThemeManager.Green;
		_flashStyle.CornerRadiusTopLeft     = 12;
		_flashStyle.CornerRadiusTopRight    = 12;
		_flashStyle.CornerRadiusBottomLeft  = 12;
		_flashStyle.CornerRadiusBottomRight = 12;
		_flashStyle.ContentMarginLeft   = 24;
		_flashStyle.ContentMarginRight  = 24;
		_flashStyle.ContentMarginTop    = 14;
		_flashStyle.ContentMarginBottom = 14;

		// Apply normal appearance
		CustomMinimumSize = new Vector2(0, 56);
		AddThemeStyleboxOverride("normal",  _normalStyle);
		AddThemeStyleboxOverride("hover",   _normalStyle);
		AddThemeStyleboxOverride("pressed", _normalStyle);
		AddThemeColorOverride("font_color",         Colors.White);
		AddThemeColorOverride("font_hover_color",   Colors.White);
		AddThemeColorOverride("font_pressed_color", Colors.White);
		AddThemeFontSizeOverride("font_size", 18);

		// Connect press
		Pressed += OnPressed;
	}

	// ── Handlers ─────────────────────────────────────────────────────────────────

	private async void OnPressed()
	{
		if (_flashing)
			return;

		_flashing = true;

		// Flash green + "Packed!"
		Text = "Packed!";
		AddThemeStyleboxOverride("normal",  _flashStyle);
		AddThemeStyleboxOverride("hover",   _flashStyle);
		AddThemeStyleboxOverride("pressed", _flashStyle);

		EmitSignal(SignalName.PackCompleted, RigId);

		// Hold the flash for ~1 second, then revert
		await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

		Text = "Pack";
		AddThemeStyleboxOverride("normal",  _normalStyle);
		AddThemeStyleboxOverride("hover",   _normalStyle);
		AddThemeStyleboxOverride("pressed", _normalStyle);

		_flashing = false;
	}
}
