using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace Tetris
{
    internal partial class Panel
    {
        private CustonButton _themeButton = new();

        private void InitializeSettings()
        {
            _themeButton.Text = UsingDark ? "Dark" : "Light";
            //_themeButton.BackColor = Color.Black; // 选择基础背景颜色，这里是黑色
            _themeButton.Click += (sender, e) => ThemeButtonClick();
            this.Controls.Add(_themeButton);
        }

        private void ResizeSettings()
        {
            // Define the block size for the preview, and the start position
            var blockSize = _blockSize * 0.672f;
            var width = blockSize * 4.56f;
            var startX = (ClientSize.Width - _Columns * _blockSize - Padding.Left) * 0.5f - width;
            var startY = (ClientSize.Height - _Rows * _blockSize + Padding.Top) * 0.5f + _blockSize * 4f;

            _themeButton.Size = new((int)width, (int)blockSize * 2);
            _themeButton.Location = new((int)startX, (int)startY);
            _themeButton.Font = new("Arial", blockSize * 0.6f + 1);
        }

        private void ThemeButtonClick()
        {
            UsingDark = !UsingDark;
            _themeButton.Text = UsingDark ? "Dark" : "Light";
        }
    }
}
