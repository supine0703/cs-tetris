namespace Tetris
{
    internal partial class GameControl
    {
        private bool _jmp = true;

        private void FrameRefresh()
        {
            if (_jmp = !_jmp) { return; }
            this.Invalidate();
        }

        private void FrameRefresh(Rectangle rect)
        {
            if (_jmp = !_jmp) { return; }
            this.Invalidate(rect);
        }
    }
}
