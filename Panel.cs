using System.Diagnostics;
using static Tetris.Block;

namespace Tetris
{
    internal partial class Panel : UserControl
    {
        private struct GridInfo(bool hasBlock, Color color)
        {
            public bool hasBlock = hasBlock;
            public Color color = color;

            public GridInfo() : this(false, Color.White) { }
        }

        private const int _MarginRowNum = 4;
        private const int _PrimaryBlockSize = (int)Block.E_BlockType.Count - 1;
        private const int _Rows = 20;
        private const int _Columns = 10;

        private readonly List<GridInfo[]> _gridInfo = new(_Rows + _MarginRowNum);
        private readonly Queue<Block> _nextBlocks = new(_PrimaryBlockSize);
        private Block _currentBlock;

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
            DrawGrid(e.Graphics);
            DrawBlock(e.Graphics);
            DrawBeEatProgress(e.Graphics);
        }

        private void KeyDownHandler(KeyEventArgs e)
        {
            if (_currentBlock == null)
            {
                return;
            }

            if (e.KeyCode == Keys.Space)
            {
                RandomBlock();
            }
            else if (e.KeyCode switch
            {
                Keys.A => TryMoveBlockLeftOrRight(-1),
                Keys.D => TryMoveBlockLeftOrRight(1),
                Keys.W => DropBlock(),
                Keys.S => !EatBlock() && TryMoveBlockUpOrDown(1),
                Keys.J => TryLeftRotate(),
                Keys.K => TryRightRotate(),
                _ => false,
            })
            {
            }
        }

        private void KeyUpHandler(KeyEventArgs e)
        {
            //Debug.WriteLine(e.KeyCode);
        }
    }
}
