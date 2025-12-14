using System;
using System.Drawing;

namespace Fed.Label.V2
{
    public static class GraphicsExtensions
    {
        public static Font GetFittedFont(this Graphics g, string text, Font desiredFont, int maxWidth, int maxHeight)
        {
            const int minFontSize = 10;

            var measures = g.MeasureString(text, desiredFont);

            if (measures.Width <= maxWidth && measures.Height <= maxHeight)
                return desiredFont;

            if (desiredFont.Size == minFontSize)
                return desiredFont;

            return g.GetFittedFont(
                text,
                new Font(
                    desiredFont.FontFamily,
                    Math.Max(minFontSize, desiredFont.Size - 1),
                    desiredFont.Style,
                    desiredFont.Unit,
                    desiredFont.GdiCharSet,
                    desiredFont.GdiVerticalFont),
                maxWidth,
                maxHeight);
        }
    }
}
