using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Dynamic;
using static Tetris.TetrisPanel;

namespace Tetris
{
    internal partial class GameControl : UserControl
    {
        private readonly FrameTimer _frameTimer = new();
        private readonly BitmapManager _bitmapManager = new();
        private int CellSize { get => _bitmapManager.CellSize; set => _bitmapManager.CellSize = value; }
        private int ThemeIndex { get; set; }

        private Label _label = new();

        public GameControl()
        {
            InitializeComponent();
            InitializeGamePaint();
            InitializeSounds();
            InitalizeGameAnimation();
            RestartGame();
            UpdateFps();

            _bitmapManager.ThemeChanged += (sender, theme) => ThemeIndex = theme switch
            {
                BitmapManager.E_Theme.Dark => 0,
                BitmapManager.E_Theme.Light => 1,
                _ => throw new NotImplementedException(),
            };
            _bitmapManager.Theme = BitmapManager.E_Theme.Dark;
            _frameTimer.Start();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CellSize = Math.Min(
                this.ClientSize.Width * 2 / 3 / (_MatrixColumns + 2),
                this.ClientSize.Height / (_MatrixRows + 2)
            );
            this.Padding = new(CellSize);
            PlayfieldLocation = new(
                (this.ClientSize.Width - _MatrixColumns * CellSize) / 2,
                (this.ClientSize.Height - _MatrixRows * CellSize) / 2
            );
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            CalculateRedrawRegion(e.ClipRectangle);
            DrawBackground(e);
            DrawPlayfield(e);

            //Debug.WriteLine($"{e.ClipRectangle}");
            //g.DrawImage(_backgrounds[(int)_bitmapManager.Theme], this.ClientRectangle);
            //g.DrawImage(_backgrounds[(int)_bitmapManager.Theme], e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);

            //int sideLength = 200;
            //int xOffset = 10;
            //int yOffset = 10;
            //int spacing = sideLength + 40; // 增加一些间距

            //Enum.GetValues(typeof(Piece.E_Type)).Cast<Piece.E_Type>().ToList().ForEach(type =>
            //{
            //    using var squareBitmap = BitmapManager.FromAlphaBitmap(127, _bitmapManager.GetPieceBitmap(type));
            //    int x = xOffset + ((int)type % 3) * spacing;
            //    int y = yOffset + ((int)type / 3) * spacing;
            //    g.DrawImage(squareBitmap, x, y);
            //});
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _frameTimer.Dispose();
                _bitmapManager.Dispose();
                DisposeSounds();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.DoubleBuffered = true;

            this.SuspendLayout();
            this.Controls.Add(_label);

            this.ResumeLayout(false);

            this.KeyDown += (sender, e) => KeyDownHandler(e);
            this.KeyUp += (sender, e) => KeyUpHandler(e);
        }

        private void UpdateFps()
        {
            _frameTimer.DelayToDo(15, UpdateFps);
            _label.Text = $"FPS: {_frameTimer.Fps:F2}";
        }
    }
}
