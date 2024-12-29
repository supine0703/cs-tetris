using Timer = System.Windows.Forms.Timer;

namespace Tetris
{
    internal partial class Panel
    {
        private readonly Timer _timer = new();
        private const int TimerInterval = 16;   // 62.5 FPS

        private float _fallSpeed = 0.05f;       // grid per frame
        private float _keyFallSpeedUp = 0.6f;   // grid per frame

        private int _autoEatProgress = 0;           // in ms
        private int _autoEatTotalProgress = 600;    // 0.6s
        private bool _autoEatEnabled = true;
        
        private void InitializeTimer()
        {
            _timer.Interval = TimerInterval;
            _timer.Tick += TimerTick;
            _timer.Start();
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            // Auto be eaten
            if (_autoEatEnabled)
            {
                if (CanFall() == 0)
                {
                    _autoEatProgress += _timer.Interval;
                    if (_autoEatProgress >= _autoEatTotalProgress)
                    {
                        EatBlock();
                        ResetAutoEatProceed();
                    }
                }
                else
                {
                    ResetAutoEatProceed();
                }
            }

            // Auto falling
            if (_fallSpeed > 0)
            {
                // Check if press the key for falling
                var pressed = false;
                foreach (var key in _canFallKeys)
                {
                    if (_pressedKeys.Contains(key))
                    {
                        pressed = true;
                        break;
                    }
                }

                if (!TryMoveBlockUpOrDown(_fallSpeed + (pressed ? _keyFallSpeedUp : 0)))
                {
                    FallBottom();
                }
            }

            this.Invalidate(); // Redraw the panel
        }

        private void ResetAutoEatProceed() { _autoEatProgress = 0; }

        private float AutoEatProgressPercent() => (float)_autoEatProgress / _autoEatTotalProgress;
    }
}
