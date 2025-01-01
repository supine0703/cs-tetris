using System.Diagnostics;
using Timer = System.Timers.Timer;

namespace Tetris
{
    internal partial class Panel
    {
        // The timer is used to refresh each frame
        private readonly Timer _timer = new(_TimerInterval); // In practice, it appears to be rounded up
        private const int _TimerInterval = 16;  // 62.5 FPS (frame per second)
        private List<long> _frameTimes = [];    // Store the timestamp of each frame
        private double Fps { get; set; } = 0;
        
        // The gravity for dropping
        private float _dropGravity = 0.01632f;              // 0.017G (cell per frame) (here 'G' refer to 60 frames per second)
        private float _softDropGravityIncrease = 0.67488f;  // 0.72G  (0.6912 * 62.5 = 40 cell per s)
        private const float _MaxGravity = 1.92f;            // 2G     (0.96 * 62.5 = 60 cell per s)
        private bool AutoDropEnabled { get; set; } = true;

        // The auto eat block progress
        private int _autoEatProgress = 0;           // in ms
        private int _autoEatTotalProgress = 500;    // 0.5s
        private bool AutoEatEnabled { get; set; } = true;

        private void InitializeTimer()
        {
            _timer.Elapsed += (sender, e) =>
            {
                if (this.IsHandleCreated)
                {
                    this.Invoke(() => TimerTick());
                }
            };
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void DisposeTimer()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        private void TimerTick()
        {
            UpdateFps();
            RefushWatchLabel();
            DelayToDoNextFrame();
            if (!Pause)
            {
                // Auto be eaten
                if (AutoEatEnabled && _currentBlock is not null && CanDrop() == 0)
                {
                    _autoEatProgress += _TimerInterval;
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

                // Auto dropping
                if (_dropGravity >= 0 && CanDrop() != 0)
                {
                    // Check if press the key for dropping
                    var pressed = _canSoftDropKeys.Any(key => _pressedKeys.Contains(key));

                    if (pressed && _autoEatProgress == 0)
                    {
                        _sounds["Move"]?.RePlay();
                    }

                    var down = (pressed ?
                        (_dropGravity + _softDropGravityIncrease) :
                        (AutoDropEnabled ? _dropGravity : 0)
                    );
                    if (!TryMoveBlockUpOrDown(down))
                    {
                        DropBottom();
                    }

                    if (CanDrop() == 0)
                    {
                        _sounds["Fall"]?.RePlay();
                    }
                }
            }
            AllAnimationNextFrame();
            this.Invalidate(); // Redraw the panel
        }

        private void UpdateFps()
        {
            // Record the timestamp of the current frame
            long currentTime = Stopwatch.GetTimestamp();
            _frameTimes.Add(currentTime);

            // Delete old timestamps longer than 1 minute
            long oneMinuteAgo = currentTime - Stopwatch.Frequency * 60;
            _frameTimes.RemoveAll(time => time < oneMinuteAgo);

            // Calculates and outputs the current average FPS
            if (_frameTimes.Count > 1)
            {
                var duration = (_frameTimes.Last() - _frameTimes[0]) / (double)Stopwatch.Frequency;
                Fps = _frameTimes.Count / duration;
            }
            RefushFpsLabel();
        }

        private void ResetAutoEatProceed() { _autoEatProgress = 0; }

        private float AutoEatProgressPercent() => (float)_autoEatProgress / _autoEatTotalProgress;

        // === For delay to do somethings ===
        private List<(int frame, Action? action)> _delayToDo = [];

        private void DelayToDo(int frame, Action? action) { _delayToDo.Add((frame, action)); }

        private void DelayToDoNextFrame()
        {
            if (Pause)
            {
                return;
            }

            _delayToDo = [.. _delayToDo.Select(toDo => (frame: toDo.frame - 1, toDo.action)).Where(toDo =>
            {
                if (toDo.frame <= 0)
                {
                    toDo.action?.Invoke();
                    return false;
                }
                return true;
            })];
        }
    }
}
