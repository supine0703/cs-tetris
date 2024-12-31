using System.Drawing.Drawing2D;

namespace Tetris
{
    internal partial class Panel
    {
        private int _blockSize; // Size of each block

        private bool UsingDark { get; set; } = true;
        private Color BackgroundColor => UsingDark ? Color.Black : Color.FromArgb(248, 247, 247);
        private Color ForegroundColor => UsingDark ? Color.White : Color.Black;
        // Define the gradient colors
        private Color BackgroundColor_1 => UsingDark ? Color.FromArgb(0, 120, 80) : Color.FromArgb(144, 238, 144);
        private Color BackgroundColor_2 => UsingDark ? Color.FromArgb(139, 0, 87) : Color.FromArgb(255, 182, 193);

        private void AdujustPanelSize()
        {
            // Adjust block size based on the new size of the window
            _blockSize = Math.Min(ClientSize.Width * 2 / 3 / (_Columns + 2), ClientSize.Height / (_Rows + 2));
            Padding = new(_blockSize);
        }

        private void DrawBackground(Graphics g)
        {
            // Draw the background gradient
            using LinearGradientBrush brush = new(ClientRectangle, BackgroundColor_1, BackgroundColor_2, 270f);
            g.FillRectangle(brush, ClientRectangle);
        }

        private void DrawPlayfield(Graphics g)
        {
            // Calculate the starting point to center the cell
            var startX = (ClientSize.Width - _Columns * _blockSize) / 2;
            var startY = (ClientSize.Height - _Rows * _blockSize) / 2;

            using (Pen outerPen = new(ForegroundColor, 4)) // Outer border pen with .. color
            using (Pen innerPen = new(Color.FromArgb(128, 128, 128, 128))) // Inner border pen with gray semi-transparency
            using (SolidBrush backBrush = new(BackgroundColor)) // Background brush with white color
            {
                // Draw outer border (around the entire playfield)
                g.DrawRectangle(outerPen, startX - 2, startY - 2, _Columns * _blockSize + 4, _Rows * _blockSize + 4);

                // Fill the background of the playfield
                g.FillRectangle(backBrush, startX, startY, _Columns * _blockSize, _Rows * _blockSize);

                // Move the starting point up by the margin row number
                startY -= _MarginRowNum * _blockSize;

                // Draw inner cell lines
                foreach (var i in Enumerable.Range(_MarginRowNum, _Rows))
                {
                    foreach (var j in Enumerable.Range(0, _Columns))
                    {
                        int x = startX + j * _blockSize;
                        int y = startY + i * _blockSize;

                        // Fill the block if it has one
                        if (_playfieldInfo[i][j].state == E_CellState.Occupied)
                        {
                            using SolidBrush brush = new(GetColor(_playfieldInfo[i][j].belongsTo));
                            g.FillRectangle(brush, x, y, _blockSize, _blockSize);
                            g.DrawRectangle(outerPen, x, y, _blockSize, _blockSize);
                        }
                        // Draw inner border
                        else
                        {
                            g.DrawRectangle(innerPen, x, y, _blockSize, _blockSize);
                        }
                    }
                }
            }

            // Draw the next block queue preview on the right side of the playfield
            DrawNextBlockPreview(g);

            // Draw the hold block preview on the left side of the playfield
            DrawHoldBlock(g);
        }

        private void DrawBlock(Graphics g)
        {
            if (_currentBlock is null)
            {
                return;
            }

            var startX = (ClientSize.Width - _Columns * _blockSize) / 2;
            var startY = (ClientSize.Height - _Rows * _blockSize) / 2;

            var pos = _currentBlock.Pos;
            var radius = _blockSize * 0.125f;

            // Draw the shadow of the block
            using (Pen shadowPen = new(Color.FromArgb(81, ForegroundColor), 3))
            using (SolidBrush shadowBrush = new(Color.FromArgb(81, GetColor(_currentBlock.BlockType))))
            using (SolidBrush centerBrush = new(Color.FromArgb(81, ForegroundColor)))
            {
                Block block = new(_currentBlock.BlockType, _currentBlock.Orientation, pos);
                DropBottom(block);
                foreach (var placeholder in block.Placeholders)
                {
                    var x = startX + (placeholder.x + block.Pos.x) * _blockSize;
                    var y = startY + (placeholder.y + block.Pos.y) * _blockSize;
                    // Draw the shadow cells
                    g.FillRectangle(shadowBrush, x, y, _blockSize, _blockSize);
                    g.DrawRectangle(shadowPen, x, y, _blockSize, _blockSize);
                    // Draw the in the center of the shadow center
                    var shadowCenterX = startX + (block.RotateCenter.x + block.Pos.x) * _blockSize;
                    var shadowCenterY = startY + (block.RotateCenter.y + block.Pos.y) * _blockSize;
                    g.FillEllipse(centerBrush, shadowCenterX - radius, shadowCenterY - radius, radius * 2, radius * 2);
                }
            }
            // Draw the block
            using (Pen pen = new(ForegroundColor, 3))
            using (SolidBrush brush = new(GetColor(_currentBlock.BlockType)))
            using (SolidBrush centerBrush = new(ForegroundColor))
            {
                foreach (var placeholder in _currentBlock.Placeholders)
                {
                    var x = startX + (placeholder.x + pos.x) * _blockSize;
                    var y = startY + (placeholder.y + pos.y) * _blockSize;

                    // Draw the block cells
                    g.FillRectangle(brush, x, y, _blockSize, _blockSize);
                    g.DrawRectangle(pen, x, y, _blockSize, _blockSize);
                }
                // Draw the in the center of the block
                var centerX = startX + (_currentBlock.RotateCenter.x + pos.x) * _blockSize;
                var centerY = startY + (_currentBlock.RotateCenter.y + pos.y) * _blockSize;
                g.FillEllipse(centerBrush, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }
        }

        private void DrawNextBlockPreview(Graphics g)
        {
            var startX = (ClientSize.Width + _Columns * _blockSize + Padding.Right) * 0.5f;
            var startY = (ClientSize.Height - _Rows * _blockSize + Padding.Top) * 0.5f + _blockSize * 0.4f;

            // Define the block size for the preview
            var blockSize = _blockSize * 0.672f;

            // Draw the preview border
            using (Pen previewPen = new(ForegroundColor, 2))
            using (SolidBrush backBrush = new(Color.FromArgb(64, BackgroundColor)))
            {
                g.FillRectangle(backBrush, startX, startY, blockSize * 4.56f, blockSize * 19.6f);
                g.DrawRectangle(previewPen, startX, startY, blockSize * 4.56f, blockSize * 19.6f);
            }

            startX += blockSize * 0.28f;
            startY -= blockSize * 0.28f;

            // Draw the preview blocks
            foreach (Block block in _nextBlocks)
            {
                using Pen pen = new(ForegroundColor, 1);
                using SolidBrush brush = new(GetColor(block.BlockType));
                using SolidBrush centerBrush = new(ForegroundColor);

                // Define the offset for the block center
                float xDiff = 0;
                float yDiff = 0;
                if (block.BlockType == Block.E_BlockType.I)
                {
                    yDiff = blockSize * 0.5f;
                }
                else if (block.BlockType != Block.E_BlockType.O)
                {
                    xDiff = blockSize * 0.5f;
                }

                // Draw the block
                foreach (var placeholder in block.Placeholders)
                {
                    var x = startX + placeholder.x * blockSize;
                    var y = startY + placeholder.y * blockSize;

                    // Draw the block cells
                    g.FillRectangle(brush, x + xDiff, y + yDiff, blockSize, blockSize);
                    g.DrawRectangle(pen, x + xDiff, y + yDiff, blockSize, blockSize);
                }
                // Draw the shadow center
                var centerX = startX + (block.RotateCenter.x) * blockSize + xDiff;
                var centerY = startY + (block.RotateCenter.y) * blockSize + yDiff;
                var radius = blockSize * 0.125f;
                g.FillEllipse(centerBrush, centerX - radius, centerY - radius, radius * 2, radius * 2);

                startY += 2.8f * blockSize + 10; // Move the starting point to the next block
            }
        }

        private void DrawHoldBlock(Graphics g)
        {
            var startX = (ClientSize.Width - _Columns * _blockSize - Padding.Left) * 0.5f;
            var startY = (ClientSize.Height - _Rows * _blockSize + Padding.Top) * 0.5f + _blockSize * 0.4f;

            // Define the block size for the preview
            var blockSize = _blockSize * 0.672f;

            // Draw the hold border
            using (Pen holdPen = new(ForegroundColor, 2))
            using (SolidBrush backBrush = new(Color.FromArgb(64, BackgroundColor)))
            {
                var w = blockSize * 4.56f;
                startX -= w;
                g.DrawRectangle(holdPen, startX, startY, w, blockSize * 3.6f);
                g.FillRectangle(backBrush, startX, startY, w, blockSize * 3.6f);
            }

            startX += blockSize * 0.28f;
            startY -= blockSize * 0.28f;

            var block = _holdBlock;

            if (block is not null)
            {
                using Pen pen = new(ForegroundColor, 1);
                using SolidBrush brush = new(GetColor(block.BlockType));
                using SolidBrush centerBrush = new(ForegroundColor);

                // Define the offset for the block center
                float xDiff = 0;
                float yDiff = 0;
                if (block.BlockType == Block.E_BlockType.I)
                {
                    yDiff = blockSize / 2;
                }
                else if (block.BlockType != Block.E_BlockType.O)
                {
                    xDiff = blockSize / 2;
                }

                // Draw the block
                foreach (var placeholder in block.Placeholders)
                {
                    var x = startX + placeholder.x * blockSize;
                    var y = startY + placeholder.y * blockSize;

                    // Draw the block cells
                    g.FillRectangle(brush, x + xDiff, y + yDiff, blockSize, blockSize);
                    g.DrawRectangle(pen, x + xDiff, y + yDiff, blockSize, blockSize);
                }
                // Draw the shadow center
                var centerX = startX + (block.RotateCenter.x) * blockSize + xDiff;
                var centerY = startY + (block.RotateCenter.y) * blockSize + yDiff;
                var radius = blockSize * 0.125f;
                g.FillEllipse(centerBrush, centerX - radius, centerY - radius, radius * 2, radius * 2);

            }
        }

        private void DrawBeEatProgress(Graphics g)
        {
            var radius = _blockSize * 1.02f;    // Ring radius
            var thickness = _blockSize * 0.54f;  // Ring thickness

            var startX = (ClientSize.Width + _Columns * _blockSize + Padding.Right) * 0.5f;
            var startY = (ClientSize.Height - _Rows * _blockSize + Padding.Top) * 0.5f + _blockSize * 14.9f;
            startX += _blockSize * 0.672f * 2.28f - radius;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw the background ring
            g.DrawEllipse(new Pen(Color.FromArgb(64, ForegroundColor), thickness), startX, startY, radius * 2, radius * 2);

            // Draw the progress ring
            float sweepAngle = 360 * AutoEatProgressPercent();
            using var pen = new Pen(UsingDark ? Color.Peru : Color.Blue, thickness);
            g.DrawArc(pen, startX, startY, radius * 2, radius * 2, -90, sweepAngle);
        }

        private Color GetColor(Block.E_BlockType type) => UsingDark ? (type switch
        {
            // Select the color based on the block type for light mode
            Block.E_BlockType.I => Color.FromArgb(121, 184, 169), // 青绿
            Block.E_BlockType.O => Color.FromArgb(212, 177, 72),  // 黄色
            Block.E_BlockType.T => Color.FromArgb(172, 69, 165),  // 紫红色
            Block.E_BlockType.J => Color.FromArgb(81, 120, 220),  // 蓝色
            Block.E_BlockType.L => Color.FromArgb(220, 130, 70),  // 橙红色
            Block.E_BlockType.S => Color.FromArgb(52, 139, 43),   // 深绿色
            Block.E_BlockType.Z => Color.FromArgb(201, 70, 90),   // 红色
            _ => BackgroundColor,
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
            _ => BackgroundColor,
        });
    }
}
