namespace Tetris
{
    internal class BitmapPanel : Control
    {
        private Bitmap? _bitmap = null;

        public BitmapPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.Enabled = false;
            this.TabStop = false;
        }

        public Bitmap? InBitmap
        {
            get => _bitmap;
            set
            {
                _bitmap?.Dispose();
                _bitmap = value;
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_bitmap is not null)
            {
                e.Graphics.DrawImage(_bitmap, 0, 0);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bitmap?.Dispose();
            }
            base.Dispose(disposing);
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
