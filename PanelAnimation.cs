using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

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
        private const int _cflTotalAnimationFrames = 16;
        private int _cflAnimationCurrentFrame = _cflTotalAnimationFrames;
        private List<Rectangle> _cflRects = [];
        private List<Size> _cflSizes = [];
        private List<int> _cflRectHeights = [];
        private Action? _onCflAnimationEnd = null;

        private void StartCflAnimation(List<Rectangle> rectangles, Action? onAnimationEnds = null, int frame = 0)
        {
            _onCflAnimationEnd?.Invoke();
            _cflAnimationCurrentFrame = frame;
            _onCflAnimationEnd = onAnimationEnds;

            _cflRects = new(rectangles);
            _cflSizes = _cflRects.Select(r => r.Size).ToList();
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
                var rect = _cflRects[i];
                using (GraphicsPath path = new())
                {
                    var tempRect = rect;
                    var widthRatio = (float)rect.Width / _cflSizes[i].Width;
                    var heightRatio = (float)rect.Height / _cflSizes[i].Height;
                    tempRect.Width += (int)(_blockSize * 2 * widthRatio);
                    tempRect.Height += (int)(_blockSize * 2 * heightRatio);
                    tempRect.X -= (int)(_blockSize * widthRatio);
                    tempRect.Y -= (int)(_blockSize * heightRatio);

                    path.AddRectangle(tempRect);
                    using PathGradientBrush brush = new(path)
                    {
                        CenterPoint = new(rect.X + rect.Width / 2, rect.Y + rect.Height / 2),
                        CenterColor = Color.FromArgb(255, 252, 249, 176),
                        SurroundColors = [Color.FromArgb(0, 255, 223, 186)],
                        FocusScales = new((float)rect.Width / tempRect.Width, (float)rect.Height / tempRect.Height),
                    };
                    g.FillRectangle(brush, tempRect);
                }

                // The progress of the animation is first fast and then slow
                var progress = Math.Sqrt(_cflAnimationCurrentFrame) * Math.Sqrt(_cflTotalAnimationFrames - 1);

                // It compresses vertically and grows horizontall
                var heightStep = (float)_cflSizes[i].Height / (_cflTotalAnimationFrames - 1);
                var newHeight = Math.Max(1, _cflSizes[i].Height - (int)(progress * heightStep));

                var widthStep = (float)_cflSizes[i].Width * 0.391 / (_cflTotalAnimationFrames - 1);
                var newWidth = _cflSizes[i].Width + (int)(progress * widthStep);

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
