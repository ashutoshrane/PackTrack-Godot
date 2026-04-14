using Godot;

/// <summary>
/// Centralized visual theme for PackTrack.
/// Exposes brand colors and factory methods for common StyleBox variants
/// so every screen renders with a consistent look.
/// Autoload singleton: add to Project → AutoLoad as "ThemeManager".
/// </summary>
public partial class ThemeManager : Node
{
	// ── Color Palette ──────────────────────────────────────────────────────────

	public static readonly Color Primary   = new Color("#1B3A5C");
	public static readonly Color Accent    = new Color("#4A9FD9");
	public static readonly Color Orange    = new Color("#E87B35");
	public static readonly Color Green     = new Color("#2D9F5C");
	public static readonly Color Amber     = new Color("#F5A623");
	public static readonly Color Red       = new Color("#D94141");
	public static readonly Color Bg        = new Color("#F5F5F5");
	public static readonly Color CardBg    = new Color("#FFFFFF");
	public static readonly Color Text      = new Color("#1E1E1E");
	public static readonly Color TextSec   = new Color("#4A4A4A");
	public static readonly Color TextTer   = new Color("#9B9B9B");
	public static readonly Color Border    = new Color("#E8E8E8");

	// ── Font Sizes ─────────────────────────────────────────────────────────────

	/// <summary>Font size for screen titles / headers.</summary>
	public static int GetHeaderFontSize() => 24;

	/// <summary>Font size for body / paragraph text.</summary>
	public static int GetBodyFontSize() => 16;

	// ── StyleBox Factories ─────────────────────────────────────────────────────

	/// <summary>
	/// Rounded button style filled with the given color.
	/// 12 px corner radius, 16 px horizontal / 12 px vertical padding.
	/// </summary>
	public static StyleBoxFlat CreateButtonStyle(Color color)
	{
		var style = new StyleBoxFlat();
		style.BgColor = color;
		style.CornerRadiusTopLeft     = 12;
		style.CornerRadiusTopRight    = 12;
		style.CornerRadiusBottomLeft  = 12;
		style.CornerRadiusBottomRight = 12;
		style.ContentMarginLeft   = 16;
		style.ContentMarginRight  = 16;
		style.ContentMarginTop    = 12;
		style.ContentMarginBottom = 12;
		return style;
	}

	/// <summary>
	/// White card panel with subtle shadow, rounded corners, and comfortable padding.
	/// </summary>
	public static StyleBoxFlat CreateCardStyle()
	{
		var style = new StyleBoxFlat();
		style.BgColor = CardBg;
		style.CornerRadiusTopLeft     = 12;
		style.CornerRadiusTopRight    = 12;
		style.CornerRadiusBottomLeft  = 12;
		style.CornerRadiusBottomRight = 12;

		// Subtle drop-shadow via border + expand
		style.ShadowColor  = new Color(0, 0, 0, 0.08f);
		style.ShadowSize   = 4;
		style.ShadowOffset = new Vector2(0, 2);

		// Border
		style.BorderColor        = Border;
		style.BorderWidthTop     = 1;
		style.BorderWidthBottom  = 1;
		style.BorderWidthLeft    = 1;
		style.BorderWidthRight   = 1;

		// Padding
		style.ContentMarginLeft   = 16;
		style.ContentMarginRight  = 16;
		style.ContentMarginTop    = 14;
		style.ContentMarginBottom = 14;

		return style;
	}

	/// <summary>
	/// Text-input style with light border, white fill, and rounded corners.
	/// </summary>
	public static StyleBoxFlat CreateInputStyle()
	{
		var style = new StyleBoxFlat();
		style.BgColor = CardBg;
		style.CornerRadiusTopLeft     = 8;
		style.CornerRadiusTopRight    = 8;
		style.CornerRadiusBottomLeft  = 8;
		style.CornerRadiusBottomRight = 8;

		style.BorderColor        = Border;
		style.BorderWidthTop     = 1;
		style.BorderWidthBottom  = 1;
		style.BorderWidthLeft    = 1;
		style.BorderWidthRight   = 1;

		style.ContentMarginLeft   = 12;
		style.ContentMarginRight  = 12;
		style.ContentMarginTop    = 10;
		style.ContentMarginBottom = 10;

		return style;
	}
}
