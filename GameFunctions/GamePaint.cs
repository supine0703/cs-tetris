using System.Drawing.Drawing2D;

namespace Tetris
{
    internal partial class GameControl
    {
        private const float _PreloadPieceSizeScaling = 0.672f;
        private const float _PieceBoxPaddingRatio = 0.28f;

        private Color[][] _backgroundColors = null!;
        private Color[] _backColors = null!;
        private Color[] _foreColors = null!;
        private Color[] _pauseBackColors = null!;
        private Color[] _progressColors = null!;

        private Point PlayfieldLocation { get; set; }
        private Size PieceBoundingBoxSize { get; set; }
        private Rectangle PreviewQueueBoxRect => new(
            XiToPosX(_MatrixColumns) + Padding.Right / 2,
            YiToPosY(0.4f) + Padding.Bottom / 2,
            (int)(CellSize * _PreloadPieceSizeScaling * (4 + _PieceBoxPaddingRatio * 2)),
            (int)(CellSize * _PreloadPieceSizeScaling * (3 + _PieceBoxPaddingRatio / 2) * _PreviewPieceCount)
        );
        private Rectangle HoldQueueBoxRect
        {
            get
            {
                var cellSize = CellSize * _PreloadPieceSizeScaling;
                var width = (int)(cellSize * (4 + _PieceBoxPaddingRatio * 2));
                return new(
                    XiToPosX(0) - Padding.Left / 2 - width, YiToPosY(0.4f) + Padding.Bottom / 2,
                    width, (int)(cellSize * (3 + _PieceBoxPaddingRatio * 2))
                );
            }
        }
        private Rectangle WaitLockProgressRect
        {
            get
            {
                var rect = PreviewQueueBoxRect;
                rect.Y += rect.Height + Padding.Top + Padding.Bottom / 2;
                rect.Height = rect.Width;
                return rect;
            }
        }

        private Piece? _pieceStateBcakup;
        private Piece? _shadowStateBcakup;

        private List<(int yi, List<int> cols)> _redrawRegion = null!;

        private int BlockBorderDiff => BorderThickness / 2;
        private int XiToPosX(float xi) => (int)(PlayfieldLocation.X + xi * CellSize);
        private int YiToPosY(float yi) => (int)(PlayfieldLocation.Y + yi * CellSize);
        private int ColToPosX(int col) => XiToPosX(col);
        private int RowToPosY(int row) => YiToPosY(row - _BufferZoneRows);

        private Rectangle GetPieceBoundingBox(Piece piece)
        {
            return new(
                XiToPosX(piece.Pos.X) - BlockBorderDiff - BorderThickness,
                YiToPosY(piece.Pos.Y) - BlockBorderDiff - BorderThickness,
                PieceBoundingBoxSize.Width + BorderThickness * 2,
                PieceBoundingBoxSize.Height + BorderThickness * 2
            );
        }

        private void RecordPieceState()
        {
            if (_pieceStateBcakup is null && _activePiece is null) { return; }
            if (_pieceStateBcakup?.Equals(_activePiece) ?? false) { return; }

            if (_pieceStateBcakup is not null)
            {
                FrameInvalidate(GetPieceBoundingBox(_pieceStateBcakup));
                FrameInvalidate(GetPieceBoundingBox(_shadowStateBcakup!));
            }
            if (_activePiece is not null)
            {
                FrameInvalidate(GetPieceBoundingBox(_activePiece));
                _pieceStateBcakup = new(_activePiece);
                _shadowStateBcakup = new(_activePiece);
                DropBottom(_shadowStateBcakup);
                FrameInvalidate(GetPieceBoundingBox(_shadowStateBcakup));
            }
            else
            {
                _pieceStateBcakup = null;
                _shadowStateBcakup = null;
            }
        }

        private void InitializeGamePaint()
        {
            _backgroundColors = [
                [Color.FromArgb(0, 120, 80), Color.FromArgb(139, 0, 87)],
                [Color.FromArgb(144, 238, 144), Color.FromArgb(255, 182, 193)]
            ];
            _backColors = [Color.Black, Color.FromArgb(248, 247, 247)];
            _foreColors = [Color.White, Color.Black];
            _pauseBackColors = [Color.FromArgb(128, Color.Black), Color.FromArgb(128, 222, 222, 222)];
            _progressColors = [Color.Peru, Color.Blue];
            FrameTickLoadList?.Add((1, (sender, e) => RecordPieceState()));
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
            var penWidth = BorderThickness * 2;
            var pos = new Point(PlayfieldLocation.X - penWidth / 2, PlayfieldLocation.Y - penWidth / 2);
            Rectangle rect = new(
                pos, new Size(_MatrixColumns * CellSize + penWidth, _MatrixRows * CellSize + penWidth)
            );

            if (!e.ClipRectangle.IntersectsWith(rect)) { return; }

            var g = e.Graphics;

            using (Pen outerPen = new(_foreColors[ThemeIndex], penWidth)) // Outer border pen with .. color
            using (SolidBrush backBrush = new(_backColors[ThemeIndex])) // Background brush with white color
            {
                // Fill the background of the playfield
                g.FillRectangle(backBrush, rect);
                // Draw outer border (around the entire playfield)
                g.DrawRectangle(outerPen, rect);
            }
            using Pen innerPen = new(Color.FromArgb(128, 128, 128, 128)); // Inner border pen with gray semi-transparency
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

        private void DrawActivePiece(PaintEventArgs e)
        {
            if (_pieceStateBcakup == null) { return; }

            var g = e.Graphics;

            // Draw the shadow of the piece
            using var bitmap = BitmapManager.CreatePieceBitmap(
                _bitmapManager.Theme, _pieceStateBcakup, CellSize, BorderThickness
            );
            using (var shadowBitmap = BitmapManager.FromAlphaBitmap(64, bitmap))
            {
                g.DrawImage(
                    shadowBitmap,
                    XiToPosX(_shadowStateBcakup!.Pos.X) - BlockBorderDiff,
                    YiToPosY(_shadowStateBcakup!.Pos.Y) - BlockBorderDiff
                );
            }

            // Draw the active piece
            g.DrawImage(
                bitmap,
                XiToPosX(_pieceStateBcakup.Pos.X) - BlockBorderDiff,
                YiToPosY(_pieceStateBcakup.Pos.Y) - BlockBorderDiff
            );
        }

        private void DrawPreviewQueue(PaintEventArgs e)
        {
            var rect = PreviewQueueBoxRect;
            if (!e.ClipRectangle.IntersectsWith(rect)) { return; }

            var g = e.Graphics;
            var penWidth = BorderThickness;

            // Draw the preview border
            using (Pen previewPen = new(_foreColors[ThemeIndex], penWidth))
            using (SolidBrush backBrush = new(Color.FromArgb(64, _backColors[ThemeIndex])))
            {
                g.FillRectangle(backBrush, rect);
                g.DrawRectangle(previewPen, rect);
            }

            var cellSize = CellSize * _PreloadPieceSizeScaling;
            var startX = rect.X + cellSize * _PieceBoxPaddingRatio;
            var startY = rect.Y - cellSize * _PieceBoxPaddingRatio;

            // Draw the preview blocks
            foreach (var piece in _previewQueue)
            {
                // Define the offset for the piece center
                float xDiff = 0;
                float yDiff = 0;
                if (piece.Type == Piece.E_Type.I) { yDiff = cellSize * 0.5f; }
                else if (piece.Type != Piece.E_Type.O) { xDiff = cellSize * 0.5f; }

                // Draw the piece
                g.DrawImage(_bitmapManager.GetPieceBitmap(piece.Type), startX + xDiff, startY + yDiff);

                // Move the starting point to the next piece
                startY += cellSize * 3;
            }
        }

        private void DrawHoldQueue(PaintEventArgs e)
        {
            var rect = HoldQueueBoxRect;
            if (!e.ClipRectangle.IntersectsWith(rect)) { return; }

            var g = e.Graphics;
            var penWidth = BorderThickness;

            // Draw the hold box border
            using (Pen holdPen = new(_foreColors[ThemeIndex], penWidth))
            using (SolidBrush backBrush = new(Color.FromArgb(64, _backColors[ThemeIndex])))
            {
                g.FillRectangle(backBrush, rect);
                g.DrawRectangle(holdPen, rect);
            }

            var cellSize = CellSize * _PreloadPieceSizeScaling;
            var startX = rect.X + cellSize * _PieceBoxPaddingRatio;
            var startY = rect.Y - cellSize * _PieceBoxPaddingRatio;

            // Draw the hold piece
            foreach (var piece in _holdQueue)
            {
                // Define the offset for the piece center
                float xDiff = 0;
                float yDiff = 0;
                if (piece.Type == Piece.E_Type.I) { yDiff = cellSize * 0.5f; }
                else if (piece.Type != Piece.E_Type.O) { xDiff = cellSize * 0.5f; }
                // Draw the piece
                g.DrawImage(_bitmapManager.GetPieceBitmap(piece.Type), startX + xDiff, startY + yDiff);
            }

            if (!CanHold)
            {
                using SolidBrush backBrush = new(Color.FromArgb(128, _backColors[ThemeIndex]));
                g.FillRectangle(backBrush, rect);
            }
        }

        private void DrawWaitLockProgress(PaintEventArgs e)
        {
            var rect = WaitLockProgressRect;
            var thickness = rect.Width * 0.18f;  // Ring thickness
            var radius = rect.Width * 0.36f;    // Ring radius
            rect.X += (int)(rect.Width * 0.5f - radius);
            rect.Y += (int)(thickness / 2);

            // Draw the background ring
            using (Pen pen = new(new SolidBrush(Color.FromArgb(64, _foreColors[ThemeIndex])), thickness))
            {
                e.Graphics.DrawEllipse(pen, rect.X, rect.Y, radius * 2, radius * 2);
            }

            // Draw the progress ring
            var sweepAngle = 360 * LockDelayProgress;
            using (Pen pen = new(_progressColors[ThemeIndex], thickness))
            {
                e.Graphics.DrawArc(pen, rect.X, rect.Y, radius * 2, radius * 2, -90, sweepAngle);
            }
        }

        private void DrawPauseBackground(Graphics g)
        {
            if (GameState == E_GameState.Paused || GameState == E_GameState.GameOver)
            {
                using SolidBrush brush = new(_pauseBackColors[ThemeIndex]);
                g.FillRectangle(brush, ClientRectangle);
            }
        }
    }
}
