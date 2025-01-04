using Timer = System.Windows.Forms.Timer;

namespace Tetris
{
    internal partial class TetrisPanel
    {
        // === Key System ===
        private readonly HashSet<Keys> _pressedKeys = [];

        // === Key for hard dropping ===
        private readonly HashSet<Keys> _canHardDropKeys = [Keys.W, Keys.Up];

        // === Key for dropping ===
        private readonly HashSet<Keys> _canSoftDropKeys = [Keys.S, Keys.Down];

        // === Key for horizontal moving ===
        private readonly HashSet<Keys> _canMoveLeftKeys = [Keys.A, Keys.Left];
        private readonly HashSet<Keys> _canMoveRightKeys = [Keys.D, Keys.Right];
        private readonly Timer _safeOfMoveFirts = new();
        private readonly Timer _multiMoveTimer = new();

        // === Key for rotating ===
        private readonly HashSet<Keys> _canRotateLeftKeys = [Keys.J, Keys.X];
        private readonly HashSet<Keys> _canRotateRightKeys = [Keys.K, Keys.C];
        private readonly HashSet<Keys> _canRotate180Keys = [Keys.L, Keys.V];

        // === Key for hold ===
        private readonly HashSet<Keys> _canHoldKeys = [Keys.Space];

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
            _multiMoveTimer.Tick += (sender, e) => PressHorizontalMove();
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
                    ThemeButtonClick();
                }
                else if (e.KeyCode == Keys.B)
                {
                    BgmButtonClick();
                }
                else if (Pause)
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
                // Hard drop
                else if (_canHardDropKeys.Contains(e.KeyCode))
                {
                    changed = HardDropBlock();
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
                    ResetAutoEatProceed();
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
                _safeOfMoveFirts.Stop();
                _multiMoveTimer.Stop();
            }
        }

        private void PressHorizontalMove()
        {
            if (Pause)
            {
                _safeOfMoveFirts.Stop();
                _multiMoveTimer.Stop();
                _pressedKeys.Clear();
            }

            var move = (
                (_canMoveLeftKeys.Any(key => _pressedKeys.Contains(key)) ? -1 : 0) +
                (_canMoveRightKeys.Any(key => _pressedKeys.Contains(key)) ? 1 : 0)
            );

            // Reset drop progress
            if (move != 0 && TryMoveBlockLeftOrRight(move))
            {
                ResetAutoEatProceed();
            }
        }
    }
}
