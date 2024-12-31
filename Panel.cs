using System.Diagnostics;
using System.Drawing.Text;
using System.Reflection;
using Timer = System.Windows.Forms.Timer;

namespace Tetris
{
    internal partial class Panel : UserControl
    {
        private struct CellInfo(bool wasOccupied, Block.E_BlockType belongsTo)
        {
            public bool wasOccupied = wasOccupied;
            public Block.E_BlockType belongsTo = belongsTo;

            public CellInfo() : this(false, Block.E_BlockType.Count) { }
        }

        private const int _MarginRowNum = 5;
        private const int _PrimaryBlockSize = (int)Block.E_BlockType.Count - 1;
        private const int _Rows = 20;
        private const int _Columns = 10;

        private readonly List<CellInfo[]> _playfieldInfo = new(_Rows + _MarginRowNum);
        private readonly Queue<Block> _nextBlocks = new(_PrimaryBlockSize);
        private Block _currentBlock;
        private Block? _holdBlock;

        private bool Pause { get; set; } = false;

        private PrivateFontCollection _pfc = new();

        public Panel()
        {
            // Initialize the iconfont
            InitializeIconfont();

            // Initialize the randomizer
            InitializeRandomizer();

            // Initialize the playfield info
            foreach (var _ in Enumerable.Range(0, _Rows + _MarginRowNum))
            {
                _playfieldInfo.Add(new CellInfo[_Columns]);
            }

            // Initialize the blocks
            _currentBlock = GetRandomBlock();
            foreach (var _ in Enumerable.Range(0, _PrimaryBlockSize))
            {
                _nextBlocks.Enqueue(GetRandomBlock());
            }

            // Initialize the timer
            InitializeTimer();

            // Initialize the key system
            InitializeKeySystem();

            // Initialize the panel settings
            InitializeSettings();

            this.KeyDown += (sender, e) => KeyDownHandler(e);
            this.KeyUp += (sender, e) => KeyUpHandler(e);
            this.DoubleBuffered = true;
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
            foreach (var _ in Enumerable.Range(0, _playfieldInfo.Capacity))
            {
                _playfieldInfo.Add(new CellInfo[_Columns]);
            }

            // Reset the blocks
            _nextBlocks.Clear();
            foreach (var _ in Enumerable.Range(0, _PrimaryBlockSize))
            {
                _nextBlocks.Enqueue(GetRandomBlock());
            }

            // Reset the block
            _holdBlock = null;
            RandomBlock();

            // Start timer
            _timer.Start();
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
        }

        // === Key System ===
        private readonly HashSet<Keys> _pressedKeys = [];

        // === Key for hard dropping ===
        private readonly HashSet<Keys> _canHardDropKeys = [Keys.W, Keys.Up];

        // === Key for dropping ===
        private readonly HashSet<Keys> _canSoftDropKeys = [Keys.S, Keys.Down];

        // === Key for horizontal moving ===
        private readonly HashSet<Keys> _canMoveLeftKeys = [Keys.A, Keys.Left];
        private readonly HashSet<Keys> _canMoveRightKeys = [Keys.D, Keys.Right];
        private readonly Timer _safeOfMoveFirts = new();
        private readonly Timer _multiMoveTimer = new();

        // === Key for rotating ===
        private readonly HashSet<Keys> _canRotateLeftKeys = [Keys.J, Keys.X];
        private readonly HashSet<Keys> _canRotateRightKeys = [Keys.K, Keys.C];

        // === Key for hold ===
        private readonly HashSet<Keys> _canHoldKeys = [Keys.Space];

        // Allow the arrow keys to be used in the game
        protected override bool ProcessDialogKey(Keys key)
        {
            HashSet<Keys> allowKeys = [Keys.Up, Keys.Down, Keys.Left, Keys.Right];
            if (allowKeys.Contains(key))
            {
                return false;
            }
            return base.ProcessDialogKey(key);
        }

        private void InitializeKeySystem()
        {
            _safeOfMoveFirts.Interval = 120;
            _safeOfMoveFirts.Tick += (sender, e) => _safeOfMoveFirts.Stop();
            _safeOfMoveFirts.Tick += (sender, e) => _multiMoveTimer.Start();
            _multiMoveTimer.Interval = 40;
            _multiMoveTimer.Tick += (sender, e) => PressHorizontalMove();
        }

        private void KeyDownHandler(KeyEventArgs e)
        {
            if (e.Modifiers != Keys.None)
            {
                return;
            }

            if (!_pressedKeys.Contains(e.KeyCode))
            {
                var changed = false;
                if (e.KeyCode == Keys.P)
                {
                    PauseButtonClick();
                }
                else if (e.KeyCode == Keys.R)
                {
                    RestartButtonClick();
                }
                else if (e.KeyCode == Keys.T)
                {
                    ThemeButtonClick();
                }
                else if (Pause)
                {
                }
                // Rotate
                else if (_canRotateLeftKeys.Contains(e.KeyCode))
                {
                    changed = TryLeftRotate();
                }
                else if (_canRotateRightKeys.Contains(e.KeyCode))
                {
                    changed = TryRightRotate();
                }
                // Move
                else if (_canMoveLeftKeys.Contains(e.KeyCode))
                {
                    _safeOfMoveFirts.Start();
                    changed = TryMoveBlockLeftOrRight(-1);
                }
                else if (_canMoveRightKeys.Contains(e.KeyCode))
                {
                    _safeOfMoveFirts.Start();
                    changed = TryMoveBlockLeftOrRight(1);
                }
                // Hard drop
                else if (_canHardDropKeys.Contains(e.KeyCode))
                {
                    changed = HardDropBlock();
                }
                // Hold
                else if (_canHoldKeys.Contains(e.KeyCode))
                {
                    changed = TryHoldBlock();
                }
                else if (e.KeyCode == Keys.O)
                {
                    Size boardSize = new(_Columns * _blockSize, _Rows * _blockSize / 6);
                    var leftX = (ClientSize.Width - boardSize.Width) * 0.5f;
                    var leftY = (ClientSize.Height - boardSize.Height * 6) * 0.5f;
                    StartCflAnimation([new(new((int)leftX, (int)leftY), boardSize), new(new((int)leftX, (int)leftY + 400), boardSize)]);
                }
                // ...

                // Reset drop progress
                if (changed)
                {
                    ResetAutoEatProceed();
                }
            }

            _pressedKeys.Add(e.KeyCode);
        }

        private void KeyUpHandler(KeyEventArgs e)
        {
            _pressedKeys.Remove(e.KeyCode);

            // Stop the multi move timer
            if (_pressedKeys.All(key => !_canMoveLeftKeys.Contains(key) && !_canMoveRightKeys.Contains(key)))
            {
                _safeOfMoveFirts.Stop();
                _multiMoveTimer.Stop();
            }
        }

        private void PressHorizontalMove()
        {
            if (Pause)
            {
                _safeOfMoveFirts.Stop();
                _multiMoveTimer.Stop();
                _pressedKeys.Clear();
            }

            var move = (
                (_canMoveLeftKeys.Any(key => _pressedKeys.Contains(key)) ? -1 : 0) +
                (_canMoveRightKeys.Any(key => _pressedKeys.Contains(key)) ? 1 : 0)
            );

            // Reset drop progress
            if (move != 0 && TryMoveBlockLeftOrRight(move))
            {
                ResetAutoEatProceed();
            }
        }
    }
}
