using Timer = System.Windows.Forms.Timer;

namespace Tetris
{
    internal partial class Panel
    {
        private readonly Timer _timer = new();
        private const int TimerInterval = 16;   // 62.5 FPS

        private float _dropGravity = 0.05f;               // cell per frame
        private float _softDropGravityIncrease = 0.59f;   // cell per frame (0.64 * 62.5 = 40 cell per s)

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
            if (!Pause)
            {
                // Auto be eaten
                if (_autoEatEnabled)
                {
                    if (CanDrop() == 0)
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

                // Auto dropping
                if (_dropGravity > 0)
                {
                    // Check if press the key for dropping
                    var pressed = false;
                    foreach (var key in _canSoftDropKeys)
                    {
                        if (_pressedKeys.Contains(key))
                        {
                            pressed = true;
                            break;
                        }
                    }

                    if (!TryMoveBlockUpOrDown(_dropGravity + (pressed ? _softDropGravityIncrease : 0)))
                    {
                        DropBottom();
                    }
                }
            }
            AllAnimationNextFrame();
            this.Invalidate(); // Redraw the panel
        }

        private void ResetAutoEatProceed() { _autoEatProgress = 0; }

        private float AutoEatProgressPercent() => (float)_autoEatProgress / _autoEatTotalProgress;
    }
}
