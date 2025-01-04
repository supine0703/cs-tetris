using System.Diagnostics;
using System.Drawing.Drawing2D;
using static System.Windows.Forms.AxHost;

namespace Tetris
{
    internal partial class GameControl
    {
        private Color[][] _backgroundColors = null!;
        private Color[] _backColors = null!;
        private Color[] _foreColors = null!;

        private Point PlayfieldLocation { get; set; }

        private List<(int yi, List<int> cols)> _redrawRegion = null!;

        private int BlockBorderDiff => _bitmapManager.BlockBorderThickness / 2;
        private int XiToPosX(int xi) => PlayfieldLocation.X + xi * CellSize;
        private int YiToPosY(int yi) => PlayfieldLocation.Y + yi * CellSize;

        private void InitializeGamePaint()
        {
            _backgroundColors = [
                [Color.FromArgb(0, 120, 80), Color.FromArgb(139, 0, 87)],
                [Color.FromArgb(144, 238, 144), Color.FromArgb(255, 182, 193)]
            ];
            _backColors = [Color.Black, Color.FromArgb(248, 247, 247)];
            _foreColors = [Color.White, Color.Black];
        }

        private void CalculateRedrawRegion(Rectangle invalidateRect)
        {
            // Get all the cells
            _redrawRegion = Enumerable.Range(0, _playfieldInfo.Count).Select(
                row => (row - _BufferZoneRows, Enumerable.Range(0, _playfieldInfo[row].Length).ToList())
            ).ToList();
            if (invalidateRect == this.ClientRectangle) { return; }

            // Calculate the cell region that need to redraw
            _redrawRegion.RemoveAll(line =>
            {
                var posY = YiToPosY(line.yi);
                line.cols.RemoveAll(xi => !invalidateRect.IntersectsWith(new(XiToPosX(xi), posY, CellSize, CellSize)));
                return line.cols.Count == 0;
            });
        }

        private void DrawBackground(PaintEventArgs e)
        {
            using LinearGradientBrush brush = new(
                this.ClientRectangle,
                _backgroundColors[ThemeIndex][0], _backgroundColors[ThemeIndex][1],
                270f
            );
            e.Graphics.FillRectangle(brush, e.ClipRectangle);
        }

        private void DrawPlayfield(PaintEventArgs e)
        {
            int penWidth = 4;
            var pos = new Point(PlayfieldLocation.X - penWidth / 2, PlayfieldLocation.Y - penWidth / 2);
            Rectangle rect = new(
                pos, new Size(_MatrixColumns * CellSize + penWidth, _MatrixRows * CellSize + penWidth)
            );

            if (!e.ClipRectangle.IntersectsWith(rect)) { return; }

            //Debug.WriteLine("DrawPlayfield: " + rect);
            using (Pen outerPen = new(_foreColors[ThemeIndex], penWidth)) // Outer border pen with .. color
            using (Pen innerPen = new(Color.FromArgb(128, 128, 128, 128))) // Inner border pen with gray semi-transparency
            using (SolidBrush backBrush = new(_backColors[ThemeIndex])) // Background brush with white color
            {
                var g = e.Graphics;

                // Fill the background of the playfield
                g.FillRectangle(backBrush, rect);

                // Draw outer border (around the entire playfield)
                g.DrawRectangle(outerPen, rect);

                _redrawRegion.ForEach(line =>
                {
                    var row = line.yi + _BufferZoneRows;
                    var posY = YiToPosY(line.yi);
                    line.cols.ForEach(col =>
                    {
                        var posX = XiToPosX(col);
                        // Fill the block if it has one
                        if (_playfieldInfo[row][col].State == E_CellState.Occupied)
                        {
                            g.DrawImage(
                                _bitmapManager.GetBlockBitmap(_playfieldInfo[row][col].BelongsTo), 
                                posX - BlockBorderDiff, posY - BlockBorderDiff
                            );
                        }
                        // Draw inner border
                        else if (row >= _BufferZoneRows)
                        {
                            g.DrawRectangle(innerPen, posX, posY, CellSize, CellSize);
                        }
                    });
                });
            }
        }
    }
}
