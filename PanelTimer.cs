using Timer = System.Windows.Forms.Timer;

namespace Tetris
{
    internal partial class Panel
    {
        private readonly Timer _timer = new();
        private int _progress = 0; // in ms
        private const bool _timerEnabled = true;
        private const int TimerInterval = 16;   // 62.5 FPS
        private const int TotalProgress = 600;  // 0.6s

        private void InitializeTimer()
        {
            _timer.Interval = TimerInterval;
            _timer.Tick += TimerTick;
        }

        private void TimerStart()
        {
            if (_timerEnabled)
            {
                _timer.Start();
            }
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            _progress += TimerInterval;
            if (_progress >= TotalProgress)
            {
                _progress = 0;
                _timer.Stop();
            }
            this.Invalidate();
        }
    }
}
