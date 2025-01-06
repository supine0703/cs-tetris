//#define DYNAMIC_NATURE_GRAVITY

using System.Diagnostics;

namespace Tetris
{
    // Define the game rules, game state, and game control
    internal partial class GameControl
    {
        // Define some game constants
        private const int _MatrixColumns = 10;
        private const int _MatrixRows = 20;
        private const int _BufferZoneRows = 20;
        private const int _PreviewPieceCount = 6;
        private const int _HoldPieceCount = 1;
        private const double _MaxGravityLimit = 20; // 20 cell per second no matter how many fps
        private const double _MinSoftDropGravity = 0.5;
        private const int _EntryDelayFrames = 10;
        private const int _LockDelayFrames = 36; // = 32 * 16 = 512ms
        private const int _LineClearDelayFrames = _CflTotalAnimationFrames;

        // Define the game playfield cell state
        private enum E_CellState { Empty = 0, Clearing, Occupied, }
        private static int E_CellStateCount => Enum.GetValues(typeof(E_CellState)).Length;
        private struct CellInfo(E_CellState state = default, Piece.E_Type belongsTo = default)
        {
            public E_CellState State { get; set; } = state;
            public Piece.E_Type BelongsTo { get; set; } = belongsTo;
        }

        // Define the game state
        private enum E_GameState { Waiting = 0, Ready, Playing, Paused, GameOver, }
        private static int E_GameStateCount => Enum.GetValues(typeof(E_GameState)).Length;

        // Define some variable or attribute for the game control
        private delegate void GameStateChangedEventHandler(E_GameState state);
        private event GameStateChangedEventHandler? GameStateChanged;
        private E_GameState GameState
        {
            get => _gameState;
            set
            {
                if (value == _gameState) { return; }
                _gameState = value;
                GameStateChanged?.Invoke(_gameState);
            }
        }
        private E_GameState _gameState;
        private readonly BagRandomizer<Piece.E_Type> _randomizer = new(Enum.GetValues(typeof(Piece.E_Type)));
        private readonly List<CellInfo[]> _playfieldInfo = new(_MatrixRows + _BufferZoneRows);
        private readonly Queue<Piece> _previewQueue = new(_PreviewPieceCount);
        private readonly Queue<Piece> _holdQueue = new(_HoldPieceCount + 1);
        private Piece? _activePiece;

        // Define about the Gravity
        private ulong _clearLineCount = 0;
        private ulong Level => _clearLineCount / 10 + 1;
        private double NatureGravity => Math.Min(_MaxGravityLimit,
#if DYNAMIC_NATURE_GRAVITY
            (double)(1.0 / (Math.Pow((0.8 - (Level - 1) * 0.007), Level - 1) * _frameTimer.Fps)) // dynamic
#else
            (double)(_frameTimer.Interval / 1000.0 / Math.Pow(0.8 - (Level - 1) * 0.007, Level - 1)) // static
#endif
        );
        private double SoftDropGravity => Math.Min(
            _MaxGravityLimit, Math.Max(_MinSoftDropGravity, NatureGravity * 20)
        );

        // Define about the Lock
        private int _waitedLockFrames = 0;
        private float _lockDelayProgress => (float)_waitedLockFrames / _LockDelayFrames;
        private void ResetWaitedLockFrames()
        {
            if (_waitedLockFrames != 0)
            {
                _waitedLockFrames = 0;
                FrameInvalidate(WaitLockProgressRect);
            }
        }

        // ...
        private void GameRuleLoad()
        {
            FrameTickLoadList?.Add((100, (sender, e) => TryDropPerFrame()));
            RestartGame();
        }

        private void RestartGame()
        {
            // Reset the randomizer
            _randomizer.RefillBag();

            // Reset the playfield info
            _playfieldInfo.Clear();
            foreach (var _ in Enumerable.Range(0, _playfieldInfo.Capacity))
            {
                _playfieldInfo.Add(new CellInfo[_MatrixColumns]);
            }

            // Reset the pieces
            _previewQueue.Clear();
            foreach (var _ in Enumerable.Range(0, _PreviewPieceCount))
            {
                _previewQueue.Enqueue(GetRandomPiece());
            }
            NextPiece();

            // Reset the hold block
            _holdQueue.Clear();
            GameState = E_GameState.Playing; // TODO ...

            this.Invalidate();

            //// Update pause message and animation
            //_stopwatch.Restart();
            //PauseButtonClick();
            //PauseButtonClick();
            //ResetAllAnimation();

            //// Reset the game info
            //LineClearCount = 0;
            //TotalScore = 0;
            //RefushTotalScore();
        }

        private bool CheckTopOut()
        {
            // TODO: more...
            return _playfieldInfo[_BufferZoneRows - 1].Any(cell => cell.State != E_CellState.Empty);
        }

        private void OnTopOut()
        {
            GameState = E_GameState.GameOver;
            _sounds["GameOver"]?.RePlay();
            _activePiece = null;
        }

        // TODO ...

        private Piece GetRandomPiece() => new(_randomizer.Next());

        private void NextPiece()
        {
            _activePiece = _previewQueue.Dequeue();
            _previewQueue.Enqueue(GetRandomPiece());
            FrameInvalidate(PreviewQueueBoxRect);
        }

        private bool TryDropPerFrame()
        {
            if (_activePiece == null) { return false; }
            if (GameState != E_GameState.Playing) { return false; }

            if (CanDrop() == 0)
            {
                if (++_waitedLockFrames >= _LockDelayFrames)
                {
                    if (TryPieceLock()) { }
                }
                FrameInvalidate(WaitLockProgressRect);
                return false;
            }
            ResetWaitedLockFrames();
            var pressed = SoftDropPressed;
            if (pressed) { _sounds["Move"]?.RePlay(); }
            var down = (float)(pressed ? SoftDropGravity : NatureGravity);
            if (!TryPieceMoveUpOrDown(down)) { DropBottom(); }
            if (CanDrop() == 0) { _sounds["Fall"]?.RePlay(); }
            return true;
        }

        private bool TryLineClear()
        {
            if (_activePiece is null) { return false; }
            if (GameState != E_GameState.Playing) { return false; }

            // Filter out all filled rows
            var fullLineRows = _activePiece.Placeholders
                .Select(pos => (int)Math.Ceiling(pos.Y + _activePiece.Pos.Y) + _BufferZoneRows)
                .ToHashSet()
                .Order()
                .Where(row => Enumerable.Range(0, _MatrixColumns).All(
                    col => _playfieldInfo[row][col].State == E_CellState.Occupied
                ))
                .ToList();

            if (fullLineRows.Count == 0) { return false; }

            _activePiece = null;
            _sounds["ClearLine"]?.RePlay();

            //CaculateScore(fullLineRows.Count);
            _clearLineCount += (ulong)fullLineRows.Count;
            StartCflAnimation(fullLineRows.Select(row =>
            {
                foreach (var i in Enumerable.Range(0, _playfieldInfo[row].Length))
                {
                    _playfieldInfo[row][i].State = E_CellState.Clearing;
                }
                return new Point(PlayfieldLocation.X, YiToPosY(row - _BufferZoneRows));
            }).ToArray());

            _frameTimer.DelayToDo(_LineClearDelayFrames, () => _frameTimer.DelayToDo(_EntryDelayFrames, () =>
            {
                NextPiece();
                fullLineRows.ForEach(line =>
                {
                    _playfieldInfo.RemoveAt(line);
                    _playfieldInfo.Insert(0, new CellInfo[_MatrixColumns]);
                });
                FrameInvalidate(new Rectangle(PlayfieldLocation.X, 0, _MatrixColumns * CellSize, this.ClientSize.Height));
                // Check if the game is over
                if (CheckTopOut()) { OnTopOut(); }
            }));
            return true;
        }

        private bool TryPieceLock()
        {
            if (_activePiece is null) { return false; }
            if (GameState != E_GameState.Playing) { return false; }

            if (CanDrop() > 0) { return false; }

            ResetWaitedLockFrames();
            _sounds["Lock"]?.RePlay();

            foreach (var pos in _activePiece.Placeholders)
            {
                var row = (int)Math.Ceiling(pos.Y + _activePiece.Pos.Y + _BufferZoneRows);
                var col = (int)Math.Ceiling(pos.X + _activePiece.Pos.X);
                _playfieldInfo[row][col].State = E_CellState.Occupied;
                _playfieldInfo[row][col].BelongsTo = _activePiece.Type;
                FrameInvalidate(new Rectangle(ColToPosX(col), RowToPosY(row), CellSize, CellSize));
            }

            // Clear full lines
            if (!TryLineClear())
            {
                _activePiece = null;

                // Check if the game is over
                if (CheckTopOut()) { OnTopOut(); }
                else
                {
                    _frameTimer.DelayToDo(_EntryDelayFrames, () => NextPiece());
                }
            }

            return true;
        }

        private bool TryPieceHold()
        {
            if (_activePiece is null) { return false; }
            if (GameState != E_GameState.Playing) { return false; }

            ResetWaitedLockFrames();
            _sounds["Hold"]?.RePlay();
            _holdQueue.Enqueue(new(_activePiece.Type));

            if (_holdQueue.Count <= _HoldPieceCount) { NextPiece(); }
            else { _activePiece = _holdQueue.Dequeue(); }

            FrameInvalidate(HoldQueueBoxRect);
            return true;
        }

        private bool TryPieceHardDrop()
        {
            if (_activePiece is null) { return false; }
            if (GameState != E_GameState.Playing) { return false; }

            // Get the top placeholders of the current piece
            List<Position> placeholders = _activePiece.Placeholders
                .GroupBy(pos => pos.X)
                .Select(g => g.OrderBy(pos => pos.Y).First())
                .ToList();

            // Get start position
            var startPos = _activePiece.Pos;

            // Drop the piece to the bottom
            DropBottom();

            // Calculate the delta y
            var deltaY = _activePiece.Pos.Y - startPos.Y;

            // Get rectangles for the hard drop animation and run the animation
            StartHdAnimation(placeholders.Select(placeholder => new Rectangle(
                PlayfieldLocation.X + (int)((placeholder.X + startPos.X) * CellSize),
                PlayfieldLocation.Y + (int)((placeholder.Y + startPos.Y) * CellSize),
                CellSize, (int)(CellSize * deltaY) - 2 // subtract 2 (cell border)
            )).ToArray(), _activePiece.Type);

            if (!TryPieceLock())
            {
                throw new Exception("Unknow why the block cannot be eaten in HardDropBlock");
            }
            return true;
        }

        private bool TryPieceMoveLeftOrRight(int distance)
        {
            if (_activePiece is null) { return false; }
            if (GameState != E_GameState.Playing) { return false; }

            _activePiece.MoveLeftOrRight(distance);
            if (CheckCollision())
            {
                // undo the move if there is a collision
                _activePiece.MoveLeftOrRight(-distance);
                return false;
            }
            ResetWaitedLockFrames();
            _sounds["Move"]?.RePlay();
            return true;
        }

        private bool TryPieceMoveUpOrDown(float distance)
        {
            if (_activePiece is null) { return false; }
            if (GameState != E_GameState.Playing) { return false; }

            _activePiece.MoveUpOrDown(distance);
            if (CheckCollision())
            {
                // undo the move if there is a collision
                _activePiece.MoveUpOrDown(-distance);
                return false;
            }
            ResetWaitedLockFrames();
            return true;
        }

        private bool TryPieceRotateLeft()
        {
            if (_activePiece is null) { return false; }
            if (GameState != E_GameState.Playing) { return false; }

            _activePiece.RotateLeft();
            if (CheckCollision() && !TestPieceWallKick(Piece.E_Rotation.Left))
            {
                // undo the rotation if there is a collision
                _activePiece.RotateRight();
                return false;
            }
            ResetWaitedLockFrames();
            _sounds["Rotate"]?.RePlay();
            return true;
        }

        private bool TryPieceRotateRight()
        {
            if (_activePiece is null) { return false; }
            if (GameState != E_GameState.Playing) { return false; }

            _activePiece.RotateRight();
            if (CheckCollision() && !TestPieceWallKick(Piece.E_Rotation.Right))
            {
                // undo the rotation if there is a collision
                _activePiece.RotateLeft();
                return false;
            }
            ResetWaitedLockFrames();
            _sounds["Rotate"]?.RePlay();
            return true;
        }

        private bool TryPieceRotateTwice()
        {
            if (_activePiece is null) { return false; }
            if (GameState != E_GameState.Playing) { return false; }

            _activePiece.RotateTwice();
            if (CheckCollision() && !TestPieceWallKick(Piece.E_Rotation.Left) && !TestPieceWallKick(Piece.E_Rotation.Right))
            {
                // undo the rotation if there is a collision
                _activePiece.RotateTwice();
                return false;
            }
            ResetWaitedLockFrames();
            _sounds["Rotate"]?.RePlay();
            return true;
        }

        private bool TestPieceWallKick(Piece.E_Rotation direction)
        {
            if (_activePiece is null) { return false; }

            return _activePiece.GetPrevWallKickTable(direction).Any(diffPos =>
            {
                // Coordinates of the wall kick table are different from the block's position,
                // which is relative to the left-bottom corner, but panel's grid is relative to the left-top corner.
                // So the X coordinate needs to be Added, and the Y coordinate needs to be subtracted from the block's position.
                Position expectedPos = new(_activePiece.Pos.X + diffPos.X, _activePiece.Pos.Y - diffPos.Y);
                Piece piece = new(_activePiece.Type, _activePiece.Orientation, expectedPos);

                if (!CheckCollision(piece))
                {
                    _sounds["Fall"]?.RePlay();
                    _activePiece = piece;
                    return true;
                }
                return false;
            });
        }

        private void DropBottom() => DropBottom(_activePiece);
        private void DropBottom(Piece? piece)
        {
            if (piece is null) { return; }
            if (GameState != E_GameState.Playing) { return; }

            float distance;
            while ((distance = CanDrop(piece)) != 0) { piece.MoveUpOrDown(distance); }

            if (piece.Pos.Y % 1 != 0)
            {
                throw new Exception($"{piece.Pos.Y} is not an integer, distance: {distance}");
            }
        }

        private bool CheckCollision() => CheckCollision(_activePiece);
        private bool CheckCollision(Piece? piece)
        {
            if (piece is null) { return false; }

            return piece.Placeholders.Any(pos =>
            {
                var col = (int)Math.Ceiling(pos.X + piece.Pos.X);
                var row = (int)Math.Ceiling(pos.Y + piece.Pos.Y) + _BufferZoneRows;


                return (
                    // Check if the block is out of bounds
                    (col < 0 || col >= _MatrixColumns || row < 0 || row >= _MatrixRows + _BufferZoneRows) ||
                    // Check if the block collides with another block
                    _playfieldInfo[row][col].State != E_CellState.Empty
                );
            });
        }

        private float CanDrop() => CanDrop(_activePiece);
        private float CanDrop(Piece? piece)
        {
            if (piece is null) { return 0; }

            var distance = (float)(Math.Ceiling(piece.Pos.Y) - piece.Pos.Y);

            return piece.Placeholders.Any(placeholder =>
            {
                var col = (int)Math.Ceiling(placeholder.X + piece.Pos.X);
                var yi = (int)Math.Ceiling(placeholder.Y + piece.Pos.Y);

                // Check if the pos is integer
                if (distance == 0) { yi++; }

                var row = yi + _BufferZoneRows;

                // Check if the bottom of the piece has not space to drop
                return (yi >= _MatrixRows || _playfieldInfo[row][col].State != E_CellState.Empty);
            }) ? 0 : (distance == 0 ? 1 : distance);
        }
    }
}
