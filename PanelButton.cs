using System.Drawing.Drawing2D;

namespace Tetris
{
    internal partial class Panel
    {
        // Custom button class for the theme switch and unable to be selected
        private class CustonButton : Button
        {
            private Color _defaultBackColor = Color.FromArgb(128, 0, 0, 0); // 半透明黑色背景
            private Color _hoverBackColor = Color.FromArgb(150, 0, 0, 0); // 悬停时更深的灰色
            private Color _clickBackColor = Color.FromArgb(180, 0, 0, 0); // 点击时更深的灰色
            private Color _borderColor = Color.FromArgb(128, 192, 192, 192); // 半透明灰白色边框
            private Color _textColor = Color.FromArgb(128, 255, 255, 255); // 半透明白色字体

            public CustonButton()
            {
                this.SetStyle(ControlStyles.Selectable, false);
                this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                this.TabStop = false;
                this.BackColor = Color.Transparent;
                this.FlatStyle = FlatStyle.Flat;
                this.FlatAppearance.BorderSize = 0;
            }

            protected override void OnPaint(PaintEventArgs pevent)
            {
                base.OnPaint(pevent);

                GraphicsPath path = new();
                int radius = 20; // 圆角半径
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(this.Width - radius - 1, 0, radius, radius, 270, 90);
                path.AddArc(this.Width - radius - 1, this.Height - radius - 1, radius, radius, 0, 90);
                path.AddArc(0, this.Height - radius - 1, radius, radius, 90, 90);
                path.CloseAllFigures();

                this.Region = new Region(path); // 裁剪区域，使按钮外观为圆角

                // 绘制圆角矩形背景
                using (SolidBrush brush = new(this.BackColor))
                {
                    pevent.Graphics.FillPath(brush, path);
                }

                // 绘制半透明边框
                using (Pen pen = new(_borderColor, 2))
                {
                    pevent.Graphics.DrawPath(pen, path);
                }

                // 绘制按钮文本
                TextRenderer.DrawText(pevent.Graphics, this.Text, this.Font, this.ClientRectangle, _textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);
                this.BackColor = _hoverBackColor;
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                this.BackColor = _defaultBackColor;
            }

            protected override void OnMouseDown(MouseEventArgs mevent)
            {
                base.OnMouseDown(mevent);
                this.BackColor = _clickBackColor;
            }

            protected override void OnMouseUp(MouseEventArgs mevent)
            {
                base.OnMouseUp(mevent);
                this.BackColor = _hoverBackColor;
            }

            public new Color BackColor
            {
                get { return _defaultBackColor; }
                set
                {
                    _defaultBackColor = Color.FromArgb(128, value); // 更新透明度和背景颜色
                    this.Invalidate();
                }
            }
        }
    }
}
