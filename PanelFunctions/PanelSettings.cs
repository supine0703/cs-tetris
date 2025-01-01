namespace Tetris
{
    internal partial class Panel
    {
        private CustonButton _themeButton = new();
        private CustonButton _pauseButton = new();
        private CustonButton _restartButton = new();
        private Label _messageLabel = new();
        private float _massageLabelRatio = 1.0f;

        private void InitializeSettings()
        {
            // Initialize the message label
            _messageLabel.Hide();
            _messageLabel.TextAlign = ContentAlignment.MiddleCenter;
            _messageLabel.TabStop = false;
            _messageLabel.BackColor = Color.Transparent;
            _messageLabel.ForeColor = UsingDark ? Color.White : Color.Black;
            this.Controls.Add(_messageLabel);

            // Initialize the theme button
            _themeButton.Text = UsingDark ? "Dark" : "Light";
            _themeButton.Click += (sender, e) => ThemeButtonClick();
            this.Controls.Add(_themeButton);

            // Initialize the pause button
            _pauseButton.Text = Pause ? "\ue615" : "\ue616";
            _pauseButton.UseCompatibleTextRendering = true;
            _pauseButton.Click += (sender, e) => PauseButtonClick();
            this.Controls.Add(_pauseButton);

            // Initialize the restart button
            _restartButton.Text = "\ue617";
            _restartButton.UseCompatibleTextRendering = true;
            _restartButton.Click += (sender, e) => RestartButtonClick();
            this.Controls.Add(_restartButton);
        }

        private void ResizeSettings()
        {
            // Define the block size for the preview, and the start position
            Size boardSize = new(_Columns * _blockSize, _Rows * _blockSize);
            var leftX = (ClientSize.Width - boardSize.Width - Padding.Left) * 0.5f;
            var leftY = (ClientSize.Height - boardSize.Height + Padding.Top) * 0.5f;
            var rightX = (ClientSize.Width + boardSize.Width + Padding.Right) * 0.5f;
            var rightY = (ClientSize.Height - boardSize.Height + Padding.Top) * 0.5f;
            var blockSize = _blockSize * 0.672f;
            var width = blockSize * 4.56f;
            var height = blockSize * 4.28f;

            // Resize the pause button
            {
                var s = (int)(blockSize * 1.92f);
                _pauseButton.Size = new((int)s, (int)s);
                _pauseButton.Location = new((int)rightX, (int)rightY - s);
                _pauseButton.Font = new Font(_pfc.Families[0], blockSize * 0.593f + 1);
            }

            // Resize the restart button
            {
                var s = (int)(blockSize * 1.92f);
                _restartButton.Size = new(s, s);
                _restartButton.Location = new((int)leftX - s, (int)leftY - s);
                _restartButton.Font = new Font(_pfc.Families[0], blockSize * 0.593f + 1);
            }

            // Resize the message label
            {
                _messageLabel.Size = new(boardSize.Width, boardSize.Height / 3);
                _messageLabel.Location = new((int)(leftX + Padding.Left * 0.5f), (int)(leftY) + boardSize.Height / 5);
                RefushMessageFontSize();
            }

            // Resize the theme button
            _themeButton.Size = new((int)width, (int)blockSize * 2);
            _themeButton.Location = new((int)(leftX - width), (int)(leftY + height + blockSize));
            _themeButton.Font = new Font(_themeButton.Font.FontFamily, blockSize * 0.47f + 1, _themeButton.Font.Style);
        }
        private void RefushMessageFontSize(float ratio) { _massageLabelRatio = ratio; RefushMessageFontSize(); }

        private void RefushMessageFontSize()
        {
            _messageLabel.Font = new Font(_themeButton.Font.FontFamily, (_blockSize * 0.612f + 1) * _massageLabelRatio, _themeButton.Font.Style);
        }

        private void ThemeButtonClick()
        {
            UsingDark = !UsingDark;
            _themeButton.Text = UsingDark ? "Dark" : "Light";
            _themeButton.UsingDark = UsingDark;
            _pauseButton.UsingDark = UsingDark;
            _restartButton.UsingDark = UsingDark;
            _messageLabel.ForeColor = UsingDark ? Color.White : Color.Black;
            this.Invalidate();
        }

        private void PauseButtonClick()
        {
            Pause = !Pause;
            _pauseButton.Text = Pause ? "\ue615" : "\ue616";
            if (Pause)
            {
                _massageLabelRatio = GameOver ? 1.2f : 2f;
                _messageLabel.Text = GameOver ? "Game Over" : "Pause";
                _messageLabel.Show();
                RefushMessageFontSize();
            }
            else
            {
                _messageLabel.Hide();
            }
            this.Invalidate();
        }

        private void RestartButtonClick() { Restart(); }
    }
}
