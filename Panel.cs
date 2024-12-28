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

            // For avoid overflow
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

            this.KeyDown += (sender, e) => KeyDownHandler(e);
            this.DoubleBuffered = true;
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
            this.Invalidate();
        }

        // ...
        private void OnGameOver()
        {
            MessageBox.Show("Game Over!");
        }

        private void KeyDownHandler(KeyEventArgs e)
        {
            if (_currentBlock == null)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Space:
                    RandomBlock();
                    Invalidate();
                    break;
                case Keys.A:
                    TryMoveBlockLeftOrRight(-1);
                    break;
                case Keys.D:
                    TryMoveBlockLeftOrRight(1);
                    break;
                case Keys.W:
                    DropBlock();
                    Invalidate();
                    break;
                case Keys.S:
                    if (!EatBlock())
                    {
                        TryMoveBlockUpOrDown(1);
                    }
                    Invalidate();
                    break;
                case Keys.J:
                    TryLeftRotate();
                    break;
                case Keys.K:
                    TryRightRotate();
                    break;
                default:
                    return;
            }
        }

        private bool EatBlock()
        {
            if (_currentBlock == null)
            {
                return false;
            }
            if (CanFall())
            {
                return false;
            }

            foreach (var position in _currentBlock.Placeholders)
            {
                var y = (int)(position.y + _currentBlock.Pos.y + _MarginRowNum);
                var x = (int)(position.x + _currentBlock.Pos.x);
                _gridInfo[y][x].hasBlock = true;
                _gridInfo[y][x].color = GetColor(_currentBlock.BlockType);
            }

            // Eliminate full lines
            HashSet<int> fullLines = [];
            foreach (var position in _currentBlock.Placeholders)
            {
                var lineFull = true;
                var y = (int)(position.y + _currentBlock.Pos.y + _MarginRowNum);
                foreach (int i in Enumerable.Range(0, _Columns))
                {
                    if (!_gridInfo[y][i].hasBlock)
                    {
                        lineFull = false;
                        break;
                    }
                }
                if (lineFull)
                {
                    fullLines.Add(y);
                }
            }

            fullLines = [.. fullLines.OrderByDescending(x => -x)];
            foreach (var line in fullLines)
            {
                _gridInfo.RemoveAt(line);
                _gridInfo.Insert(0, new GridInfo[_Columns]);
            }

            // Check if the game is over
            if (CheckGameOver())
            {
                Restart();
                return true;
            }

            RandomBlock();
            return true;
        }

        private bool DropBlock()
        {
            if (_currentBlock == null)
            {
                return false;
            }
            while (CanFall())
            {
                _currentBlock.MoveUpOrDown(1);
            }
            return EatBlock();
        }

        private void TryMoveBlockLeftOrRight(int direction)
        {
            if (_currentBlock == null)
            {
                return;
            }

            _currentBlock.MoveLeftOrRight(direction);
            if (CheckCollision(_currentBlock))
            {
                // undo the move if there is a collision
                _currentBlock.MoveLeftOrRight(-direction);
            }
            Invalidate(); // Redraw the grid and block
        }

        private void TryMoveBlockUpOrDown(int direction)
        {
            if (_currentBlock == null)
            {
                return;
            }

            _currentBlock.MoveUpOrDown(direction);
            if (CheckCollision(_currentBlock))
            {
                // undo the move if there is a collision
                _currentBlock.MoveUpOrDown(-direction);
            }
            Invalidate(); // Redraw the grid and block
        }

        private void TryLeftRotate()
        {
            if (_currentBlock == null)
            {
                return;
            }

            _currentBlock.LeftRotate();
            if (CheckCollision(_currentBlock) && !TestKickWall(Block.E_RotateDirection.Left))
            {
                // undo the rotation if there is a collision
                _currentBlock.RightRotate();
            }
            Invalidate(); // Redraw the grid and block
        }

        private void TryRightRotate()
        {
            if (_currentBlock == null)
            {
                return;
            }

            _currentBlock.RightRotate();
            if (CheckCollision(_currentBlock) && !TestKickWall(Block.E_RotateDirection.Right))
            {
                // undo the rotation if there is a collision
                _currentBlock.LeftRotate();
            }
            Invalidate(); // Redraw the grid and block
        }

        private bool TestKickWall(Block.E_RotateDirection direction)
        {
            if (_currentBlock == null)
            {
                return false;
            }

            foreach (var diffPos in _currentBlock.GetPrevKickWallTable(direction))
            {
                // Coordinates of the kick wall table are different from the block's position,
                // which is relative to the left-bottom corner, but panel's grid is relative to the left-top corner.
                // So the X coordinate needs to be Added, and the Y coordinate needs to be subtracted from the block's position.
                var expectedPos = diffPos;
                expectedPos.y = -expectedPos.y;
                expectedPos += _currentBlock.Pos;
                Block block = new(_currentBlock.BlockType, _currentBlock.FacingDirection, expectedPos);
                if (!CheckCollision(block))
                {
                    _currentBlock = block;
                    return true;
                }
            }

            return false;
        }

        private bool CheckGameOver()
        {
            if (_currentBlock == null)
            {
                return false;
            }

            foreach (var grid in _gridInfo[_MarginRowNum - 1])
            {
                if (grid.hasBlock)
                {
                    OnGameOver();
                    return true;
                }
            }
            return false;
        }

        private bool CheckCollision(Block block)
        {
            foreach (var position in block.Placeholders)
            {
                var posX = (int)(position.x + block.Pos.x);
                var posY = (int)(position.y + block.Pos.y + _MarginRowNum);

                // Check if the block is out of bounds
                if (posX < 0 || posX >= _Columns || posY < 0 || posY >= _Rows + _MarginRowNum)
                {
                    return true;
                }

                // Check if the block collides with another block
                if (_gridInfo[posY][posX].hasBlock)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CanFall(Block block)
        {
            if (block == null)
            {
                return false;
            }

            foreach (var position in block.Placeholders)
            {
                var y = (int)(position.y + block.Pos.y) + 1;
                var x = (int)(position.x + block.Pos.x);
                if (y >= _Rows || _gridInfo[y + _MarginRowNum][x].hasBlock)
                {
                    return false;
                }
            }
            return true;
        }

        private bool CanFall()
        {
            return CanFall(_currentBlock);
        }

        private void RandomBlock()
        {
            _currentBlock = _nextBlocks.Dequeue();
            _nextBlocks.Enqueue(new Block(_randomizer.Next()));
        }
    }
}
