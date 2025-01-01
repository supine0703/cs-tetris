namespace Tetris
{
    internal partial class Panel
    {
        private readonly BagRandomizer<Block.E_Type> _randomizer = new();
        private const int _EntryDelayFrames = 10;
        private bool GameOver { get; set; } = false;

        private int _lineClearCount = 0;
        private int _totalScore = 0;

        private void InitializeRandomizer()
        {
            // Initialize the randomizer
            var arr = Enum.GetValues(typeof(Block.E_Type));
            _randomizer.Reset([.. arr.Cast<Block.E_Type>().Where(e => e != Block.E_Type.Count)]);
        }

        private Block GetRandomBlock() => new(_randomizer.Next());

        private void RandomBlock()
        {
            _currentBlock = _nextBlocks.Dequeue();
            _nextBlocks.Enqueue(GetRandomBlock());
        }

        private void OnTopOut()
        {
            _sounds["GameOver"]?.Play();
            GameOver = true;
            _currentBlock = null;
            PauseButtonClick();
        }

        private bool CheckTopOut()
        {
            // TODO: more...
            return _playfieldInfo[_MarginRowNum - 1].Any(cell => cell.State != E_CellState.Empty);
        }

        private bool EatBlock()
        {
            if (_currentBlock is null)
            {
                return false;
            }

            if (CanDrop() > 0)
            {
                return false;
            }

            _sounds["Lock"]?.Play();

            foreach (var placeholder in _currentBlock.Placeholders)
            {
                var y = (int)Math.Ceiling(placeholder.Y + _currentBlock.Pos.Y + _MarginRowNum);
                var x = (int)Math.Ceiling(placeholder.X + _currentBlock.Pos.X);
                _playfieldInfo[y][x].State = E_CellState.Occupied;
                _playfieldInfo[y][x].BelongsTo = _currentBlock.Type;
            }

            // Clear full lines
            if (!ClearFullLines())
            {
                RandomBlock();

                // Check if the game is over
                if (CheckTopOut())
                {
                    OnTopOut();
                }
            }

            return true;
        }

        private bool HardDropBlock()
        {
            if (_currentBlock is null)
            {
                return false;
            }
            
            // Get the top placeholders of the current block
            List<Position> placeholders = [.. _currentBlock.Placeholders
                .ToList().GroupBy(pos => pos.X).Select(g => g.OrderBy(pos => pos.Y).First())
            ];

            // Get start position
            var startPos = _currentBlock.Pos;

            // Drop the block to the bottom
            DropBottom();

            // Calculate the delta y
            var deltaY = _currentBlock.Pos.Y - startPos.Y;

            // Get rectangles for the hard drop animation and run the animation
            var startX = (ClientSize.Width - _Columns * _blockSize) / 2;
            var startY = (ClientSize.Height - _Rows * _blockSize) / 2;
            StartHdAnimation([.. placeholders.Select(placeholder => new Rectangle(
                startX + (int)((placeholder.X + startPos.X) * _blockSize),
                startY + (int)(( placeholder.Y + startPos.Y) * _blockSize),
                _blockSize, (int)(_blockSize * deltaY) - 2 // subtract 2 (cell border)
            ))], _currentBlock.Type);

            if (!EatBlock())
            {
                throw new Exception("Unknow why the block cannot be eaten in HardDropBlock");
            }
            return true;
        }

        private bool ClearFullLines()
        {
            if (_currentBlock is null)
            {
                return false;
            }

            // Filter out all filled rows
            List<int> fullLines = [.. _currentBlock.Placeholders
                .Select(placeholder => (int)Math.Ceiling(placeholder.Y + _currentBlock.Pos.Y) + _MarginRowNum)
                .ToHashSet()
                .Order()
                .Where(line => Enumerable.Range(0, _Columns).All(
                    i => _playfieldInfo[line][i].State == E_CellState.Occupied
                ))
            ];

            if (fullLines.Count == 0)
            {
                return false;
            }

            // Define some variables to create the rectangle for the animation
            var width = _Columns * _blockSize;
            var startX = (ClientSize.Width - width) / 2;
            var startY = (ClientSize.Height - (_Rows + (_PrimaryBlockSize - 1) * 2) * _blockSize) / 2;

            List<Rectangle> animationRects = [.. fullLines.Select(line =>
            {
                foreach (var i in Enumerable.Range(0, _playfieldInfo[line].Length))
                {
                    _playfieldInfo[line][i].State = E_CellState.Clearing;
                }
                return new Rectangle(startX, startY + line * _blockSize, width, _blockSize);
            })];

            _sounds["ClearLine"]?.Play();

            _currentBlock = null;
            StartCflAnimation(animationRects, () => DelayToDo(_EntryDelayFrames, () => 
            {
                RandomBlock();
                fullLines.ForEach(line =>
                {
                    _playfieldInfo.RemoveAt(line);
                    _playfieldInfo.Insert(0, new CellInfo[_Columns]);
                });
                // Check if the game is over
                if (CheckTopOut())
                {
                    OnTopOut();
                }
            }));
            return true;
        }

        private bool TryHoldBlock()
        {
            if (_currentBlock is null)
            {
                return false;
            }

            _sounds["Hold"]?.Play();
            (_holdBlock, _currentBlock) = (new(_currentBlock.Type), _holdBlock);
            if (_currentBlock is null)
            {
                RandomBlock();
            }

            return true;
        }

        private bool TryMoveBlockLeftOrRight(int distance)
        {
            if (_currentBlock is null)
            {
                return false;
            }

            _currentBlock.MoveLeftOrRight(distance);
            if (CheckCollision())
            {
                // undo the move if there is a collision
                _currentBlock.MoveLeftOrRight(-distance);
                return false;
            }
            _sounds["Move"]?.Play();
            return true;
        }

        private bool TryMoveBlockUpOrDown(float distance)
        {
            if (_currentBlock is null)
            {
                return false;
            }

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
            if (_currentBlock is null)
            {
                return false;
            }

            _currentBlock.LeftRotate();
            if (CheckCollision() && !TestWallKick(Block.E_Rotation.Left))
            {
                // undo the rotation if there is a collision
                _currentBlock.RightRotate();
                return false;
            }
            _sounds["Rotate"]?.Play();
            return true;
        }

        private bool TryRightRotate()
        {
            if (_currentBlock is null)
            {
                return false;
            }

            _currentBlock.RightRotate();
            if (CheckCollision() && !TestWallKick(Block.E_Rotation.Right))
            {
                // undo the rotation if there is a collision
                _currentBlock.LeftRotate();
                return false;
            }
            _sounds["Rotate"]?.Play();
            return true;
        }

        private bool TryTwiceRotate()
        {
            if (_currentBlock is null)
            {
                return false;
            }
            _currentBlock.TwiceRptate();
            if (CheckCollision() && !TestWallKick(Block.E_Rotation.Left) && !TestWallKick(Block.E_Rotation.Right))
            {
                // undo the rotation if there is a collision
                _currentBlock.TwiceRptate();
                return false;
            }
            _sounds["Rotate"]?.Play();
            return true;
        }

        private bool TestWallKick(Block.E_Rotation direction)
        {
            if (_currentBlock is null)
            {
                return false;
            }

            return _currentBlock.GetPrevWallKickTable(direction).Any(diffPos =>
            {
                // Coordinates of the wall kick table are different from the block's position,
                // which is relative to the left-bottom corner, but panel's grid is relative to the left-top corner.
                // So the X coordinate needs to be Added, and the Y coordinate needs to be subtracted from the block's position.
                Position expectedPos = new(_currentBlock.Pos.X + diffPos.X, _currentBlock.Pos.Y - diffPos.Y);
                Block block = new(_currentBlock.Type, _currentBlock.Orientation, expectedPos);

                if (!CheckCollision(block))
                {
                    _sounds["Fall"]?.Play();
                    _currentBlock = block;
                    return true;
                }
                return false;
            });
        }

        private bool CheckCollision() => CheckCollision(_currentBlock);
        private bool CheckCollision(Block? block)
        {
            if (block is null)
            {
                return false;
            }

            return block.Placeholders.Any(placeholder =>
            {
                var posX = (int)Math.Ceiling(placeholder.X + block.Pos.X);
                var posY = (int)Math.Ceiling(placeholder.Y + block.Pos.Y) + _MarginRowNum;


                return (
                    // Check if the block is out of bounds
                    (posX < 0 || posX >= _Columns || posY < 0 || posY >= _Rows + _MarginRowNum) ||
                    // Check if the block collides with another block
                    _playfieldInfo[posY][posX].State != E_CellState.Empty
                );
            });
        }

        private float CanDrop() => CanDrop(_currentBlock);
        private float CanDrop(Block? block)
        {
            if (block is null)
            {
                return 0;
            }

            var distance = (float)(Math.Ceiling(block.Pos.Y) - block.Pos.Y);

            return block.Placeholders.Any(placeholder =>
            {
                var y = (int)Math.Ceiling(placeholder.Y + block.Pos.Y);
                var x = (int)Math.Ceiling(placeholder.X + block.Pos.X);

                // Check if the pos is integer
                if (distance == 0)
                {
                    y++;
                }

                // Check if the bottom of the block has space to drop
                return (y >= _Rows || _playfieldInfo[y + _MarginRowNum][x].State != E_CellState.Empty);
            }) ? 0 : (distance == 0 ? 1 : distance);
        }

        private void DropBottom() { DropBottom(_currentBlock); }
        private void DropBottom(Block? block)
        {
            if (block is null)
            {
                return;
            }

            float distance;
            while ((distance = CanDrop(block)) != 0)
            {
                block.MoveUpOrDown(distance);
            }

            if (block.Pos.Y % 1 != 0)
            {
                throw new Exception($"{block.Pos.Y} is not an integer, distance: {distance}");
            }
        }
    }
}
