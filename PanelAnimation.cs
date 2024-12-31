using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace Tetris
{
    internal partial class Panel
    {
        private void AllAnimationNextFrame()
        {
            if (Pause)
            {
                return;
            }

            CflAnimationNextFrame();
        }

        private void DrawAllAnimationFrame(Graphics g)
        {
            DrawCflAnimationFrame(g);
        }

        // === Clear Full Lines Animation ===
        private const int _cflTotalAnimationFrames = 160;
        private int _cflAnimationCurrentFrame = _cflTotalAnimationFrames;
        private List<Rectangle> _cflRects = [];
        private List<Rectangle> _cflBkRects = [];
        private List<int> _cflRectHeights = [];
        private Action? _onCflAnimationEnd = null;
        private Color CflCenterColor => UsingDark ? Color.FromArgb(252, 249, 176) : Color.FromArgb(171, 32, 164);

        private void StartCflAnimation(List<Rectangle> rectangles, Action? onAnimationEnds = null, int frame = 0)
        {
            _onCflAnimationEnd?.Invoke();
            _cflAnimationCurrentFrame = frame;
            _onCflAnimationEnd = onAnimationEnds;

            _cflRects = new(rectangles);
            _cflBkRects = new(rectangles);
            _cflRectHeights = _cflRects.Select(r => r.Height).ToList();
        }

        private void CflAnimationNextFrame()
        {
            if (_cflAnimationCurrentFrame < _cflTotalAnimationFrames)
            {
                _cflAnimationCurrentFrame++;
                if (_cflAnimationCurrentFrame == _cflTotalAnimationFrames)
                {
                    _onCflAnimationEnd?.Invoke();
                    _onCflAnimationEnd = null;
                }
            }
        }

        private void DrawCflAnimationFrame(Graphics g)
        {
            if (_cflAnimationCurrentFrame < 0 || _cflAnimationCurrentFrame >= _cflTotalAnimationFrames)
            {
                return;
            }

            foreach (var i in Enumerable.Range(0, _cflRects.Count))
            {
                // The progress of the animation is first fast and then slow
                var progress = (
                    Math.Sqrt(_cflAnimationCurrentFrame) *
                    Math.Sqrt(_cflTotalAnimationFrames - 1) /
                    (_cflTotalAnimationFrames - 1)
                );


                var alpha = 155 + (int)(100 * (1 - progress));
                var rect = _cflRects[i];
                var bkRect = _cflBkRects[i];

                var hDelta = (int)((float)rect.Width / bkRect.Width * _blockSize);
                var vDelta = (int)((float)rect.Height / bkRect.Height * _blockSize) * 2;

                using (GraphicsPath path = new())
                {
                    // Define tempRect that uses the halo effect
                    var tempRect = rect;
                    
                    tempRect.Width += hDelta * 2;
                    tempRect.X -= hDelta;

                    tempRect.Height += vDelta * 2;
                    tempRect.Y -= vDelta;

                    // Draw horizontal
                    path.AddEllipse(tempRect);
                    using (PathGradientBrush brush = new(path)
                    {
                        CenterPoint = new(tempRect.X + tempRect.Width / 2, tempRect.Y + tempRect.Height / 2),
                        CenterColor = Color.FromArgb(alpha, CflCenterColor),
                        SurroundColors = [Color.Transparent],
                        FocusScales = new((float)bkRect.Width / tempRect.Width / 2, 0),
                    }) { g.FillRectangle(brush, tempRect); }

                    

                    // Draw vertical
                    //Rectangle vRect = new(bkRect.X, tempRect.Y + 1, bkRect.Width, Math.Max(1, vDelta));

                    //using (LinearGradientBrush brush = new(vRect, Color.Transparent, Color.FromArgb(alpha, CflCenterColor), 90f))
                    //{
                    //    vRect.Y++;
                    //    //g.FillRectangle(brush, vRect);
                    //}

                    //// Draw top
                    //path.Reset();
                    //path.AddRectangle(vRect);
                    //using (PathGradientBrush brush = new(path)
                    //{
                    //    CenterPoint = new(vRect.X + vRect.Width / 2, vRect.Bottom),
                    //    CenterColor = Color.FromArgb(alpha, CflCenterColor),
                    //    SurroundColors = [Color.Transparent],
                    //    FocusScales = new(1, 0),
                    //}) { g.FillRectangle(brush, tempRect); }
                    
                    //// Draw bottom
                    //vRect.Y = tempRect.Bottom - vDelta;
                    //path.Reset();
                    //path.AddRectangle(vRect);
                    //using (PathGradientBrush brush = new(path)
                    //{
                    //    CenterPoint = new(vRect.X + vRect.Width / 2, vRect.Top),
                    //    CenterColor = Color.FromArgb(alpha, CflCenterColor),
                    //    SurroundColors = [Color.Transparent],
                    //    FocusScales = new(1, 0),
                    //}) { g.FillRectangle(brush, tempRect); }

                    // Draw corners
                    //Rectangle cornerRect = new(tempRect.Location, new(hDelta, vDelta));

                    //// Draw top left
                    //path.Reset();
                    //path.AddRectangle(cornerRect);
                    //using (PathGradientBrush brush = new(path)
                    //{
                    //    CenterPoint = new(cornerRect.Right, cornerRect.Bottom),
                    //    CenterColor = Color.FromArgb(alpha, CflCenterColor),
                    //    SurroundColors = [Color.Transparent],
                    //    FocusScales = new(0, 0),
                    //}) { g.FillRectangle(brush, tempRect); }
                }

                // It compresses vertically and grows horizontall
                var newHeight = Math.Max(1, bkRect.Height - (int)(progress * bkRect.Height));
                var newWidth = bkRect.Width + (int)(progress * bkRect.Width * 0.391f);

                // Keep the center position of the rectangle
                rect.Y = rect.Top + (rect.Height / 2) - newHeight / 2;
                rect.Height = newHeight;
                rect.X = rect.Left + (rect.Width / 2) - newWidth / 2;
                rect.Width = newWidth;

                // Update the rectangle
                _cflRects[i] = rect;
            }
        }
    }
}
