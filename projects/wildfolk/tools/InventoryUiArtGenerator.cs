using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Wildfolk.Tools
{
    public static class InventoryUiArtGenerator
    {
        public static void Generate(string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);
            Save(outputDirectory, "inventory_panel.png", 320, 192, DrawPanel);
            Save(outputDirectory, "inventory_rail.png", 256, 128, DrawRail);
            Save(outputDirectory, "inventory_slot.png", 128, 128, DrawSlot);
            Save(outputDirectory, "inventory_selected_slot.png", 128, 128, DrawSelectedSlot);
            Save(outputDirectory, "inventory_button.png", 256, 128, DrawButton);
        }

        private static void Save(string directory, string name, int width, int height, Action<Graphics> draw)
        {
            using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                draw(graphics);
                bitmap.Save(Path.Combine(directory, name), ImageFormat.Png);
            }
        }

        private static GraphicsPath Rounded(float x, float y, float width, float height, float radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2f;
            path.AddArc(x, y, diameter, diameter, 180f, 90f);
            path.AddArc(x + width - diameter, y, diameter, diameter, 270f, 90f);
            path.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(x, y + height - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        private static void FillRounded(Graphics graphics, float x, float y, float width, float height, float radius, Color color)
        {
            using (var path = Rounded(x, y, width, height, radius))
            using (var brush = new SolidBrush(color)) graphics.FillPath(brush, path);
        }

        private static void StrokeRounded(Graphics graphics, float x, float y, float width, float height, float radius, Color color, float thickness)
        {
            using (var path = Rounded(x + thickness / 2f, y + thickness / 2f, width - thickness, height - thickness, Math.Max(1f, radius - thickness / 2f)))
            using (var pen = new Pen(color, thickness))
            {
                pen.Alignment = PenAlignment.Center;
                pen.LineJoin = LineJoin.Round;
                graphics.DrawPath(pen, path);
            }
        }

        private static void DrawPanel(Graphics graphics)
        {
            FillRounded(graphics, 1f, 1f, 318f, 190f, 13f, Color.FromArgb(150, 48, 35, 26));
            StrokeRounded(graphics, 1f, 1f, 318f, 190f, 13f, Color.FromArgb(245, 30, 23, 19), 2.2f);
        }

        private static void DrawRail(Graphics graphics)
        {
            FillRounded(graphics, 1f, 1f, 254f, 126f, 9f, Color.FromArgb(238, 53, 41, 31));
            StrokeRounded(graphics, 1f, 1f, 254f, 126f, 9f, Color.FromArgb(172, 139, 106, 75), 1.6f);
        }

        private static void DrawSlot(Graphics graphics)
        {
            FillRounded(graphics, 1f, 1f, 126f, 126f, 8f, Color.FromArgb(248, 87, 65, 47));
            StrokeRounded(graphics, 1f, 1f, 126f, 126f, 8f, Color.FromArgb(246, 38, 29, 23), 1.8f);
        }

        private static void DrawSelectedSlot(Graphics graphics)
        {
            FillRounded(graphics, 2f, 2f, 124f, 124f, 8f, Color.FromArgb(42, 239, 169, 82));
            StrokeRounded(graphics, 2f, 2f, 124f, 124f, 8f, Color.FromArgb(255, 237, 170, 91), 3.2f);
        }

        private static void DrawButton(Graphics graphics)
        {
            FillRounded(graphics, 1f, 1f, 254f, 126f, 12f, Color.FromArgb(246, 132, 84, 52));
            StrokeRounded(graphics, 1f, 1f, 254f, 126f, 12f, Color.FromArgb(246, 43, 29, 21), 2f);
            using (var pen = new Pen(Color.FromArgb(70, 255, 228, 187), 1.1f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                graphics.DrawLine(pen, 22f, 9f, 234f, 9f);
            }
        }
    }
}
