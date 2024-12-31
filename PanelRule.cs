using System.Diagnostics;
using System.Net.Quic;

namespace Tetris
{
    internal partial class Panel
    {
        private readonly BagRandomizer<Block.E_BlockType> _randomizer = new();

        private void InitializeRandomizer()
        {
            // Initialize the randomizer
            var arr = Enum.GetValues(typeof(Block.E_BlockType));
            _randomizer.Reset(new(arr.Cast<Block.E_BlockType>().Where(e => e != Block.E_BlockType.Count)));
        }

        private Block GetRandomBlock() => new(_randomizer.Next());

        private void RandomBlock()
        {
            _currentBlock = _nextBlocks.Dequeue();
            _nextBlocks.Enqueue(GetRandomBlock());
        }

        private void OnGameOver()
        {
            this.Invalidate();
            _timer.Stop();
            MessageBox.Show("Game Over!");
        }

        private bool CheckGameOver()
        {
            foreach (var grid in _gridInfo[_MarginRowNum - 1])
            {
                if (grid.hasBlock)
                {
                    return true;
                }
            }
            return false;
        }

        private bool EatBlock()
        {
            if (CanFall() > 0)
            {
                return false;
            }

            foreach (var placeholder in _currentBlock.Placeholders)
            {
                var y = (int)Math.Ceiling(placeholder.y + _currentBlock.Pos.y + _MarginRowNum);
                var x = (int)Math.Ceiling(placeholder.x + _currentBlock.Pos.x);
                _gridInfo[y][x].hasBlock = true;
                _gridInfo[y][x].belongsTo = _currentBlock.BlockType;
            }

            // Eliminate full lines
            EliminateFullLines();

            // Check if the game is over
            if (CheckGameOver())
            {
                OnGameOver();
                Restart();
                return true;
            }

            RandomBlock();
            return true;
        }

        private bool DropBlock()
        {
            FallBottom();
            return EatBlock();
        }

        private void EliminateFullLines()
        {
            HashSet<int> fullLines = [];
            foreach (var placeholder in _currentBlock.Placeholders)
            {
                var lineFull = true;
                var y = (int)Math.Ceiling(placeholder.y + _currentBlock.Pos.y) + _MarginRowNum;
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

            if (fullLines.Count == 0)
            {
                return;
            }

            var width = _Columns * _blockSize + Padding.Horizontal;
            var startX = (ClientSize.Width - width) / 2;
            var startY = (ClientSize.Height - (_Rows + (_PrimaryBlockSize - 1) * 2) * _blockSize) / 2;

            List<int> lines = [.. fullLines.OrderBy(x => x)];

            List<Rectangle> rectangles = [];

            var currentLine = lines[0];
            Rectangle currentRect = new(startX, startY + currentLine * _blockSize, width, 0);
            foreach (var line in lines)
            {
                if (currentLine == line)
                {
                    currentRect.Height += _blockSize;
                    currentLine++;
                }
                else
                {
                    rectangles.Add(currentRect);
                    currentRect = new(startX, startY + line * _blockSize, width, _blockSize);
                    currentLine = line + 1;
                }
            }
            rectangles.Add(currentRect);

            Debug.WriteLine($"{rectangles.Count}");

            StartEflAnimation(rectangles, () =>
            {
                foreach (var line in lines)
                {
                    _gridInfo.RemoveAt(line);
                    _gridInfo.Insert(0, new GridInfo[_Columns]);
                }
            });
        }

        private bool TryBackupBlock()
        {
            (_backupBlock, _currentBlock) = (new(_currentBlock.BlockType), _backupBlock);
            if (_currentBlock is null)
            {
                RandomBlock();
            }

            return true;
        }

        private bool TryMoveBlockLeftOrRight(int direction)
        {
            _currentBlock.MoveLeftOrRight(direction);
            if (CheckCollision())
            {
                // undo the move if there is a collision
                _currentBlock.MoveLeftOrRight(-direction);
                return false;
            }
            return true;
        }

        private bool TryMoveBlockUpOrDown(float direction)
        {
            _currentBlock.MoveUpOrDown(direction);
            if (CheckCollision())
            {
                // undo the move if there is a collision
                _currentBlock.MoveUpOrDown(-direction);
                return false;
            }
            return true;
        }

        private bool TryLeftRotate()
        {
            _currentBlock.LeftRotate();
            if (CheckCollision() && !TestKickWall(Block.E_RotateDirection.Left))
            {
                // undo the rotation if there is a collision
                _currentBlock.RightRotate();
                return false;
            }
            return true;
        }

        private bool TryRightRotate()
        {
            _currentBlock.RightRotate();
            if (CheckCollision() && !TestKickWall(Block.E_RotateDirection.Right))
            {
                // undo the rotation if there is a collision
                _currentBlock.LeftRotate();
                return false;
            }
            return true;
        }

        private bool TestKickWall(Block.E_RotateDirection direction)
        {
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

        private bool CheckCollision() => CheckCollision(_currentBlock);
        private bool CheckCollision(Block block)
        {
            foreach (var placeholder in block.Placeholders)
            {
                var posX = (int)Math.Ceiling(placeholder.x + block.Pos.x);
                var posY = (int)Math.Ceiling(placeholder.y + block.Pos.y) + _MarginRowNum;

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

        private float CanFall() => CanFall(_currentBlock);
        private float CanFall(Block block)
        {
            var distance = (float)(Math.Ceiling(block.Pos.y) - block.Pos.y);

            foreach (var placeholder in block.Placeholders)
            {
                var y = (int)Math.Ceiling(placeholder.y + block.Pos.y);
                var x = (int)Math.Ceiling(placeholder.x + block.Pos.x);
                // Check if the pos is integer
                if (distance == 0)
                {
                    y++;
                }

                // Check if the bottom of the block has space to fall
                if (y >= _Rows || _gridInfo[y + _MarginRowNum][x].hasBlock)
                {
                    return 0;
                }
            }
            return distance == 0 ? 1 : distance;
        }

        private void FallBottom() { FallBottom(_currentBlock); }
        private void FallBottom(Block block)
        {
            float distance;
            while ((distance = CanFall(block)) != 0)
            {
                block.MoveUpOrDown(distance);
            }

            if (block.Pos.y % 1 != 0)
            {
                throw new Exception($"{block.Pos.y} is not an integer, distance: {distance}");
            }
        }
    }
}
