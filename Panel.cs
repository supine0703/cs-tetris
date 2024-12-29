using System.Diagnostics;

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
        private readonly BagRandomizer<Block.E_BlockType> _randomizer;
        private readonly List<GridInfo[]> _gridInfo;
        private readonly Queue<Block> _nextBlocks;
        private Block _currentBlock;

        public Panel()
        {
            // Initialize the randomizer
            var arr = Enum.GetValues(typeof(Block.E_BlockType));
            _randomizer = new(arr.Cast<Block.E_BlockType>().Where(e => e != Block.E_BlockType.Count).ToList());

            // Initialize the grid infos
            _gridInfo = new(_Rows + _MarginRowNum);
            foreach (var _ in Enumerable.Range(0, _Rows + _MarginRowNum))
            {
                _gridInfo.Add(new GridInfo[_Columns]);
            }

            // Initialize the blocks
            _currentBlock = new(_randomizer.Next());
            _nextBlocks = new(_PrimaryBlockSize);
            foreach (var _ in Enumerable.Range(0, _PrimaryBlockSize))
            {
                _nextBlocks.Enqueue(new Block(_randomizer.Next()));
            }

            // Initialize the timer
            InitializeTimer();

            this.KeyDown += (sender, e) => KeyDownHandler(e);
            this.KeyUp += (sender, e) => KeyUpHandler(e);
            this.DoubleBuffered = true;
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

        public void Restart()
        {
            _randomizer.RefillBag();

            // For avoid overflow
            _gridInfo.Clear();
            foreach (var _ in Enumerable.Range(0, _Rows + _MarginRowNum))
            {
                _gridInfo.Add(new GridInfo[_Columns]);
            }
            // Initialize the blocks
            _nextBlocks.Clear();
            foreach (var _ in Enumerable.Range(0, _PrimaryBlockSize))
            {
                _nextBlocks.Enqueue(new Block(_randomizer.Next()));
            }
            RandomBlock();
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
