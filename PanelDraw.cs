using System.Drawing.Drawing2D;

namespace Tetris
{
    internal partial class Panel
    {
        private int _blockSize; // Size of each block
        private Size PanelSize => new(ClientSize.Width - Padding.Horizontal, ClientSize.Height - Padding.Vertical);

        private const bool UsingDark = true;
        private static readonly Color _backColor = UsingDark ? Color.Black : Color.FromArgb(242, 242, 242);
        private static readonly Color _foreColor = UsingDark ? Color.White : Color.Black;
        // Define the gradient colors
        private static readonly Color _backgroundColor_1 = UsingDark ? Color.FromArgb(0, 120, 80) : Color.FromArgb(144, 238, 144);
        private static readonly Color _backgroundColor_2 = UsingDark ? Color.FromArgb(139, 0, 87) : Color.FromArgb(255, 182, 193);

        private void AdujustPanelSize()
        {
            // Adjust block size based on the new size of the window
            _blockSize = Math.Min(ClientSize.Width * 2 / 3 / (_Columns + 2), ClientSize.Height / (_Rows + 2));
            Padding = new(_blockSize);
        }

        private void DrawBackground(Graphics g)
        {
            // Draw the background gradient
            using LinearGradientBrush brush = new(ClientRectangle, _backgroundColor_1, _backgroundColor_2, 270f);
            g.FillRectangle(brush, ClientRectangle);
        }

        private void DrawGrid(Graphics g)
        {
            // Calculate the starting point to center the grid
            var startX = (PanelSize.Width - _Columns * _blockSize) / 2 + Padding.Left;
            var startY = (PanelSize.Height - _Rows * _blockSize) / 2 + Padding.Top;

            using (Pen outerPen = new(_foreColor, 3)) // Outer border pen with .. color
            using (Pen innerPen = new(Color.FromArgb(128, 128, 128, 128))) // Inner border pen with gray semi-transparency
            using (Brush backBrush = new SolidBrush(_backColor)) // Background brush with white color
            {
                // Draw outer border (around the entire grid)
                g.DrawRectangle(outerPen, startX, startY - 1, _Columns * _blockSize + 1, _Rows * _blockSize + 1);

                // Fill the background of the grid
                g.FillRectangle(backBrush, startX, startY, _Columns * _blockSize, _Rows * _blockSize);

                // Move the starting point up by the margin row number
                startY -= _MarginRowNum * _blockSize;

                // Draw inner grid lines
                foreach (var i in Enumerable.Range(_MarginRowNum, _Rows))
                {
                    foreach (var j in Enumerable.Range(0, _Columns))
                    {
                        int x = startX + j * _blockSize;
                        int y = startY + i * _blockSize;

                        // Draw inner border
                        g.DrawRectangle(innerPen, x, y, _blockSize, _blockSize);
                        // Fill the block if it has one
                        if (_gridInfo[i][j].hasBlock)
                        {
                            using Brush brush = new SolidBrush(_gridInfo[i][j].color);
                            g.FillRectangle(brush, x, y, _blockSize, _blockSize);
                            g.DrawRectangle(outerPen, x, y, _blockSize, _blockSize);
                        }
                    }

                    // Draw the row number at the left side of the grid
                    using Font font = new("Arial", 12);
                    // g.DrawString((i).ToString(), font, Brushes.Black, startX - 28, startY + i * _blockSize + 5);
                    g.DrawString((_Rows + _MarginRowNum - i).ToString(), font, Brushes.Black, startX - 28, startY + i * _blockSize + 5);
                }
            }

            // Draw the next block queue preview on the right side of the grid
            DrawNextBlockPreview(g);
        }

        private void DrawBlock(Graphics g)
        {
            var startX = (PanelSize.Width - _Columns * _blockSize) / 2 + Padding.Left;
            var startY = (PanelSize.Height - _Rows * _blockSize) / 2 + Padding.Top;

            var pos = _currentBlock.Pos;
            var radius = _blockSize * 0.125f;
            using Brush centerBrush = new SolidBrush(_foreColor);

            // Draw the shadow of the block
            using Pen shadowPen = new(Color.FromArgb(72, _foreColor), 3);
            // using Brush shadowBack = new SolidBrush(Color.FromArgb(36, _foreColor));
            using Brush shadowBrush = new SolidBrush(Color.FromArgb(72, GetColor(_currentBlock.BlockType)));
            Block block = new(_currentBlock.BlockType, _currentBlock.FacingDirection, pos);
            FallBottom(block);
            foreach (var placeholder in block.Placeholders)
            {
                var x = startX + (placeholder.x + block.Pos.x) * _blockSize;
                var y = startY + (placeholder.y + block.Pos.y) * _blockSize;
                // Draw the shadow grid
                // g.FillRectangle(shadowBack, x, y, _blockSize, _blockSize);
                g.FillRectangle(shadowBrush, x, y, _blockSize, _blockSize);
                g.DrawRectangle(shadowPen, x, y, _blockSize, _blockSize);
                // Draw the in the center of the shadow center
                var shadowCenterX = startX + (block.RotateCenter.x + block.Pos.x) * _blockSize;
                var shadowCenterY = startY + (block.RotateCenter.y + block.Pos.y) * _blockSize;
                g.FillEllipse(centerBrush, shadowCenterX - radius, shadowCenterY - radius, radius * 2, radius * 2);
            }

            // Draw the block
            using Pen pen = new(_foreColor, 3);
            using Brush brush = new SolidBrush(GetColor(_currentBlock.BlockType));
            foreach (var placeholder in _currentBlock.Placeholders)
            {
                var x = startX + (placeholder.x + pos.x) * _blockSize;
                var y = startY + (placeholder.y + pos.y) * _blockSize;

                // Draw the block grid
                g.FillRectangle(brush, x, y, _blockSize, _blockSize);
                g.DrawRectangle(pen, x, y, _blockSize, _blockSize);
            }
            // Draw the in the center of the block
            var centerX = startX + (_currentBlock.RotateCenter.x + pos.x) * _blockSize;
            var centerY = startY + (_currentBlock.RotateCenter.y + pos.y) * _blockSize;
            g.FillEllipse(centerBrush, centerX - radius, centerY - radius, radius * 2, radius * 2);
        }

        private void DrawNextBlockPreview(Graphics g)
        {
            var previewStartX = (PanelSize.Width + _Columns * _blockSize) / 2 + Padding.Left + _blockSize / 2;
            var previewStartY = (PanelSize.Height - _Rows * _blockSize) / 2 + Padding.Top + _blockSize / 2;

            // Draw the preview border
            using (Pen previewPen = new(_foreColor, 2))
            {
                g.DrawRectangle(previewPen, previewStartX, previewStartY, 3 * _blockSize, 14.4f * _blockSize);
            }

            previewStartX += _blockSize / 2;
            previewStartY += _blockSize / 2;

            // Draw the preview blocks
            foreach (Block block in _nextBlocks)
            {
                using Pen pen = new(_foreColor, 1);
                using Brush brush = new SolidBrush(GetColor(block.BlockType));
                using Brush centerBrush = new SolidBrush(_foreColor);

                var blockSize = _blockSize / 2;
                var xDiff = 0;
                if (block.BlockType != Block.E_BlockType.I && block.BlockType != Block.E_BlockType.O)
                {
                    xDiff = blockSize / 2;
                }

                foreach (var placeholder in block.Placeholders)
                {
                    var x = previewStartX + placeholder.x * blockSize;
                    var y = previewStartY + placeholder.y * blockSize;

                    // Draw the block grid
                    g.FillRectangle(brush, x + xDiff, y, blockSize, blockSize);
                    g.DrawRectangle(pen, x + xDiff, y, blockSize, blockSize);
                }
                // Draw the shadow center
                var centerX = previewStartX + (block.RotateCenter.x) * blockSize + xDiff;
                var centerY = previewStartY + (block.RotateCenter.y) * blockSize;
                var radius = blockSize * 0.125f;
                g.FillEllipse(centerBrush, centerX - radius, centerY - radius, radius * 2, radius * 2);

                previewStartY += 4 * blockSize + 10; // 移动到下一个方块的预览位置
            }
        }

        private void DrawBeEatProgress(Graphics g)
        {
            var startX = (PanelSize.Width + _Columns * _blockSize) / 2 + Padding.Left + _blockSize;
            var startY = (PanelSize.Height - _Rows * _blockSize) / 2 + Padding.Top + _blockSize * 16.4f;
            var radius = _blockSize; // Ring radius
            var thickness = _blockSize / 2; // Ring thickness

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw the background ring
            g.DrawEllipse(new Pen(Color.FromArgb(64, _foreColor), thickness), startX, startY, radius * 2, radius * 2);

            // Draw the progress ring
            float sweepAngle = 360 * AutoEatProgressPercent();
            using var pen = new Pen(UsingDark ? Color.Peru : Color.Blue, thickness);
            g.DrawArc(pen, startX, startY, radius * 2, radius * 2, -90, sweepAngle);
        }

        private static Color GetColor(Block.E_BlockType type) => UsingDark ? (type switch
        {
            // Select the color based on the block type for light mode
            Block.E_BlockType.I => Color.FromArgb(121, 184, 169), // 青绿
            Block.E_BlockType.O => Color.FromArgb(212, 177, 72),  // 黄色
            Block.E_BlockType.T => Color.FromArgb(172, 69, 165),  // 紫红色
            Block.E_BlockType.J => Color.FromArgb(81, 120, 220),  // 蓝色
            Block.E_BlockType.L => Color.FromArgb(220, 130, 70),  // 橙红色
            Block.E_BlockType.S => Color.FromArgb(52, 139, 43),   // 深绿色
            Block.E_BlockType.Z => Color.FromArgb(201, 70, 90),   // 红色
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        }) : (type switch
        {
            // Select the color based on the block type for light mode
            Block.E_BlockType.I => Color.FromArgb(184, 231, 223), // 青绿
            Block.E_BlockType.O => Color.FromArgb(240, 230, 140), // 黄色
            Block.E_BlockType.T => Color.FromArgb(200, 150, 180), // 紫红色
            Block.E_BlockType.J => Color.FromArgb(100, 150, 220), // 蓝色
            Block.E_BlockType.L => Color.FromArgb(220, 130, 70),  // 橙红色
            Block.E_BlockType.S => Color.FromArgb(130, 170, 110), // 绿松石色
            Block.E_BlockType.Z => Color.FromArgb(230, 100, 100), // 红色
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        });
    }
}
