using System.Drawing.Drawing2D;

namespace Tetris
{
    internal partial class GameControl : UserControl
    {
        private new Padding Padding { get; set; }
        private readonly FrameTimer _frameTimer = new();
        private readonly BitmapManager _bitmapManager = new() { PieceSizeScaling = _PreloadPieceSizeScaling };
        private int BorderThickness => _bitmapManager.BlockBorderThickness;
        private int CellSize { get => _bitmapManager.CellSize; set => _bitmapManager.CellSize = value; }
        private int ThemeIndex { get; set; }

        private Label _label = new();

        private List<(int priority, EventHandler handler)>? FrameTickLoadList { get; set; } = [];

        public GameControl()
        {
            InitializeFontResources();
            InitializeComponent();
            InitializeSounds();
            InitializeFrameInvalidate();
            InitializeGamePaint();
            InitalizeGameAnimation();
            InitializeKeySystem();
            // SkipOneFrameInFrameCount = 2; // There seems to be no need to skip frames

            this.Load += GameControlLoad;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CellSize = Math.Max(1, Math.Min(
                this.ClientSize.Width * 2 / 3 / (_MatrixColumns + 2),
                this.ClientSize.Height / (_MatrixRows + 3)
            ));
            this.Padding = new(CellSize);
            PlayfieldLocation = new(
                (this.ClientSize.Width - _MatrixColumns * CellSize) / 2,
                (this.ClientSize.Height - _MatrixRows * CellSize) / 2
            );
            PieceBoundingBoxSize = new(
                4 * CellSize + BorderThickness,
                4 * CellSize + BorderThickness
            );
            ResizeSettings();
            FrameInvalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            DrawBackground(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            CalculateRedrawRegion(e.ClipRectangle);

            DrawPlayfield(e);
            DrawPreviewQueue(e);
            DrawHoldQueue(e);
            DrawWaitLockProgress(e);

            //e.Graphics.CompositingQuality = CompositingQuality.HighQuality; // Unnecessary and resource-intensive

            DrawActivePiece(e);
            DrawAllAnimationFrame(e);
            DrawPauseBackground(e.Graphics);
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
            _label.AutoSize = true;
            InitializeSettings();

            this.ResumeLayout(false);
        }

        private void GameControlLoad(object? sender, EventArgs e)
        {
            LoadGameRule();

            if (this.ParentForm is not null)
            {
                this.ParentForm.Deactivate += (sender, e) =>
                {
                    if (GameState == E_GameState.Playing) { GameState = E_GameState.Paused; }
                };
            }

            if (FrameTickLoadList is not null)
            {
                foreach (var (_, handler) in FrameTickLoadList.OrderByDescending(t => t.priority))
                {
                    _frameTimer.Tick += handler;
                }
                FrameTickLoadList.Clear();
                FrameTickLoadList = null;
            }

            _bitmapManager.ThemeChanged += (sender, theme) => ThemeIndex = theme switch
            {
                BitmapManager.E_Theme.Dark => 0,
                BitmapManager.E_Theme.Light => 1,
                _ => throw new NotImplementedException(),
            };
            _bitmapManager.ThemeChanged += (sender, theme) => this.Refresh();
            _bitmapManager.Theme = BitmapManager.E_Theme.Dark;
            _frameTimer.Start();

            UpdateFps();
            BgmMuteSwitch();
        }

        private void UpdateFps()
        {
            _frameTimer.DelayToDo(15, UpdateFps);
            //_label.Text = $"FPS: {_frameTimer.Fps:F2}";
            _label.Text = $"G: {NatureGravity}, {NatureGravity * 62.5f / 60:F8}";
            _label.Refresh();
        }
    }
}
