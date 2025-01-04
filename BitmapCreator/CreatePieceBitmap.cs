using NAudio.Gui;
using NAudio.Utils;
using static System.Windows.Forms.AxHost;
using static Tetris.BitmapManager;

namespace Tetris
{
    internal partial class BitmapManager
    {
        public static Bitmap CreateBlockBitmapOf(E_Theme theme, Piece.E_Type type, int cellSize, int borderThickness)
            => CreateBlockBitmapOf(theme, 255, type, cellSize, borderThickness);

        public static Bitmap CreateBlockBitmapOf(E_Theme theme, int alpha, Piece.E_Type type, int cellSize, int borderThickness)
        {
            (var color, var borderColor) = theme switch
            {
                E_Theme.Dark => (Piece.GetColors(type)[0], Color.White),
                E_Theme.Light => (Piece.GetColors(type)[1], Color.Black),
                _ => throw new ArgumentOutOfRangeException(nameof(theme)),
            };
            return CreateBlockBitmap(color, cellSize - borderThickness, borderColor, new Thickness(borderThickness));
        }

        public static Bitmap CreatePieceBitmap(E_Theme theme, Piece.E_Type piece, int cellSize, int borderThickness)
            => CreatePieceBitmap(theme, new Piece(piece), cellSize, borderThickness);

        public static Bitmap CreatePieceBitmap(E_Theme theme, Piece piece, int cellSize, int borderThickness)
        {
            var sideLength = cellSize * 4 + borderThickness; // (borderThickness / 2) * 2
            Bitmap bitmap = new(sideLength, sideLength);

            (var color, var borderColor) = theme switch
            {
                E_Theme.Dark => (piece.Colors[0], Color.White),
                E_Theme.Light => (piece.Colors[1], Color.Black),
                _ => throw new ArgumentOutOfRangeException(nameof(theme)),
            };
            using var block = CreateBlockBitmap(color, cellSize - borderThickness, borderColor, new Thickness(borderThickness));

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent); // Set the background color (transparent)

                // Draw the block of the piece
                foreach (var pos in piece.Placeholders)
                {
                    g.DrawImage(block, pos.ToPointF(cellSize));
                }

                // Draw the point in the center of the piece
                var radius = cellSize * 0.09372f;
                var center = (piece.RotateCenter * cellSize + new Position(borderThickness * 0.5f)).ToPointF();
                var centerColor = borderColor;
                if (piece.Type == Piece.E_Type.I || piece.Type == Piece.E_Type.O)
                {
                    var newRadius = radius * 1.78f;
                    using (SolidBrush brush = new(centerColor))
                    {
                        g.FillEllipse(brush, center.X - newRadius, center.Y - newRadius, newRadius * 2, newRadius * 2);
                    }
                    centerColor = color;
                }
                using (SolidBrush brush = new(centerColor))
                {
                    g.FillEllipse(brush, center.X - radius, center.Y - radius, radius * 2, radius * 2);
                }
            }
            return bitmap;
        }
    }
}
