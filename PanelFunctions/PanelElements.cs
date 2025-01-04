using System.Drawing.Drawing2D;

namespace Tetris
{
    internal partial class TetrisPanel
    {
        // Custom button class for the theme switch and unable to be selected
        private class CustonButton : Label
        {
            private enum E_State
            {
                Normal, Hover, Pressed,
            }

            private const int _BoardWidth = 3;

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

            public float FiletRatio { get; set; } = 0.333f;

            public CustonButton()
            {
                this.TextAlign = ContentAlignment.MiddleCenter;
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
                var radius = (int)(Math.Min(this.Width, this.Height) * FiletRatio); // Filet ratio
                GraphicsPath path = new();
                path.AddArc(new Rectangle(0, 0, radius, radius), 180, 90);
                path.AddArc(new Rectangle(this.Width - radius, 0, radius, radius), 270, 90);
                path.AddArc(new Rectangle(this.Width - radius, this.Height - radius, radius, radius), 0, 90);
                path.AddArc(new Rectangle(0, this.Height - radius, radius, radius), 90, 90);
                path.CloseAllFigures();

                // Set the region
                this.Region = new Region(path);

                // Draw the background
                using Pen pen = new(BoardColor, _BoardWidth);
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

        // Custom label class for the theme switch
        private class CustomLabel : Label
        {
            public bool UsingDark { get; set; } = true;

            public Color ForeColor1 { get; set; } = Color.White;
            public Color ForeColor2 { get; set; } = Color.Black;

            public CustomLabel()
            {
                this.TextAlign = ContentAlignment.MiddleCenter;
                this.TabStop = false;
                this.SetStyle(ControlStyles.Selectable, false);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                this.BackColor = Color.Transparent;
                this.ForeColor = UsingDark ? ForeColor1 : ForeColor2;
                base.OnPaint(e);
            }
        }

        // Custom numeric up down class for unable to be selected
        private class CustomNumericUpDown : NumericUpDown
        {
            public CustomNumericUpDown()
            {
                this.TextAlign = HorizontalAlignment.Center;
                this.TabStop = false;
                this.SetStyle(ControlStyles.Selectable, false);
            }

            protected override void OnGotFocus(EventArgs e)
            {
                base.OnGotFocus(e);
                this.Parent?.Focus();
            }
        }
    }
}
