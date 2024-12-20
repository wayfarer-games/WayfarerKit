using UnityEngine;

namespace WayfarerKit.Helpers
{
    public static class ColorHelpers
    {
        public static Color DarkenColor(this Color color, float amount)
        {
            Color.RGBToHSV(color, out var h, out var s, out var v);

            v -= amount;
            v = Mathf.Clamp01(v);

            return Color.HSVToRGB(h, s, v);
        }

        public static bool IsGrayscale(this Color color) => Mathf.Approximately(color.r, color.g) && Mathf.Approximately(color.g, color.b);

        public static Color AdjustIfGrayscale(this Color color)
        {
            if (!IsGrayscale(color)) return color;

            const float adjustment = 0.04f;

            var r = color.r;
            var g = color.g + adjustment;
            var b = color.b - adjustment;

            r = Mathf.Clamp01(r);
            g = Mathf.Clamp01(g);
            b = Mathf.Clamp01(b);

            return new(r, g, b, color.a);
        }

        public static bool IsColorLight(this Color color, double threshold = .5d)
        {
            double perceivedBrightness = Mathf.Sqrt(
                0.299f * color.r * color.r +
                0.587f * color.g * color.g +
                0.114f * color.b * color.b
            );

            return perceivedBrightness > threshold;
        }

        public static bool IsSimilarWith(this Color a, Color b, float threshold = .1f) 
            => Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b) + Mathf.Abs(a.a - b.a) < threshold;

        public static Color ChangeAlpha(this Color color, float alpha) => new(color.r, color.g, color.b, Mathf.Clamp01(alpha));
    }
}