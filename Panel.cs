using System.Diagnostics;
using System.Drawing.Text;
using System.Reflection;

namespace Tetris
{
    internal partial class Panel : UserControl
    {
        public enum E_CellState
        {
            Empty = 0, Clearing, Occupied,
        }

        private struct CellInfo(E_CellState state, Block.E_Type belongsTo)
        {
            public E_CellState State { get; set; } = state;
            public Block.E_Type BelongsTo { get; set; } = belongsTo;

            public CellInfo() : this(E_CellState.Empty, Block.E_Type.Count) { }
        }

        private const int _MarginRowNum = 5;
        private const int _PrimaryBlockSize = (int)Block.E_Type.Count - 1;
        private const int _Rows = 20;
        private const int _Columns = 10;

        // Coordinate system: (0, 0) is the top-left corner, x is right, y is down,
        // the top of the playfield is y = -_MarginRowNum, the bottom is y = _Rows - 1
        private readonly List<CellInfo[]> _playfieldInfo = new(_Rows + _MarginRowNum);
        private readonly Queue<Block> _nextBlocks = new(_PrimaryBlockSize);
        private Block? _currentBlock;
        private Block? _holdBlock;

        private bool Pause { get; set; } = false;

        private PrivateFontCollection _pfc = new();

        public Panel()
        {
            // Initialize the iconfont
            InitializeIconfont();

            // Initialize the sounds
            InitializeSounds();

            // Initialize the randomizer, playfield info, blocks, hold block
            Restart();

            // Initialize the timer
            InitializeTimer();

            // Initialize the key system
            InitializeKeySystem();

            // Initialize the panel settings
            InitializeSettings();

            this.KeyDown += (sender, e) => KeyDownHandler(e);
            this.KeyUp += (sender, e) => KeyUpHandler(e);
            this.DoubleBuffered = true;
            this.Load += new EventHandler(PanelLoad);
        }

        private void InitializeIconfont()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();

            // Find the iconfont file in the resources
            var iconfontName = resourceNames.FirstOrDefault(name => name.Contains("iconfont"))
                ?? throw new Exception("Iconfont file not found.");

            using var fontStream = assembly.GetManifestResourceStream(iconfontName)
                ?? throw new Exception($"Failed to load the iconfont file: {iconfontName}");

            var fontData = new byte[fontStream.Length];
            fontStream.Read(fontData, 0, fontData.Length);
            fontStream.Close();

            unsafe
            {
                fixed (byte* ptr = fontData)
                {
                    _pfc.AddMemoryFont((IntPtr)ptr, fontData.Length);
                }
            }
        }

        private void Restart()
        {
            // Reset the randomizer
            InitializeRandomizer();

            // Reset the playfield info
            _playfieldInfo.Clear();
            Enumerable.Range(0, _playfieldInfo.Capacity).ToList().ForEach(
                _ => _playfieldInfo.Add(new CellInfo[_Columns])
            );

            // Reset the blocks
            _nextBlocks.Clear();
            Enumerable.Range(0, _PrimaryBlockSize).ToList().ForEach(
                _ => _nextBlocks.Enqueue(GetRandomBlock())
            );

            // Reset the hold block
            _holdBlock = null;
            RandomBlock();
            GameOver = false;

            // Update pause message and animation
            PauseButtonClick();
            PauseButtonClick();
            ResetAllAnimation();
        }

        private void PanelLoad(object? sender, EventArgs e)
        {
            if (this.ParentForm is not null)
            {
                this.ParentForm.Deactivate += (sender, e) => PauseButtonClick();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdujustPanelSize();
            ResizeSettings();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawBackground(e.Graphics);
            DrawPlayfield(e.Graphics);
            DrawBlock(e.Graphics);
            DrawBeEatProgress(e.Graphics);
            DrawAllAnimationFrame(e.Graphics);
            DrawPauseBackground(e.Graphics);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeSounds();
            }
            base.Dispose(disposing);
        }
    }
}
