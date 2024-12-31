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

        private void OnTopOut()
        {
            this.Invalidate();
            _timer.Stop();
            MessageBox.Show("Game Over!");
        }

        private bool CheckTopOut()
        {
            foreach (var cell in _playfieldInfo[_MarginRowNum - 1])
            {
                if (cell.wasOccupied)
                {
                    return true;
                }
            }
            return false;
        }

        private bool EatBlock()
        {
            if (CanDrop() > 0)
            {
                return false;
            }

            foreach (var placeholder in _currentBlock.Placeholders)
            {
                var y = (int)Math.Ceiling(placeholder.y + _currentBlock.Pos.y + _MarginRowNum);
                var x = (int)Math.Ceiling(placeholder.x + _currentBlock.Pos.x);
                _playfieldInfo[y][x].wasOccupied = true;
                _playfieldInfo[y][x].belongsTo = _currentBlock.BlockType;
            }

            // Clear full lines
            ClearFullLines();

            // Check if the game is over
            if (CheckTopOut())
            {
                OnTopOut();
                Restart();
                return true;
            }

            RandomBlock();
            return true;
        }

        private bool HardDropBlock()
        {
            DropBottom();
            return EatBlock();
        }

        private void ClearFullLines()
        {
            HashSet<int> fullLines = [];
            foreach (var placeholder in _currentBlock.Placeholders)
            {
                var lineFull = true;
                var y = (int)Math.Ceiling(placeholder.y + _currentBlock.Pos.y) + _MarginRowNum;
                foreach (int i in Enumerable.Range(0, _Columns))
                {
                    if (!_playfieldInfo[y][i].wasOccupied)
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

            var width = _Columns * _blockSize;
            var startX = (ClientSize.Width - width) / 2;
            var startY = (ClientSize.Height - (_Rows + (_PrimaryBlockSize - 1) * 2) * _blockSize) / 2;

            List<int> lines = [.. fullLines.OrderBy(x => x)];
            StartCflAnimation([.. lines.Select(line =>
            {
                for (int i = 0; i < _playfieldInfo[line].Length; i++)
                {
                    _playfieldInfo[line][i].belongsTo = Block.E_BlockType.Count;
                }
                return new Rectangle(startX, startY + line * _blockSize, width, _blockSize);
            })], () =>
            {
                foreach (var line in lines)
                {
                    _playfieldInfo.RemoveAt(line);
                    _playfieldInfo.Insert(0, new CellInfo[_Columns]);
                }
            });
        }

        private bool TryHoldBlock()
        {
            (_holdBlock, _currentBlock) = (new(_currentBlock.BlockType), _holdBlock);
            if (_currentBlock is null)
            {
                RandomBlock();
            }

            return true;
        }

        private bool TryMoveBlockLeftOrRight(int distance)
        {
            _currentBlock.MoveLeftOrRight(distance);
            if (CheckCollision())
            {
                // undo the move if there is a collision
                _currentBlock.MoveLeftOrRight(-distance);
                return false;
            }
            return true;
        }

        private bool TryMoveBlockUpOrDown(float distance)
        {
            _currentBlock.MoveUpOrDown(distance);
            if (CheckCollision())
            {
                // undo the move if there is a collision
                _currentBlock.MoveUpOrDown(-distance);
                return false;
            }
            return true;
        }

        private bool TryLeftRotate()
        {
            _currentBlock.LeftRotate();
            if (CheckCollision() && !TestWallKick(Block.E_Rotation.Left))
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
            if (CheckCollision() && !TestWallKick(Block.E_Rotation.Right))
            {
                // undo the rotation if there is a collision
                _currentBlock.LeftRotate();
                return false;
            }
            return true;
        }

        private bool TestWallKick(Block.E_Rotation direction)
        {
            foreach (var diffPos in _currentBlock.GetPrevWallKickTable(direction))
            {
                // Coordinates of the wall kick table are different from the block's position,
                // which is relative to the left-bottom corner, but panel's grid is relative to the left-top corner.
                // So the X coordinate needs to be Added, and the Y coordinate needs to be subtracted from the block's position.
                var expectedPos = diffPos;
                expectedPos.y = -expectedPos.y;
                expectedPos += _currentBlock.Pos;
                Block block = new(_currentBlock.BlockType, _currentBlock.Orientation, expectedPos);
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
                if (_playfieldInfo[posY][posX].wasOccupied)
                {
                    return true;
                }
            }
            return false;
        }

        private float CanDrop() => CanDrop(_currentBlock);
        private float CanDrop(Block block)
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

                // Check if the bottom of the block has space to drop
                if (y >= _Rows || _playfieldInfo[y + _MarginRowNum][x].wasOccupied)
                {
                    return 0;
                }
            }
            return distance == 0 ? 1 : distance;
        }

        private void DropBottom() { DropBottom(_currentBlock); }
        private void DropBottom(Block block)
        {
            float distance;
            while ((distance = CanDrop(block)) != 0)
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
