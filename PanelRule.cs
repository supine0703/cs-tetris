namespace Tetris
{
    internal partial class Panel
    {
        private void OnGameOver()
        {
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
            while (CanFall())
            {
                _currentBlock.MoveUpOrDown(1);
            }
            return EatBlock();
        }

        private void EliminateFullLines()
        {
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
        }

        private bool TryMoveBlockLeftOrRight(int direction)
        {
            _currentBlock.MoveLeftOrRight(direction);
            if (CheckCollision(_currentBlock))
            {
                // undo the move if there is a collision
                _currentBlock.MoveLeftOrRight(-direction);
                return false;
            }
            return true;
        }

        private bool TryMoveBlockUpOrDown(int direction)
        {
            _currentBlock.MoveUpOrDown(direction);
            if (CheckCollision(_currentBlock))
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
            if (CheckCollision(_currentBlock) && !TestKickWall(Block.E_RotateDirection.Left))
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
            if (CheckCollision(_currentBlock) && !TestKickWall(Block.E_RotateDirection.Right))
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

        private bool CheckCollision() => CheckCollision(_currentBlock);

        private bool CanFall(Block block)
        {
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

        private bool CanFall() => CanFall(_currentBlock);

        private void RandomBlock()
        {
            _currentBlock = _nextBlocks.Dequeue();
            _nextBlocks.Enqueue(new Block(_randomizer.Next()));
        }
    }
}
