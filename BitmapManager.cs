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
        public void NextTheme() => Theme = (E_Theme)(((int)Theme + 1) % E_ThemeCount);

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
        private int _cellSize = 0;

        public float BlockSizeScaling
        {
            get => _blockSizeScaling;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(float.IsNaN(value) ? 0 : value, 0, nameof(BlockSizeScaling));
                if (value != _blockSizeScaling) { _blockSizeScaling = value; CellResize(_cellSize); }
            }
        }
        private float _blockSizeScaling = 1.0f;
        public float PieceSizeScaling
        {
            get => _pieceSizeScaling;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(float.IsNaN(value) ? 0 : value, 0, nameof(PieceSizeScaling));
                if (value != _pieceSizeScaling) { _pieceSizeScaling = value; CellResize(_cellSize); }
            }
        }
        private float _pieceSizeScaling = 1.0f;

        public int BlockBorderThickness { get; private set; } = 2;
        public int PieceBorderThickness { get; private set; } = 2;
        private List<List<Bitmap>>? _blockBitmaps = null;
        private List<List<Bitmap>>? _pieceBitmaps = null;

        // For frame animation
        public delegate Bitmap BitmapFactory(int cellSize, int frame);
        private readonly Dictionary<string, (BitmapFactory creator, List<Bitmap?> a)> _frameAnimations = [];

        public void RegisterFrameAnimation(string name, int frame, BitmapFactory creator)
        { _frameAnimations[name] = (creator, [.. new Bitmap?[frame]]); }

        public void Dispose()
        {
            _blockBitmaps?.ForEach(bitmaps => bitmaps.ForEach(bitmap => bitmap.Dispose()));
            _pieceBitmaps?.ForEach(bitmaps => bitmaps.ForEach(bitmap => bitmap.Dispose()));
            _frameAnimations.Values.ToList().ForEach(pair => pair.a.ForEach(bitmap => bitmap?.Dispose()));
            _frameAnimations.Clear();
        }

        public Bitmap GetBlockBitmap(Piece.E_Type type) => _blockBitmaps?[(int)Theme][(int)type]
            ?? throw new Exception("BitmapManager are not initialized. Please set 'CellSize' first.");
        public Bitmap GetPieceBitmap(Piece.E_Type type) => _pieceBitmaps?[(int)Theme][(int)type]
            ?? throw new Exception("BitmapManager are not initialized. Please set 'CellSize' first.");
        public Bitmap GetFrameAnimation(string name, int frame) => _frameAnimations[name].a[frame]
            ?? (_frameAnimations[name].a[frame] = _frameAnimations[name].creator(CellSize, frame));

        private void CellResize(int cellSize)
        {
            if (cellSize <= 0) { return; }
            ResizeBlockAndPieceBitmaps(cellSize);
            ResizeFrameAnimations();
        }

        private void ResizeBlockAndPieceBitmaps(int cellSize)
        {
            if (cellSize <= 0) { return; }

            // Resize block and piece bitmaps
            var blockCellSize = Math.Max(3, (int)(cellSize * BlockSizeScaling));
            BlockBorderThickness = Math.Max(2, blockCellSize / 15);
            DisposeAndCreateBitmaps(
                ref _blockBitmaps, (theme, type) => CreateBlockBitmapOf(theme, type, blockCellSize, BlockBorderThickness)
            );

            var pieceCellSize = Math.Max(3, (int)(cellSize * PieceSizeScaling));
            PieceBorderThickness = Math.Max(2, pieceCellSize / 15);
            DisposeAndCreateBitmaps(
                ref _pieceBitmaps, (theme, type) => CreatePieceBitmap(theme, type, pieceCellSize, PieceBorderThickness)
            );

            void DisposeAndCreateBitmaps(ref List<List<Bitmap>>? bmps, Func<E_Theme, Piece.E_Type, Bitmap> CreateBitmap)
            {
                // Dispose old bitmaps
                bmps?.ForEach(bitmaps => bitmaps.ForEach(bitmap => bitmap.Dispose()));

                // Create new bitmaps
                bmps = [.. Enum.GetValues(typeof(E_Theme)).Cast<E_Theme>().Select(
                    theme => Enum.GetValues(typeof(Piece.E_Type)).Cast<Piece.E_Type>().Select(
                        type => CreateBitmap(theme, type)
                    ).ToList()
                )];
            }
        }
    
        private void ResizeFrameAnimations()
        {
            // Clear old bitmaps
            foreach (var (_, bitmaps) in _frameAnimations.Values)
            {
                foreach (var i in Enumerable.Range(0, bitmaps.Count)) 
                {
                    bitmaps[i]?.Dispose();
                    bitmaps[i] = null;
                }
            }
        }
    }
}
