using System.Diagnostics;
using Timer = System.Threading.Timer;

namespace Tetris
{
    internal partial class TetrisPanel
    {
        // The timer is used to refresh each frame
        private Timer? _timer = null; // In practice, it appears to be rounded up
        private const int _TimerInterval = 16;  // 62.5 FPS (frame per second)
        private List<long> _frameTimes = [];    // Store the timestamp of each frame
        private double Fps => _frameTimer.Fps;

        private readonly FrameTimer _frameTimer = new();

        // The gravity for dropping
        private float _dropGravity = 0.01632f;              // 0.017G (cell per frame) (here 'G' refer to 60 frames per second)
        private float _softDropGravityIncrease = 0.67488f;  // 0.72G  (0.6912 * 62.5 = 40 cell per s)
        private const float _MaxGravity = 1.92f;            // 2G     (0.96 * 62.5 = 60 cell per s)
        private bool AutoDropEnabled { get; set; } = true;

        // The auto eat block progress
        private int _autoEatProgress = 0;           // in ms
        private int _autoEatTotalProgress = 500;    // 0.5s
        private bool AutoEatEnabled { get; set; } = true;

        bool _isProcessing = false;

        private void InitializeTimer()
        {
            //_timer.Elapsed += (sender, e) =>
            //{
            //        this.Invoke(() => TimerTick());
            //};
            //_timer.AutoReset = true;
            //_timer.Start();
            _frameTimer.Tick += (sender, e) => TimerTick();
            _frameTimer.Start();
        }

        private void DisposeTimer()
        {
            //_timer.Stop();
            //_timer.Dispose();
        }

        private void TimerTick()
        {
            RefushFpsLabel();
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
            this.Update();
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
