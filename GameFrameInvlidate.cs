using System.Diagnostics;

namespace Tetris
{
    internal partial class GameControl
    {
        private uint SkipOneFrameInFrameCount { get; set; } = 0; // v=0: no jump, v>=1: jump 1/v
        private uint _waitSkipOneFrameCounter = 0;
        private List<Rectangle>? _waitingInvalidateRects = null;

        private void InitializeFrameInvalidate()
        {
            FrameTickLoadList?.Add((int.MinValue, (sender, e) => InvalidateWattingRects()));
        }

        private void FrameInvalidate() { _waitingInvalidateRects = null; }

        private void FrameInvalidate(Rectangle rect) { _waitingInvalidateRects?.Add(rect); }

        private void InvalidateWattingRects()
        {
            if (++_waitSkipOneFrameCounter != SkipOneFrameInFrameCount || _frameTimer.SkipOneFrame)
            {
                if (_waitingInvalidateRects is null)
                {
                    this.Invalidate();
                }
                else if (_waitingInvalidateRects.Count > 0)
                {
                    _waitingInvalidateRects.ForEach(rect => this.Invalidate(rect));
                }
                _waitingInvalidateRects = [];
                if (_frameTimer.SkipOneFrame) { _waitSkipOneFrameCounter = 0; }
            }
            else if (_waitSkipOneFrameCounter >= SkipOneFrameInFrameCount)
            {
                if (_waitSkipOneFrameCounter == SkipOneFrameInFrameCount) { _frameTimer.FlagOneFrameIsSkipped(); }
                _waitSkipOneFrameCounter = 0;
            }
        }
    }
}
