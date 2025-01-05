using System.Diagnostics;

namespace Tetris
{
    internal partial class GameControl
    {
        private const int _AutoShiftDelay = 13; // 13 / 62.5 = 208 ms
        private const int _AutoShiftPeriod = 5; // 5 / 62.5 = 80 ms

        // === Key System ===
        private readonly HashSet<Keys> _pressedKeys = [];

        // === Key for hard dropping ===
        private readonly HashSet<Keys> _canHardDropKeys = [Keys.W, Keys.Up];

        // === Key for dropping ===
        private readonly HashSet<Keys> _canSoftDropKeys = [Keys.S, Keys.Down];

        // === Key for horizontal moving ===
        private readonly HashSet<Keys> _canMoveLeftKeys = [Keys.A, Keys.Left];
        private readonly HashSet<Keys> _canMoveRightKeys = [Keys.D, Keys.Right];
        //private readonly Timer _safeOfMoveFirts = new();
        //private readonly Timer _multiMoveTimer = new();

        // === Key for rotating ===
        private readonly HashSet<Keys> _canRotateLeftKeys = [Keys.J, Keys.X];
        private readonly HashSet<Keys> _canRotateRightKeys = [Keys.K, Keys.C];
        private readonly HashSet<Keys> _canRotate180Keys = [Keys.L, Keys.V];

        // === Key for hold ===
        private readonly HashSet<Keys> _canHoldKeys = [Keys.Space];

        // Allow the arrow keys to be used in the game (some key default behavior is disabled)
        protected override bool ProcessDialogKey(Keys key)
        {
            HashSet<Keys> allowKeys = [Keys.Up, Keys.Down, Keys.Left, Keys.Right];
            if (allowKeys.Contains(key))
            {
                return false;
            }
            return base.ProcessDialogKey(key);
        }

        // ...
        private uint autoShiftCounter = 0;

        private void InitializeKeySystem()
        {
            FrameTickLoadList?.Add((100, (sender, e) => PressHorizontalMove()));
            this.KeyDown += (sender, e) => KeyDownHandler(e);
            this.KeyUp += (sender, e) => KeyUpHandler(e);
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
                if (e.KeyCode == Keys.P || e.KeyCode == Keys.Escape)
                {
                    PauseButtonClick();
                }
                else if (e.KeyCode == Keys.R)
                {
                    RestartButtonClick();
                }
                else if (e.KeyCode == Keys.T)
                {
                    //ThemeButtonClick();
                    _bitmapManager.NextTheme();
                }
                else if (e.KeyCode == Keys.B)
                {
                    BgmButtonClick();
                }
                else if (GameState != E_GameState.Playing)
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
                else if (_canRotate180Keys.Contains(e.KeyCode))
                {
                    changed = TryTwiceRotate();
                }
                // Hard drop
                else if (_canHardDropKeys.Contains(e.KeyCode))
                {
                    changed = HardDropPiece();
                }
                // Soft drop
                else if (_canSoftDropKeys.Contains(e.KeyCode))
                {
                }
                // Hold
                else if (_canHoldKeys.Contains(e.KeyCode))
                {
                    changed = TryHoldBlock();
                }
                else if (e.KeyCode == Keys.O)
                {
                    // Test ...
                }
                // ...

                // Reset drop progress
                if (changed)
                {
                    //ResetAutoEatProceed();
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
            }
        }

        private void PressHorizontalMove()
        {
            if (GameState != E_GameState.Playing)
            {
                autoShiftCounter = 0;
                return;
            }

            var move = _canMoveLeftKeys.Any(key => _pressedKeys.Contains(key)) ? -1 : 0;
            move += _canMoveRightKeys.Any(key => _pressedKeys.Contains(key)) ? 1 : 0;

            if (move == 0) { autoShiftCounter = 0; }
            else if (++autoShiftCounter == 1) { TryMoveBlockLeftOrRight(move); }
            else if (autoShiftCounter == _AutoShiftDelay + _AutoShiftPeriod)
            {
                TryMoveBlockLeftOrRight(move);
                autoShiftCounter = _AutoShiftDelay;
            }

        }
    }
}
