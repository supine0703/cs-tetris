using System.Diagnostics;
using Timer = System.Threading.Timer;

namespace Tetris
{
    internal class FrameTimer : IDisposable
    {
        private const int _DefaultInterval = 16; // 62.5 FPS (frame per second)

        private List<(ulong frame, Action action)> _delayedActions = []; // Store the delayed actions
        private readonly List<long> _frameTimeStamps = []; // Store the timestamp of each frame
        private readonly Timer _timer;
        
        private readonly SynchronizationContext _syncContext;
        private bool _isTickInvoking = false;
        private readonly object _lock = new(); // Lock object for thread safety

        public FrameTimer()
        {
            _syncContext = SynchronizationContext.Current
                ?? throw new InvalidOperationException("SynchronizationContext is not set.");
            _timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }
        public FrameTimer(int interval) : this()
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(interval, 1, nameof(interval));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(interval, 1000, nameof(interval));
            Interval = interval;
        }

        public event EventHandler? Tick;
        public int Interval { get; private set; } = _DefaultInterval;
        public double Fps { get; private set; } = 0;
        

        public void Start() => _timer.Change(0, Interval);
        public void Stop() => _timer.Change(Timeout.Infinite, Timeout.Infinite);
        public void Dispose() => _timer.Dispose();
        public void DelayToDo(ulong frame, Action action)
        {
            lock (_lock) { _delayedActions.Add((frame, action)); }
        }

        private void TimerCallback(object? state)
        {
            if (_isTickInvoking) { return; }
            _isTickInvoking = true;

            // Update the FPS value
            UpdateFps();
            _syncContext.Post(_ =>
            {
                // Invoke the Tick event
                Tick?.Invoke(this, EventArgs.Empty);

                // Execute the delayed actions
                lock (_lock)
                {
                    _delayedActions = _delayedActions.Select(to => (frame: to.frame - 1, to.action)).Where(toDo =>
                    {
                        if (toDo.frame <= 1)
                        {
                            _syncContext.Post(_ => toDo.action.Invoke(), null);
                            return false;
                        }
                        return true;
                    }).ToList();
                }
                _isTickInvoking = false;
            }, null);
        }

        private void UpdateFps()
        {
            // Record the timestamp of the current frame
            var currentTime = Stopwatch.GetTimestamp();
            _frameTimeStamps.Add(currentTime);

            // Delete old timestamps longer than 2 seconds
            var ago = currentTime - Stopwatch.Frequency * 2;
            _frameTimeStamps.RemoveAll(time => time < ago);

            // Calculates and outputs the current average FPS
            if (_frameTimeStamps.Count > 1)
            {
                var duration = (_frameTimeStamps.Last() - _frameTimeStamps.First()) / (double)Stopwatch.Frequency;
                Fps = _frameTimeStamps.Count / duration;
            }
            else { Fps = 0; }
        }
    }
}
