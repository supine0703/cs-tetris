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
                    var pressed = _canSoftDropKeys.Any(key => _pressedKeys.Contains(key));
                    
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
    
        // === For delay to do somethings ===
        private List<(int frame, Action? action)> _delayToDo = [];

        private void DelayToDo(int frame, Action? action) { _delayToDo.Add((frame, action)); }

        private void DelayToDoNextFrame()
        {
            if (_delayToDo.Count == 0)
            {
                return;
            }

            foreach (var i in Enumerable.Range(_delayToDo.Count - 1, 0))
            {
                var (frame, action) = _delayToDo[i];
                if (--frame <= 0)
                {
                    action?.Invoke();
                    _delayToDo.RemoveAt(i);
                }
                else
                {
                    _delayToDo[i] = (frame, action);
                }
            }
        }
    }
}
