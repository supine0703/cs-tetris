//#define CLF_ANIMATION_ONLY_RENDER_SLEF
#define CLF_ANIMATION_DRAW_CORNORS // Corners seems unnecessary

using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace Tetris
{
    internal partial class GameControl
    {
        private void InitalizeGameAnimation()
        {
            _frameTimer.Tick += (sender, e) => AllAnimationNextFrame();
            InitCflAnimation();
        }

        private void AllAnimationNextFrame()
        {
            if (GameState == E_GameState.Paused) { return; }

            CflAnimationNextFrame();
            HdAnimationNextFrame();
        }

        private void DrawAllAnimationFrame(PaintEventArgs e)
        {
            DrawCflAnimationFrame(e);
            DrawHdAnimationFrame(e);
        }

        private void ResetAllAnimation()
        {
            // Reset the Clear Full Lines Animation
            _cflAnimations.Clear();
            _cflAEndToDoActions.Clear();
            // Reset the Hard Drop Animation
            _hdAnimations.Clear();
        }

        // === Clear Full Lines Animation ===
        //private const bool CLF_ANIMATION_ONLY_RENDER_SLEF = false; // Full rendering may not reach 60FPS
        //private const bool CLF_ANIMATION_DRAW_CORNORS = true; // Corners seems unnecessary
        private const string _CflAnimationKey = "Clear-Full-Line(s)-Animation";
        private const int _CflTotalAnimationFrames = 24; // Line Clear Delay
        private const float _CflHorizontalExtensionRatio = 0.391f;
        private readonly Color[] _cflCenterColors = [Color.FromArgb(252, 249, 176), Color.FromArgb(171, 32, 164)];
        private List<(int frame, Point location)> _cflAnimations = [];
        private List<(int frame, Action toDo)> _cflAEndToDoActions = [];

        private (Size inner, Size outer) GetCflAnimationSize(int cellSize, int frame)
        {
            // The progress of the animation is first fast and then slow
            var progress = Math.Sqrt(1.0 * frame / (_CflTotalAnimationFrames - 1));

            Size inner = new(
                (int)(_MatrixColumns * cellSize),
                (int)Math.Max(1, cellSize * (1 - progress))
            );
            Size outer = new(
                (int)((inner.Width + cellSize * 2) * (1 + progress * _CflHorizontalExtensionRatio)),
                (int)(inner.Height + (cellSize * (1 - progress) * 2))
            );
            return (inner, outer);
        }

        private (int x, int y, Size outer) GetCflAnimationPosDelta(int cellSize, int frame)
        {
            var (inner, outer) = GetCflAnimationSize(cellSize, frame);
            var xDelta = (outer.Width - inner.Width) / 2;
            var yDelta = (outer.Height - cellSize) / 2;
            return (xDelta, yDelta, outer);
        }

        private void InitCflAnimation()
        {
            _bitmapManager.RegisterFrameAnimation(_CflAnimationKey, _CflTotalAnimationFrames, CreateCflAnimation);
        }

        private void StartCflAnimation(Point[] locations, Action? endToDo = null)
        {
            _cflAnimations.AddRange(locations.Select(l => (0, l)));
            if (endToDo is not null) { _cflAEndToDoActions.Add((_CflTotalAnimationFrames, endToDo)); }
        }

        private void CflAnimationNextFrame()
        {
            // Execute the actions that should be done at the end of the animation
            _cflAEndToDoActions = _cflAEndToDoActions.Select(cflToDo => {
                if (cflToDo.frame == 1) { cflToDo.toDo.Invoke(); }
                return (frame: cflToDo.frame - 1, cflToDo.toDo);
            }).Where(cflToDo => cflToDo.frame > 0).ToList();
            // Remove the animations that have reached the end
            _cflAnimations = _cflAnimations.Select(cflA =>
            {
#if CLF_ANIMATION_ONLY_RENDER_SLEF
                if (cflA.frame % 2 != 0 || cflA.frame == _CflTotalAnimationFrames - 1)
#endif
                {
                    var (deltaX, deltaY, outer) = GetCflAnimationPosDelta(CellSize, cflA.frame);
                    Point pos = new(cflA.location.X - deltaX, cflA.location.Y - deltaY);
                    FrameInvalidate(new Rectangle(pos, outer));
                    (deltaX, deltaY, outer) = GetCflAnimationPosDelta(CellSize, cflA.frame - 1);
                    pos = new(cflA.location.X - deltaX, cflA.location.Y - deltaY);
                    FrameInvalidate(new Rectangle(pos, outer));
                }
                return (frame: cflA.frame + 1, cflA.location);
            }).Where(cflA => cflA.frame < _CflTotalAnimationFrames).ToList();
        }

        private void DrawCflAnimationFrame(PaintEventArgs e)
        {
            _cflAnimations.ForEach(cflA =>
            {
                var (deltaX, deltaY, outer) = GetCflAnimationPosDelta(CellSize, cflA.frame);
                Point pos = new(cflA.location.X - deltaX, cflA.location.Y - deltaY);
                if (e.ClipRectangle.IntersectsWith(new Rectangle(pos, outer)))
                {
                    e.Graphics.DrawImage(_bitmapManager.GetFrameAnimation(_CflAnimationKey, cflA.frame), pos);
                }
            });
        }

        private Bitmap CreateCflAnimation(int cellSize, int frame)
        {
            //var alpha = 80 + (int)(175 * (1 - Math.Sqrt(1.0 * frame / (_CflTotalAnimationFrames - 1))));
            var alpha = 155 + (int)(100 * (1 - Math.Sqrt(1.0 * frame / (_CflTotalAnimationFrames - 1))));

            var (inner, outer) = GetCflAnimationSize(cellSize, frame);

            Rectangle outerRect = new(0, 0, outer.Width, outer.Height);

            var hDelta = (outer.Width - inner.Width) / 2;
            var vDelta = (outer.Height - inner.Height) / 2;
            Rectangle topLeft = new(0, 0, hDelta, vDelta);
            Rectangle topRight = new(inner.Width + hDelta, 0, hDelta, vDelta);
            Rectangle bottomLeft = new(0, inner.Height + vDelta, hDelta, vDelta);
            Rectangle bottomRight = new(inner.Width + hDelta, inner.Height + vDelta, hDelta, vDelta);

            Bitmap bitmap = new(outer.Width, outer.Height);
            using Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.Transparent);

            // Draw the center rectangle
            using (GraphicsPath path = new())
            {
                // Create the brush
                path.AddRectangle(outerRect);
                using PathGradientBrush brush = new(path)
                {
                    CenterPoint = new(outerRect.X + outerRect.Width / 2, outerRect.Y + outerRect.Height / 2),
                    CenterColor = Color.FromArgb(alpha, _cflCenterColors[ThemeIndex]),
                    SurroundColors = [Color.Transparent],
                    FocusScales = new((float)inner.Width / outer.Width, (float)inner.Height / outer.Height),
                };

                path.Reset();
                path.AddRectangle(outerRect);
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

#if CLF_ANIMATION_DRAW_CORNORS
            if (hDelta > 0 && vDelta > 0)
            {
                // Draw top left corner
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
                        CenterColor = Color.FromArgb(alpha, _cflCenterColors[ThemeIndex]),
                        SurroundColors = [Color.Transparent],
                        FocusScales = new(0, 0),
                    };
                    path.Reset();
                    path.AddRectangle(topLeft);
                    g.FillPath(brush, path);
                }
                // Draw top right corner
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
                        CenterColor = Color.FromArgb(alpha, _cflCenterColors[ThemeIndex]),
                        SurroundColors = [Color.Transparent],
                        FocusScales = new(0, 0),
                    };
                    path.Reset();
                    path.AddRectangle(topRight);
                    g.FillPath(brush, path);
                }
                // Draw bottom left corner
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
                        CenterColor = Color.FromArgb(alpha, _cflCenterColors[ThemeIndex]),
                        SurroundColors = [Color.Transparent],
                        FocusScales = new(0, 0),
                    };
                    path.Reset();
                    path.AddRectangle(bottomLeft);
                    g.FillPath(brush, path);
                }
                // Draw bottom right corner
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
                        CenterColor = Color.FromArgb(alpha, _cflCenterColors[ThemeIndex]),
                        SurroundColors = [Color.Transparent],
                        FocusScales = new(0, 0),
                    };
                    path.Reset();
                    path.AddRectangle(bottomRight);
                    g.FillPath(brush, path);
                }
            }
#endif

            return bitmap;
        }

        // === Hard Drop Animation ===
        private const int _HdTotalAnimationFrames = 17;
        private List<(int frame, Rectangle rect, Piece.E_Type belongsTo)> _hdAnimations = [];

        private void StartHdAnimation(Rectangle[] rectangle, Piece.E_Type belongsTo)
        {
            _hdAnimations.AddRange(rectangle.Select(r => (0, r, belongsTo)));
        }

        private void HdAnimationNextFrame()
        {
            _hdAnimations = _hdAnimations.Select(hdA =>
            {
                FrameInvalidate(hdA.rect);
                return (frame: hdA.frame + 1, hdA.rect, hdA.belongsTo);
            }).Where(hdA => hdA.frame < _HdTotalAnimationFrames).ToList();
        }

        private void DrawHdAnimationFrame(PaintEventArgs e)
        {
            foreach (var (frame, rect, belongsTo) in _hdAnimations)
            {
                if (!e.ClipRectangle.IntersectsWith(rect)) { continue; }
                // The progress of the animation is
                // Math.Sqrt if wanna first fast and then slow
                // Math.Pow if wanna first slow and then fast
                var alpha = 255 - (int)(240 * Math.Sqrt(1.0 * frame / _HdTotalAnimationFrames));
                //var alpha = 255 - (int)(255 * Math.Pow(1.0 * frame / _HdTotalAnimationFrames, 2));
                using LinearGradientBrush brush = new(
                    rect,
                    Color.FromArgb(Math.Max(0, alpha - 100), Piece.GetColors(belongsTo)[ThemeIndex]),
                    Color.FromArgb(alpha, Piece.GetColors(belongsTo)[ThemeIndex]),
                    LinearGradientMode.Vertical
                );
                e.Graphics.FillRectangle(brush, rect);
            }
        }
    }
}
