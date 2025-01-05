using System.Drawing.Drawing2D;
using static System.Windows.Forms.AxHost;

namespace Tetris
{
    internal partial class GameControl
    {
        // Custom button class for the theme switch and unable to be selected
        private class CustomButton : Control
        {
            private enum E_State { Normal, Hover, Pressed, }

            private E_State _state = default;

            public int BorderThickness { get; set; } = 3;
            public float FiletRatio { get; set; } = 0.333f;
            public Color BorderColor { get; set; } = Color.FromArgb(242, 242, 242, 242);
            public Color HoverBackColor { get; set; } = Color.FromArgb(37, 0, 0, 0);
            public Color PressedBackColor { get; set; } = Color.FromArgb(73, 0, 0, 0);
            public new Color BackColor { get; set; } = Color.Transparent;
            public new Color ForeColor { get; set; } = Color.White;

            public CustomButton()
            {
                this.TabStop = false;
                this.DoubleBuffered = true;
                this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                this.SetStyle(ControlStyles.Selectable, false);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

                // Set the base class ForeColor and BackColor based on the state
                base.ForeColor = this.ForeColor;
                base.BackColor = _state switch
                {
                    E_State.Normal => this.BackColor,
                    E_State.Hover => HoverBackColor,
                    E_State.Pressed => PressedBackColor,
                    _ => throw new NotImplementedException(),
                };

                // Call methods to draw rounded rectangle and text
                DrawRoundedRectangle(e.Graphics, this.Width, this.Height);
                DrawButtonText(e.Graphics);
            }

            private void DrawRoundedRectangle(Graphics graphics, int width, int height)
            {
                if (BorderThickness == 0) { return; }

                // Calculate the radius for the rounded corners
                var radius = (int)(Math.Min(width, height) * FiletRatio);
                GraphicsPath path = new();

                // Create rounded rectangle path
                path.AddArc(new Rectangle(0, 0, radius, radius), 180, 90);
                path.AddArc(new Rectangle(width - radius, 0, radius, radius), 270, 90);
                path.AddArc(new Rectangle(width - radius, height - radius, radius, radius), 0, 90);
                path.AddArc(new Rectangle(0, height - radius, radius, radius), 90, 90);
                path.CloseAllFigures();

                // Set the region of the control to the rounded rectangle path
                this.Region = new Region(path);

                // Draw the border of the control
                using Pen pen = new(BorderColor, BorderThickness)
                {
                    Alignment = PenAlignment.Inset // Draw border inside the control to reduce jagging
                };
                graphics.DrawPath(pen, path);
            }

            private void DrawButtonText(Graphics graphics)
            {
                // Draw the text on the button
                using Brush textBrush = new SolidBrush(this.ForeColor);
                var textSize = graphics.MeasureString(this.Text, this.Font);
                var textPoint = new PointF(
                    (this.Width - textSize.Width) / 2,
                    (this.Height - textSize.Height) / 2
                );
                graphics.DrawString(this.Text, this.Font, textBrush, textPoint);
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                _state = E_State.Hover;
                Invalidate(); // Redraw the control to reflect the state change
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                _state = E_State.Normal;
                Invalidate(); // Redraw the control to reflect the state change
                base.OnMouseLeave(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                _state = E_State.Pressed;
                Invalidate(); // Redraw the control to reflect the state change
                base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                _state = E_State.Hover;
                Invalidate(); // Redraw the control to reflect the state change
                base.OnMouseUp(e);
            }

            protected override void OnGotFocus(EventArgs e) { this.Parent?.Focus(); }
            protected override void OnMouseHover(EventArgs e) { }
            protected override void OnMouseWheel(MouseEventArgs e) { }
            protected override void OnMouseClick(MouseEventArgs e) { }
            protected override void OnMouseDoubleClick(MouseEventArgs e) { }
            protected override void OnMouseMove(MouseEventArgs e) { }
            protected override void OnMouseCaptureChanged(EventArgs e) { }
        }

        // Custom label class for the theme switch
        private class CustomLabel : Control
        {
            public int BorderThickness { get; set; } = 0;
            public float FiletRatio { get; set; } = 0.333f;
            public Color BorderColor { get; set; } = Color.White;
            public new Color BackColor { get; set; } = Color.Transparent;
            public new Color ForeColor { get; set; } = Color.White;
            public ContentAlignment TextAlign { get; set; } = ContentAlignment.MiddleCenter; // New property for text alignment

            public CustomLabel()
            {
                this.TabStop = false;
                this.DoubleBuffered = true;
                this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                this.SetStyle(ControlStyles.Selectable, false);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

                // Set the base class ForeColor and BackColor
                base.ForeColor = this.ForeColor;
                base.BackColor = this.BackColor;

                // Call methods to draw rounded rectangle and text
                DrawRoundedRectangle(e.Graphics, this.Width, this.Height);
                DrawButtonText(e.Graphics);
            }

            private void DrawRoundedRectangle(Graphics graphics, int width, int height)
            {
                if (BorderThickness == 0) { return; }

                // Calculate the radius for the rounded corners
                var radius = (int)(Math.Min(width, height) * FiletRatio);
                GraphicsPath path = new();

                // Create rounded rectangle path
                path.AddArc(new Rectangle(0, 0, radius, radius), 180, 90);
                path.AddArc(new Rectangle(width - radius, 0, radius, radius), 270, 90);
                path.AddArc(new Rectangle(width - radius, height - radius, radius, radius), 0, 90);
                path.AddArc(new Rectangle(0, height - radius, radius, radius), 90, 90);
                path.CloseAllFigures();

                // Set the region of the control to the rounded rectangle path
                this.Region = new Region(path);

                // Draw the border of the control
                using Pen pen = new(BorderColor, BorderThickness)
                {
                    Alignment = PenAlignment.Inset // Draw border inside the control to reduce jagging
                };
                graphics.DrawPath(pen, path);
            }

            private void DrawButtonText(Graphics graphics)
            {
                using Brush textBrush = new SolidBrush(this.ForeColor);
                var textSize = graphics.MeasureString(this.Text, this.Font);
                PointF textPoint = GetTextPosition(textSize);

                graphics.DrawString(this.Text, this.Font, textBrush, textPoint);
            }

            private PointF GetTextPosition(SizeF textSize)
            {
                float x = 0f;
                float y = 0f;

                switch (TextAlign)
                {
                    case ContentAlignment.TopLeft:
                        x = 0;
                        y = 0;
                        break;
                    case ContentAlignment.TopCenter:
                        x = (this.Width - textSize.Width) / 2;
                        y = 0;
                        break;
                    case ContentAlignment.TopRight:
                        x = this.Width - textSize.Width;
                        y = 0;
                        break;
                    case ContentAlignment.MiddleLeft:
                        x = 0;
                        y = (this.Height - textSize.Height) / 2;
                        break;
                    case ContentAlignment.MiddleCenter:
                        x = (this.Width - textSize.Width) / 2;
                        y = (this.Height - textSize.Height) / 2;
                        break;
                    case ContentAlignment.MiddleRight:
                        x = this.Width - textSize.Width;
                        y = (this.Height - textSize.Height) / 2;
                        break;
                    case ContentAlignment.BottomLeft:
                        x = 0;
                        y = this.Height - textSize.Height;
                        break;
                    case ContentAlignment.BottomCenter:
                        x = (this.Width - textSize.Width) / 2;
                        y = this.Height - textSize.Height;
                        break;
                    case ContentAlignment.BottomRight:
                        x = this.Width - textSize.Width;
                        y = this.Height - textSize.Height;
                        break;
                }

                return new PointF(x, y);
            }

            protected override void OnGotFocus(EventArgs e) { this.Parent?.Focus(); }
            protected override void OnMouseEnter(EventArgs e) { }
            protected override void OnMouseLeave(EventArgs e) { }
            protected override void OnMouseHover(EventArgs e) { }
            protected override void OnMouseWheel(MouseEventArgs e) { }
            protected override void OnMouseClick(MouseEventArgs e) { }
            protected override void OnMouseDoubleClick(MouseEventArgs e) { }
            protected override void OnMouseDown(MouseEventArgs e) { }
            protected override void OnMouseUp(MouseEventArgs e) { }
            protected override void OnMouseMove(MouseEventArgs e) { }
            protected override void OnMouseCaptureChanged(EventArgs e) { }
        }
    }
}
