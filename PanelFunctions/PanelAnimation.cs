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
            HdAnimationNextFrame();
        }

        private void DrawAllAnimationFrame(Graphics g)
        {
            DrawCflAnimationFrame(g);
            DrawHdAnimationFrame(g);
        }

        // === Clear Full Lines Animation ===
        private const int _CflTotalAnimationFrames = 21; // Line Clear Delay
        private int _cflAnimationCurrentFrame = _CflTotalAnimationFrames;
        private List<Rectangle> _cflRects = [];
        private List<Rectangle> _cflBkRects = [];
        private Action? _onCflAnimationEnd = null;
        private Color CflCenterColor => UsingDark ? Color.FromArgb(252, 249, 176) : Color.FromArgb(171, 32, 164);
        private const bool CLF_A_DRAW_CORNORS = false; // It seems unnecessary

        private void StartCflAnimation(List<Rectangle> rectangles, Action? onAnimationEnds = null, int frame = 0)
        {
            _onCflAnimationEnd?.Invoke();
            _cflAnimationCurrentFrame = frame;
            _onCflAnimationEnd = onAnimationEnds;

            _cflRects = new(rectangles);
            _cflBkRects = new(rectangles);
        }

        private void CflAnimationNextFrame()
        {
            if (_cflAnimationCurrentFrame < _CflTotalAnimationFrames)
            {
                _cflAnimationCurrentFrame++;
                if (_cflAnimationCurrentFrame == _CflTotalAnimationFrames)
                {
                    _onCflAnimationEnd?.Invoke();
                    _onCflAnimationEnd = null;
                }
            }
        }

        private void DrawCflAnimationFrame(Graphics g)
        {
            if (_cflAnimationCurrentFrame < 0 || _cflAnimationCurrentFrame >= _CflTotalAnimationFrames)
            {
                return;
            }

            foreach (var i in Enumerable.Range(0, _cflRects.Count))
            {
                var rect = _cflRects[i];
                var bkRect = _cflBkRects[i];

                // Create whole rectangle
                var hDelta = (int)((float)rect.Width / bkRect.Width * _blockSize);
                var vDelta = (int)((float)rect.Height / bkRect.Height * _blockSize) * 2;
                Rectangle tempRect = new(
                    rect.X - hDelta, rect.Y - vDelta, rect.Width + hDelta * 2, rect.Height + vDelta * 2
                );

                // Create 4 rectangles for the corners
                hDelta = (tempRect.Width - bkRect.Width) / 2;
                Rectangle topLeft = new(bkRect.X - hDelta, rect.Y - vDelta, hDelta, vDelta);
                Rectangle topRight = new(bkRect.Right, rect.Y - vDelta, hDelta, vDelta);
                Rectangle bottomLeft = new(bkRect.X - hDelta, rect.Bottom, hDelta, vDelta);
                Rectangle bottomRight = new(bkRect.Right, rect.Bottom, hDelta, vDelta);

                var alpha = 155 + (int)(100 * (1 - 1.0 * _cflAnimationCurrentFrame / _CflTotalAnimationFrames));

                // Draw the center rectangle
                using (GraphicsPath path = new())
                {
                    // Create the brush
                    path.AddRectangle(tempRect);
                    using PathGradientBrush brush = new(path)
                    {
                        CenterPoint = new(tempRect.X + tempRect.Width / 2, tempRect.Y + tempRect.Height / 2),
                        CenterColor = Color.FromArgb(alpha, CflCenterColor),
                        SurroundColors = [Color.Transparent],
                        FocusScales = new((float)bkRect.Width / tempRect.Width, (float)rect.Height / tempRect.Height),
                    };

                    path.Reset();
                    path.AddRectangle(tempRect);
                    // Subtract the 4 corners
                    path.SetMarkers();
                    path.AddRectangle(topLeft);
                    path.AddRectangle(topRight);
                    path.AddRectangle(bottomLeft);
                    path.AddRectangle(bottomRight);
                    path.SetMarkers();

                    // Draw the center rectangle
                    g.FillPath(brush, path);
                }

#if CLF_A_DRAW_CORNORS == true
                if (vDelta != 0 )
                {
                    using (GraphicsPath path = new())
                    {
                        // Create the brush
                        path.AddEllipse(new(
                            topLeft.Right - topLeft.Width,
                            topLeft.Bottom - topLeft.Height,
                            topLeft.Width * 2,
                            topLeft.Height * 2
                        ));
                         using PathGradientBrush brush = new(path)
                        {
                            CenterPoint = new(topLeft.Right, topLeft.Bottom),
                            CenterColor = Color.FromArgb(alpha, CflCenterColor),
                            SurroundColors = [Color.Transparent],
                            FocusScales = new(0, 0),
                        };
                        path.Reset();
                        path.AddRectangle(topLeft);
                        g.FillPath(brush, path);
                    }

                    using (GraphicsPath path = new())
                    {
                        // Create the brush
                        path.AddEllipse(new(
                            topRight.Left - topRight.Width,
                            topRight.Bottom - topRight.Height,
                            topRight.Width * 2,
                            topRight.Height * 2
                        ));
                        using PathGradientBrush brush = new(path)
                        {
                            CenterPoint = new(topRight.Left, topRight.Bottom),
                            CenterColor = Color.FromArgb(alpha, CflCenterColor),
                            SurroundColors = [Color.Transparent],
                            FocusScales = new(0, 0),
                        };
                        path.Reset();
                        path.AddRectangle(topRight);
                        g.FillPath(brush, path);
                    }

                    using (GraphicsPath path = new())
                    {
                        // Create the brush
                        path.AddEllipse(new(
                            bottomLeft.Right - bottomLeft.Width,
                            bottomLeft.Top - bottomLeft.Height,
                            bottomLeft.Width * 2,
                            bottomLeft.Height * 2
                        ));
                        using PathGradientBrush brush = new(path)
                        {
                            CenterPoint = new(bottomLeft.Right, bottomLeft.Top),
                            CenterColor = Color.FromArgb(alpha, CflCenterColor),
                            SurroundColors = [Color.Transparent],
                            FocusScales = new(0, 0),
                        };
                        path.Reset();
                        path.AddRectangle(bottomLeft);
                        g.FillPath(brush, path);
                    }

                    using (GraphicsPath path = new())
                    {
                        // Create the brush
                        path.AddEllipse(new(
                            bottomRight.Left - bottomRight.Width,
                            bottomRight.Top - bottomRight.Height,
                            bottomRight.Width * 2,
                            bottomRight.Height * 2
                        ));
                        using PathGradientBrush brush = new(path)
                        {
                            CenterPoint = new(bottomRight.Left, bottomRight.Top),
                            CenterColor = Color.FromArgb(alpha, CflCenterColor),
                            SurroundColors = [Color.Transparent],
                            FocusScales = new(0, 0),
                        };
                        path.Reset();
                        path.AddRectangle(bottomRight);
                        g.FillPath(brush, path);
                    }
                }
#endif
                // The progress of the animation is first fast and then slow
                var progress = Math.Sqrt(1.0 * _cflAnimationCurrentFrame / (_CflTotalAnimationFrames - 1));

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

        // === Hard Drop Animation ===
        private const int _HdTotalAnimationFrames = 15;
        private List<(int frame, Rectangle rect, Block.E_Type belongsTo)> _hdAnimations = [];

        private void StartHdAnimation(Rectangle[] rectangle, Block.E_Type belongsTo)
        {
            _hdAnimations.AddRange(rectangle.Select(r => (0, r, belongsTo)));
        }

        private void HdAnimationNextFrame()
        {
            _hdAnimations = [.. _hdAnimations
                .Select(hdA => (frame: hdA.frame + 1, hdA.rect, hdA.belongsTo))
                .Where(hdA => hdA.frame < _HdTotalAnimationFrames)
            ];
        }

        private void DrawHdAnimationFrame(Graphics g)
        {
            foreach (var (frame, rect, belongsTo) in _hdAnimations)
            {
                // The progress of the animation is
                // Math.Sqrt if wanna first fast and then slow
                // Math.Pow if wanna first slow and then fast
                var alpha = 255 - (int)(255 * (1.0 * frame / _HdTotalAnimationFrames));

                using LinearGradientBrush brush = new(
                    rect,
                    Color.FromArgb(Math.Max(0, alpha - 100), GetColor(belongsTo)), 
                    Color.FromArgb(alpha, GetColor(belongsTo)),
                    LinearGradientMode.Vertical
                );
                g.FillRectangle(brush, rect);
            }
        }
    }
}
