using System.Diagnostics;
using static Tetris.Block;
using Timer = System.Windows.Forms.Timer;

namespace Tetris
{
    internal partial class Panel : UserControl
    {
        private struct GridInfo(bool hasBlock, Color color)
        {
            public bool hasBlock = hasBlock;
            public Color color = color;

            public GridInfo() : this(false, _backColor) { }
        }

        private const int _MarginRowNum = 4;
        private const int _PrimaryBlockSize = (int)Block.E_BlockType.Count - 1;
        private const int _Rows = 20;
        private const int _Columns = 10;

        private readonly List<GridInfo[]> _gridInfo = new(_Rows + _MarginRowNum);
        private readonly Queue<Block> _nextBlocks = new(_PrimaryBlockSize);
        private Block _currentBlock;
        private Block? _backupBlock;

        public Panel()
        {
            // Initialize the randomizer
            InitializeRandomizer();

            // Initialize the grid infos
            foreach (var _ in Enumerable.Range(0, _Rows + _MarginRowNum))
            {
                _gridInfo.Add(new GridInfo[_Columns]);
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

            this.KeyDown += (sender, e) => KeyDownHandler(e);
            this.KeyUp += (sender, e) => KeyUpHandler(e);
            this.DoubleBuffered = true;
        }

        private void Restart()
        {
            // Reset the randomizer
            InitializeRandomizer();

            // Reset the grid infos
            _gridInfo.Clear();
            foreach (var _ in Enumerable.Range(0, _gridInfo.Capacity))
            {
                _gridInfo.Add(new GridInfo[_Columns]);
            }

            // Reset the blocks
            _nextBlocks.Clear();
            foreach (var _ in Enumerable.Range(0, _PrimaryBlockSize))
            {
                _nextBlocks.Enqueue(GetRandomBlock());
            }

            // Reset the block
            RandomBlock();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdujustPanelSize();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawBackground(e.Graphics);
            DrawGrid(e.Graphics);
            DrawBlock(e.Graphics);
            DrawBeEatProgress(e.Graphics);
        }

        // === Key System ===
        private readonly HashSet<Keys> _pressedKeys = [];

        // === Key for falling ===
        private readonly HashSet<Keys> _canFallKeys = [Keys.S, Keys.Down];

        // === Key for horizontal moving ===
        private readonly HashSet<Keys> _canMoveLeftKeys = [Keys.A, Keys.Left];
        private readonly HashSet<Keys> _canMoveRightKeys = [Keys.D, Keys.Right];
        private readonly Timer _safeOfMoveFirts = new();
        private readonly Timer _multiMoveTimer = new();

        // === Key for rotating ===
        private readonly HashSet<Keys> _canRotateLeftKeys = [Keys.J, Keys.X];
        private readonly HashSet<Keys> _canRotateRightKeys = [Keys.K, Keys.C];

        // === Key for dropping ===
        private readonly HashSet<Keys> _canDropKeys = [Keys.W, Keys.Up];

        // === Key for backup ===
        private readonly HashSet<Keys> _canBackupKeys = [Keys.Space];

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
            _multiMoveTimer.Tick += (sender, e) => PreeHorizontalMove();
        }

        private void KeyDownHandler(KeyEventArgs e)
        {
            if (!_pressedKeys.Contains(e.KeyCode))
            {
                var changed = false;
                // Rotate
                if (_canRotateLeftKeys.Contains(e.KeyCode))
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
                // Drop
                else if (_canDropKeys.Contains(e.KeyCode))
                {
                    changed = DropBlock();
                }
                // Backup
                else if (_canBackupKeys.Contains(e.KeyCode))
                {
                    changed = TryBackupBlock();
                }

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
            var nonMove = true;
            foreach (var key in _pressedKeys)
            {
                if (_canMoveLeftKeys.Contains(key) || _canMoveRightKeys.Contains(key))
                {
                    nonMove = false;
                    break;
                }
            }
            if (nonMove)
            {
                _safeOfMoveFirts.Stop();
                _multiMoveTimer.Stop();
            }
        }

        private void PreeHorizontalMove()
        {
            var leftPressed = false;
            foreach (var key in _canMoveLeftKeys)
            {
                if (_pressedKeys.Contains(key))
                {
                    leftPressed = true;
                    break;
                }
            }
            var rightPressed = false;
            foreach (var key in _canMoveRightKeys)
            {
                if (_pressedKeys.Contains(key))
                {
                    rightPressed = true;
                    break;
                }
            }

            var changed = false;
            if (leftPressed && !rightPressed)
            {
                changed = TryMoveBlockLeftOrRight(-1);
            }
            else if (rightPressed && !leftPressed)
            {
                changed = TryMoveBlockLeftOrRight(1);
            }

            // Reset drop progress
            if (changed)
            {
                ResetAutoEatProceed();
            }
        }
    }
}
