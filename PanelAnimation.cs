using System.Diagnostics;
using System.Drawing;
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

            EflAnimationNextFrame();
        }

        private void DrawAllAnimationFrame(Graphics g)
        {
            DrawEflAnimationFrame(g);
        }

        // === Eliminate Full Lines Animation ===
        private const int _eflTotalAnimationFrames = 120;
        private int _eflAnimationCurrentFrame = _eflTotalAnimationFrames;
        private List<Rectangle> _eflRects = [];
        private List<int> _eflRectHeights = [];
        private Action? _onEflAnimationEnd = null;

        private void StartEflAnimation(List<Rectangle> rectangles, Action? onAnimationEnds = null, int frame = 0)
        {
            _eflAnimationCurrentFrame = frame;
            _onEflAnimationEnd = onAnimationEnds;

            _eflRects = new(rectangles);
            _eflRectHeights = _eflRects.Select(r => r.Height).ToList();
        }

        private void EflAnimationNextFrame()
        {
            if (_eflAnimationCurrentFrame < _eflTotalAnimationFrames)
            {
                _eflAnimationCurrentFrame++;
                if (_eflAnimationCurrentFrame == _eflTotalAnimationFrames)
                {
                    _onEflAnimationEnd?.Invoke();
                }
            }
        }

        private void DrawEflAnimationFrame(Graphics g)
        {
            if (_eflAnimationCurrentFrame < 0 || _eflAnimationCurrentFrame >= _eflTotalAnimationFrames)
            {
                return;
            }

            // The max limit of the gradient radius
            const float RadiusLimit = 0.659f;

            var gradientRadius = (float)_eflAnimationCurrentFrame / _eflTotalAnimationFrames;
            if (gradientRadius > RadiusLimit)
            {
                gradientRadius = RadiusLimit;
            }

            for (int i = 0; i < _eflRects.Count; i++)
            {
                var rect = _eflRects[i];

                Debug.WriteLine($"{rect}");

                // The width and height of the gradient ellipse
                var widthRadius = gradientRadius * gradientRadius / RadiusLimit * 1.273f;
                var heightRadius = widthRadius * rect.Height / rect.Width * 0.512f;

                using (GraphicsPath path = new())
                {
                    path.AddRectangle(rect);
                    using PathGradientBrush brush = new(path)
                    {
                        CenterPoint = new(rect.X + rect.Width / 2, rect.Y + rect.Height / 2),
                        CenterColor = Color.FromArgb(242, 252, 249, 176),
                        SurroundColors = [Color.FromArgb(0, 255, 223, 186)],
                        FocusScales = new(widthRadius, heightRadius),
                    };
                    g.FillRectangle(brush, rect);
                }

                // Fold lengthwise to disappear
                if (gradientRadius == RadiusLimit)
                {
                    // Keep the center position of the rectangle
                    var progress = _eflAnimationCurrentFrame - (int)(_eflTotalAnimationFrames * gradientRadius);
                    var shrinkStep = _eflRectHeights[i] / (int)(_eflTotalAnimationFrames * (1 - gradientRadius));
                    var newHeight = Math.Max(1, _eflRectHeights[i] - (progress * shrinkStep));

                    // Update the rectangle
                    rect.Y = rect.Top + (rect.Height / 2) - newHeight / 2;
                    rect.Height = newHeight;
                    _eflRects[i] = rect;
                }
            }

            //foreach (var rect in _eflRects)
            //{
            //    // The width and height of the gradient ellipse
            //    var widthRadius = gradientRadius * gradientRadius / RadiusLimit * 1.273f;
            //    var heightRadius = widthRadius * _eflRect.Height / _eflRect.Width * 0.512f;

            //    using (GraphicsPath path = new())
            //    {
            //        path.AddRectangle(_eflRect);
            //        using PathGradientBrush brush = new(path)
            //        {
            //            CenterPoint = new(_eflRect.X + _eflRect.Width / 2, _eflRect.Y + _eflRect.Height / 2),
            //            CenterColor = Color.FromArgb(242, 252, 249, 176),
            //            SurroundColors = [Color.FromArgb(0, 255, 223, 186)],
            //            FocusScales = new(widthRadius, heightRadius),
            //        };
            //        g.FillRectangle(brush, _eflRect);
            //    }

            //    // Fold lengthwise to disappear
            //    if (gradientRadius == RadiusLimit)
            //    {
            //        // Keep the center position of the rectangle
            //        var progress = _eflAnimationCurrentFrame - (int)(_eflTotalAnimationFrames * gradientRadius);
            //        var shrinkStep = _eflRectHeight / (int)(_eflTotalAnimationFrames * (1 - gradientRadius));
            //        var newHeight = Math.Max(1, _eflRectHeight - (progress * shrinkStep));

            //        // Update the rectangle
            //        _eflRect.Y = _eflRect.Top + (_eflRect.Height / 2) - newHeight / 2;
            //        _eflRect.Height = newHeight;
            //    }
            //}
        }
    }
}
