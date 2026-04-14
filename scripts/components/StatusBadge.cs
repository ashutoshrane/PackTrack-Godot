using Godot;

/// <summary>
/// Small colored pill that conveys a rig's repack status at a glance.
///   - Green  "OK"             — well within the 180-day cycle
///   - Amber  "Due in X days"  — approaching the repack deadline
///   - Red    "OVERDUE"        — past the FAA 180-day limit
/// </summary>
public partial class StatusBadge : PanelContainer
{
	/// <summary>
	/// Build the badge UI for the given status.
	/// </summary>
	/// <param name="status">"ok", "warning", or "overdue"</param>
	/// <param name="daysRemaining">Days left until the 180-day repack deadline (used for warning text).</param>
	public void Setup(string status, int daysRemaining = 0)
	{
		Color bgColor;
		Color textColor;
		string text;

		switch (status)
		{
			case "overdue":
				bgColor   = new Color(ThemeManager.Red.R, ThemeManager.Red.G, ThemeManager.Red.B, 0.15f);
				textColor = ThemeManager.Red;
				text      = "OVERDUE";
				break;

			case "warning":
				bgColor   = new Color(ThemeManager.Amber.R, ThemeManager.Amber.G, ThemeManager.Amber.B, 0.15f);
				textColor = ThemeManager.Amber;
				text      = daysRemaining > 0 ? $"Due in {daysRemaining}d" : "Due soon";
				break;

			default: // "ok"
				bgColor   = new Color(ThemeManager.Green.R, ThemeManager.Green.G, ThemeManager.Green.B, 0.15f);
				textColor = ThemeManager.Green;
				text      = "OK";
				break;
		}

		// Pill-shaped background
		var pillStyle = new StyleBoxFlat();
		pillStyle.BgColor = bgColor;
		pillStyle.CornerRadiusTopLeft     = 10;
		pillStyle.CornerRadiusTopRight    = 10;
		pillStyle.CornerRadiusBottomLeft  = 10;
		pillStyle.CornerRadiusBottomRight = 10;
		pillStyle.ContentMarginLeft   = 10;
		pillStyle.ContentMarginRight  = 10;
		pillStyle.ContentMarginTop    = 4;
		pillStyle.ContentMarginBottom = 4;
		AddThemeStyleboxOverride("panel", pillStyle);

		// Label
		var label = new Label();
		label.Text = text;
		label.AddThemeFontSizeOverride("font_size", 12);
		label.AddThemeColorOverride("font_color", textColor);
		label.HorizontalAlignment = HorizontalAlignment.Center;
		AddChild(label);
	}
}
