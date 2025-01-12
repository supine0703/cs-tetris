namespace Tetris
{
    public class Thickness(int left, int top, int right, int bottom)
    {
        public Thickness(int all = 0) : this(all, all, all, all) { }

        public int Left { get; set; } = left;
        public int Top { get; set; } = top;
        public int Right { get; set; } = right;
        public int Bottom { get; set; } = bottom;
        public int Horizontal => Left + Right;
        public int Vertical => Top + Bottom;
    }
}
