using System.Drawing.Drawing2D;

namespace Tetris
{
    internal partial class Panel
    {
        // Custom button class for the theme switch and unable to be selected
        private class CustonButton : Label
        {
            private enum E_State
            {
                Normal, Hover, Pressed,
            }

            private const int _boardWidth = 3;

            private E_State _state = E_State.Normal;

            public bool UsingDark { get; set; } = true;

            private Color BackgroundColor => _state switch
            {
                E_State.Normal => Color.Transparent,
                E_State.Hover => UsingDark ? Color.FromArgb(37, 0, 0, 0) : Color.FromArgb(37, 255, 255, 255),
                E_State.Pressed => UsingDark ? Color.FromArgb(73, 0, 0, 0) : Color.FromArgb(73, 255, 255, 255),
                _ => throw new NotSupportedException(),
            };
            private Color ForegroundColor => UsingDark ? Color.White : Color.Black;
            private Color BoardColor => UsingDark ? Color.White : Color.Black;

            public CustonButton()
            {
                this.TabStop = false;
                this.SetStyle(ControlStyles.Selectable, false);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                this.BackColor = BackgroundColor;
                this.ForeColor = ForegroundColor;
                base.OnPaint(e);

                // Open the anti-aliasing
                //e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Create a path for the rounded rectangle
                int radius = Math.Min(this.Width, this.Height) / 3; // Filet radius
                GraphicsPath path = new();
                path.AddArc(new Rectangle(0, 0, radius, radius), 180, 90);
                path.AddArc(new Rectangle(this.Width - radius, 0, radius, radius), 270, 90);
                path.AddArc(new Rectangle(this.Width - radius, this.Height - radius, radius, radius), 0, 90);
                path.AddArc(new Rectangle(0, this.Height - radius, radius, radius), 90, 90);
                path.CloseAllFigures();

                // Set the region
                this.Region = new Region(path);

                // Draw the background
                using Pen pen = new(BoardColor, _boardWidth);
                // Set the border to be drawn inside the control to reduce jagging
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawPath(pen, path);
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                _state = E_State.Hover;
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                _state = E_State.Normal;
                base.OnMouseLeave(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                _state = E_State.Pressed;
                base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                _state = E_State.Hover;
                base.OnMouseUp(e);
            }
        }
    }
}
