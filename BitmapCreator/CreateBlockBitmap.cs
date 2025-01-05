using System.Drawing.Drawing2D;

namespace Tetris
{
    internal partial class BitmapManager
    {
        public static Bitmap CreateBlockBitmap(Color color, int size)
            => CreateBlockBitmap(color, size, Color.Black);

        public static Bitmap CreateBlockBitmap(Color color, int size, Color borderColor, float borderRatio = 0.1f)
            => CreateBlockBitmap(color, size, borderColor, new Thickness((int)(size * borderRatio)));

        public static Bitmap CreateBlockBitmap(Color color, int sideLength, Color borderColor, Thickness thickness)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(sideLength, 1, nameof(sideLength));
            Bitmap bitmap = new(sideLength + thickness.Horizontal, sideLength + thickness.Vertical);

            // Define the position and rectangle of the square
            var margin = (int)(sideLength * 0.2);
            Point pos = new(thickness.Left, thickness.Right);
            Rectangle outerSquareRect = new(pos, new(sideLength, sideLength));
            Rectangle innerSquareRect = new(
                pos.X + margin, pos.Y + margin,
                sideLength - margin * 2, sideLength - margin * 2
            );

            // Define the position of square's corners
            (Point TopLeft, Point TopRight, Point BottomLeft, Point BottomRight)
            outerSquareCorners = (
                outerSquareRect.Location,
                new(outerSquareRect.Right, outerSquareRect.Top),
                new(outerSquareRect.Left, outerSquareRect.Bottom),
                new(outerSquareRect.Right, outerSquareRect.Bottom)
            ),
            innerSquareCorners = (
                innerSquareRect.Location,
                new(innerSquareRect.Right, innerSquareRect.Top),
                new(innerSquareRect.Left, innerSquareRect.Bottom),
                new(innerSquareRect.Right, innerSquareRect.Bottom)
            );

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(borderColor); // Set the background color (border)

                // Draw the main body of the square (Slightly darken the center square)
                using (SolidBrush brush = new(ControlPaint.Dark(color, 0.05f)))
                {
                    g.FillRectangle(brush, outerSquareRect);
                }

                if (margin != 0)
                {
                    // Draw top highlights (bright colors fade away)
                    using (LinearGradientBrush brush = new(
                        new(outerSquareCorners.TopLeft, new Size(sideLength, margin)),
                        Color.White, ControlPaint.Light(color, 0.25f),
                        LinearGradientMode.Vertical
                    ))
                    {
                        g.FillPolygon(brush, new Point[] {
                            outerSquareCorners.TopLeft,
                            outerSquareCorners.TopRight,
                            innerSquareCorners.TopRight,
                            innerSquareCorners.TopLeft,
                        });
                    }

                    // Draw the lower side trapezoid shadow
                    using (SolidBrush brush = new(ControlPaint.Dark(color, 0.6f)))
                    {
                        g.FillPolygon(brush, new Point[] {
                            innerSquareCorners.BottomLeft,
                            innerSquareCorners.BottomRight,
                            outerSquareCorners.BottomRight,
                            outerSquareCorners.BottomLeft,
                        });
                    }

                    // Left gradient shadow
                    // (gradient from the lower vertex of the inner square to the upper vertex of the outer square)
                    using (LinearGradientBrush brush = new(
                        new Point(innerSquareRect.Left + margin, outerSquareRect.Bottom),
                        new Point(outerSquareRect.Left + margin, outerSquareRect.Top - margin),
                        ControlPaint.Dark(color, 0.6f), ControlPaint.Light(color, 0.5f)
                    ))
                    {
                        g.FillPolygon(brush, new Point[] {
                            outerSquareCorners.TopLeft,
                            innerSquareCorners.TopLeft,
                            innerSquareCorners.BottomLeft,
                            outerSquareCorners.BottomLeft,
                        });
                    }

                    // Right-side gradient shadow
                    // (gradient from the lower vertex of the inner square to the upper vertex of the outer square)
                    using (LinearGradientBrush brush = new(
                        new Point(innerSquareRect.Right + margin, outerSquareRect.Bottom),
                        new Point(outerSquareRect.Right + margin, outerSquareRect.Top - margin),
                        ControlPaint.Dark(color, 0.6f), ControlPaint.Light(color, 0.5f)
                    ))
                    {
                        g.FillPolygon(brush, new Point[] {
                            outerSquareCorners.TopRight,
                            innerSquareCorners.TopRight,
                            innerSquareCorners.BottomRight,
                            outerSquareCorners.BottomRight,
                        });
                    }
                }
            }
            return bitmap;
        }
    }
}
