using System.Drawing.Imaging;
using System.Linq;

namespace Tetris
{
    internal partial class BitmapManager : IDisposable
    {
        public static Bitmap FromAlphaBitmap(int alpha, Bitmap bitmap)
        {
            Bitmap alphaBitmap = new(bitmap.Width, bitmap.Height);
            using (Graphics g = Graphics.FromImage(alphaBitmap))
            {
                g.Clear(Color.Transparent);

                using ImageAttributes imageAttributes = new();
                imageAttributes.SetColorMatrix(new ColorMatrix { Matrix33 = alpha / 255.0f });
                g.DrawImage(
                    bitmap,
                    new Rectangle(new(0, 0), bitmap.Size),
                    0, 0, bitmap.Width, bitmap.Height,
                    GraphicsUnit.Pixel, imageAttributes
                );
            }
            return alphaBitmap;
        }

        public enum E_Theme { Dark = 0, Light }
        public static int E_ThemeCount => Enum.GetValues(typeof(E_Theme)).Length;

        public delegate void ThemeChangedEventHandler(object sender, E_Theme theme);
        public event ThemeChangedEventHandler? ThemeChanged;
        public E_Theme Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                ThemeChanged?.Invoke(this, _theme);
            }
        }
        private E_Theme _theme;

        public delegate void CellResizedEventHandler(object sender, int cellSize);
        public event CellResizedEventHandler? CellResized;
        public int CellSize
        {
            get => _cellSize;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1, nameof(CellSize));
                if (value != _cellSize)
                {
                    _cellSize = value;
                    CellResize(_cellSize);
                    CellResized?.Invoke(this, _cellSize);
                }
            }
        }
        private int _cellSize;

        public int BlockBorderThickness { get; private set; } = 2;
        public int PieceBorderThickness { get; private set; } = 2;
        private Bitmap[][]? _blockBitmaps = null;
        private Bitmap[][]? _pieceBitmaps = null;

        public void Dispose()
        {
            _blockBitmaps?.ToList().ForEach(bitmaps => bitmaps.ToList().ForEach(bitmap => bitmap.Dispose()));
            _pieceBitmaps?.ToList().ForEach(bitmaps => bitmaps.ToList().ForEach(bitmap => bitmap.Dispose()));
        }

        public Bitmap GetBlockBitmap(Piece.E_Type type) => _blockBitmaps?[(int)Theme][(int)type]
            ?? throw new Exception("BitmapManager are not initialized. Please set 'CellSize' first.");
        public Bitmap GetPieceBitmap(Piece.E_Type type) => _pieceBitmaps?[(int)Theme][(int)type]
            ?? throw new Exception("BitmapManager are not initialized. Please set 'CellSize' first.");

        private void CellResize(int cellSize)
        {
            BlockBorderThickness = Math.Max(1, cellSize / 15);
            PieceBorderThickness = Math.Max(2, cellSize / 10);
            // Dispose the old block bitmaps and create new ones
            _blockBitmaps?.ToList().ForEach(bitmaps => bitmaps.ToList().ForEach(bitmap => bitmap.Dispose()));
            _blockBitmaps = Enum.GetValues(typeof(E_Theme)).Cast<E_Theme>().Select(
                theme => Enum.GetValues(typeof(Piece.E_Type)).Cast<Piece.E_Type>().Select(
                    type => CreateBlockBitmapOf(theme, type, cellSize, BlockBorderThickness)
                ).ToArray()
            ).ToArray();
            // Dispose the old piece bitmaps and create new ones
            _pieceBitmaps?.ToList().ForEach(bitmaps => bitmaps.ToList().ForEach(bitmap => bitmap.Dispose()));
            _pieceBitmaps = Enum.GetValues(typeof(E_Theme)).Cast<E_Theme>().Select(
                theme => Enum.GetValues(typeof(Piece.E_Type)).Cast<Piece.E_Type>().Select(
                    type => CreatePieceBitmap(theme, type, cellSize, PieceBorderThickness)
                ).ToArray()
            ).ToArray();
        }
    }
}
