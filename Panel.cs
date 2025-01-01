using System.Diagnostics;
using System.Drawing.Text;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

        // For the game font
        private readonly List<string> _fontResources = [
            "iconfont", "JetBrainsMono",
        ];
        private PrivateFontCollection _pfc = new();
        // Maybe load font exception if not locked, unknown reason
        private readonly object _fontLoadLock = new(); 

        // The Stopwatch keeps track of the time the game is played
        private readonly Stopwatch _stopwatch = new();

        public Panel()
        {
            // Initialize the font
            InitializeFontResources();

            // Initialize the sounds
            InitializeSounds();

            this.KeyDown += (sender, e) => KeyDownHandler(e);
            this.KeyUp += (sender, e) => KeyUpHandler(e);
            this.DoubleBuffered = true;
            this.Load += PanelLoad;
            
            _stopwatch.Start();
        }

        private void InitializeFontResources()
        {
            lock (_fontLoadLock)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();

                foreach (var value in _fontResources)
                {
                    var name = resourceNames.FirstOrDefault(n => n.Contains($"{value}"))
                        ?? throw new Exception($"Cannot find the {value} font resource");

                    using var stream = assembly.GetManifestResourceStream(name)
                        ?? throw new Exception($"Failed to load the font resource: {name}");

                    var data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);

                    unsafe
                    {
                        fixed (byte* ptr = data)
                        {
                            _pfc.AddMemoryFont((IntPtr)ptr, data.Length);
                        }
                    }
                }

                Debug.WriteLine($"Font count: {_pfc.Families.Length}");
                foreach (var f in _pfc.Families)
                {
                    Debug.WriteLine(f.Name);
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
            _stopwatch.Restart();
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
            // Initialize the randomizer, playfield info, blocks, hold block
            Restart();

            // Initialize the key system
            InitializeKeySystem();

            // Initialize the panel settings
            InitializeSettings();

            // Initialize the timer
            InitializeTimer();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdujustPanelSize();
            ResizeSettings();
            this.Invalidate();
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
                DisposeTimer();
            }
            base.Dispose(disposing);
        }
    }
}
