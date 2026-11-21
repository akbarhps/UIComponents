using System.Drawing;

namespace UIComponents
{
    /// <summary>The single source of visual truth — mirrors the :root tokens in web/index.html.</summary>
    public static class Theme
    {
        public static readonly Color Bg = Color.FromArgb(245, 244, 239);
        public static readonly Color BgDeep = Color.FromArgb(238, 234, 224);
        public static readonly Color Surface = Color.FromArgb(255, 255, 255);
        public static readonly Color Surface2 = Color.FromArgb(250, 249, 245);
        public static readonly Color Ink = Color.FromArgb(38, 36, 31);
        public static readonly Color Ink2 = Color.FromArgb(91, 85, 74);
        public static readonly Color Ink3 = Color.FromArgb(107, 102, 89);
        public static readonly Color Line = Color.FromArgb(230, 226, 215);
        public static readonly Color LineStrong = Color.FromArgb(213, 207, 192);
        public static readonly Color Brand = Color.FromArgb(166, 78, 42);
        public static readonly Color BrandHover = Color.FromArgb(143, 63, 32);
        public static readonly Color BrandBright = Color.FromArgb(217, 119, 87);
        public static readonly Color BrandSoft = Color.FromArgb(248, 236, 228);
        public static readonly Color Success = Color.FromArgb(53, 107, 76);
        public static readonly Color SuccessSoft = Color.FromArgb(232, 242, 236);
        public static readonly Color Warning = Color.FromArgb(133, 90, 22);
        public static readonly Color WarningSoft = Color.FromArgb(248, 240, 221);
        public static readonly Color Danger = Color.FromArgb(176, 67, 58);
        public static readonly Color DangerSoft = Color.FromArgb(249, 234, 231);
        public static readonly Color Info = Color.FromArgb(63, 100, 133);
        public static readonly Color InfoSoft = Color.FromArgb(234, 240, 246);

        public static Font Serif(float size)
        {
            return new Font("Georgia", size, FontStyle.Regular, GraphicsUnit.Point);
        }

        public static Font SerifBold(float size)
        {
            return new Font("Georgia", size, FontStyle.Bold, GraphicsUnit.Point);
        }

        public static Font Sans(float size, FontStyle style = FontStyle.Regular)
        {
            return new Font("Segoe UI", size, style, GraphicsUnit.Point);
        }

        /// <summary>Blend two colours; t = 0 gives a, t = 1 gives b.</summary>
        public static Color Blend(Color a, Color b, double t)
        {
            int r = (int)(a.R + (b.R - a.R) * t);
            int g = (int)(a.G + (b.G - a.G) * t);
            int bl = (int)(a.B + (b.B - a.B) * t);
            return Color.FromArgb(r, g, bl);
        }
    }
}
