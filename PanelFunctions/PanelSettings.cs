using System.Diagnostics;

namespace Tetris
{
    internal partial class Panel
    {
        private readonly CustonButton _themeButton = new();
        private readonly CustonButton _pauseButton = new();
        private readonly CustonButton _restartButton = new();
        private readonly Label _messageLabel = new();
        private readonly Label _watchLabel = new();
        private readonly Label _lineClearLabel = new();
        private readonly Label _totalScoreLabel = new();
        private float MassageLabelRatio { get; set; } = 1.0f;

        private readonly Label _autoEatLabel = new();
        private readonly CustonButton _autoEatButton = new();

        private readonly Label _autoDropLabel = new();
        private readonly CustonButton _autoDropButton = new();

        private readonly CustomLabel _howToPlayLabel_1 = new();
        private readonly CustomLabel _howToPlayLabel_2 = new();

        private void InitializeSettings()
        {
            // Initialize the watch label
            UniformSetLabel(_watchLabel);
            this.Controls.Add(_watchLabel);

            // Initialize the line clear label and total score label
            UniformSetLabel(_lineClearLabel);
            this.Controls.Add(_lineClearLabel);
            UniformSetLabel(_totalScoreLabel);
            this.Controls.Add(_totalScoreLabel);

            // Initialize the message label
            _messageLabel.Hide();
            UniformSetLabel(_messageLabel);
            this.Controls.Add(_messageLabel);

            // Initialize the theme button
            _themeButton.Click += (sender, e) => ThemeButtonClick();
            _themeButton.Font = new(_pfc.Families[1], 1);
            _themeButton.UseCompatibleTextRendering = true;
            this.Controls.Add(_themeButton);

            // Initialize the pause button
            _pauseButton.Text = Pause ? "\ue615" : "\ue616";
            _pauseButton.Font = new Font(_pfc.Families[0], 1);
            _pauseButton.UseCompatibleTextRendering = true;
            _pauseButton.Click += (sender, e) => PauseButtonClick();
            this.Controls.Add(_pauseButton);

            // Initialize the restart button
            _restartButton.Text = "\ue617";
            _restartButton.Font = new Font(_pfc.Families[0], 1);
            _restartButton.UseCompatibleTextRendering = true;
            _restartButton.Click += (sender, e) => RestartButtonClick();
            this.Controls.Add(_restartButton);

            // Initialize the how to play label
            InitHowToPlayLabel();

            // Initialize the auto eat label, button, ...
            UniformSetLabel(_autoEatLabel);
            _autoEatLabel.Text = "Auto Lock";
            _autoEatLabel.Font = new(_pfc.Families[1], 1);
            _autoEatLabel.UseCompatibleTextRendering = true;
            this.Controls.Add(_autoEatLabel);
            _autoEatButton.Text = $"{(AutoEatEnabled ? "On" : "Off")}";
            _autoEatButton.Click += (sender, e) => AutoEatButtonClick();
            this.Controls.Add(_autoEatButton);

            // Initialize the auto drop label, button, ...
            UniformSetLabel(_autoDropLabel);
            _autoDropLabel.Text = "Auto Drop";
            _autoDropLabel.Font = new(_pfc.Families[1], 1);
            _autoDropLabel.UseCompatibleTextRendering = true;
            this.Controls.Add(_autoDropLabel);
            _autoDropButton.Text = $"{(AutoEatEnabled ? "On" : "Off")}";
            _autoDropButton.Click += (sender, e) => AutoDropButtonClick();
            this.Controls.Add(_autoDropButton);

            // Initialize some ...
            ThemeButtonClick();
            ThemeButtonClick();

            // Test ... TODO ...
            _autoEatLabel.Hide();
            _autoEatButton.Hide();
            _autoDropButton.Click += (sender, e) => AutoEatButtonClick();
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

            // Resize the message label (center)
            {
                _messageLabel.Size = new(boardSize.Width, boardSize.Height / 3);
                _messageLabel.Location = new((int)(leftX + Padding.Left * 0.5f), (int)(leftY) + boardSize.Height / 5);
                RefushMessageFontSize();
            }

            // Resize the pause button (right)
            {
                var s = (int)(blockSize * 1.92f);
                _pauseButton.Size = new((int)s, (int)s);
                _pauseButton.Location = new((int)rightX, (int)rightY - s);
                _pauseButton.Font = new Font(_pauseButton.Font.FontFamily, blockSize * 0.593f + 1, _pauseButton.Font.Style);
            }

            // Resize the restart button (left)
            {
                var s = (int)(blockSize * 1.92f);
                _restartButton.Size = new(s, s);
                _restartButton.Location = new((int)leftX - s, (int)leftY - s);
                _restartButton.Font = new Font(_restartButton.Font.FontFamily, blockSize * 0.593f + 1, _restartButton.Font.Style);
            }

            // Resize the theme button (left)
            {
                _themeButton.Size = new((int)width, (int)blockSize * 2);
                _themeButton.Location = new((int)(leftX - width), (int)(leftY + height + blockSize));
                _themeButton.Font = new Font(_themeButton.Font.FontFamily, blockSize * 0.47f + 1, _themeButton.Font.Style);
            }

            // Resize the how to play label (right)
            {
                _howToPlayLabel_1.Size = new((int)(width * 0.72), (int)(blockSize * 8));
                _howToPlayLabel_1.Location = new((int)(leftX - width), (int)(leftY + height + blockSize * 4));
                _howToPlayLabel_1.Font = new(_howToPlayLabel_1.Font.FontFamily, blockSize * 0.211f + 1, _howToPlayLabel_1.Font.Style);
                _howToPlayLabel_2.Size = new((int)(width * 0.42), (int)(blockSize * 8));
                _howToPlayLabel_2.Location = new((int)(leftX - width * 0.28), (int)(leftY + height + blockSize * 4));
                _howToPlayLabel_2.Font = new(_howToPlayLabel_2.Font.FontFamily, blockSize * 0.211f + 1, _howToPlayLabel_2.Font.Style);
            }

            // Resize the auto eat label, button, ... (left)
            {
                Size s = new((int)width, (int)(blockSize * 1.5f));
                Point l = new((int)(leftX - width), (int)(leftY + height + blockSize * 13));

                _autoEatLabel.Size = s;
                _autoEatLabel.Location = l;
                _autoEatLabel.Font = new(_autoEatLabel.Font.FontFamily, blockSize * 0.3376f + 1, _autoEatLabel.Font.Style);
                l.Offset(0, (int)(blockSize * 1.5f));
                _autoEatButton.Size = s;
                _autoEatButton.Location = l;
                _autoEatButton.Font = new(_autoEatButton.Font.FontFamily, blockSize * 0.42f + 1, _autoEatButton.Font.Style);
            }

            // Resize the auto drop label, button, ... (left)
            {
                Size s = new((int)width, (int)(blockSize * 1.5f));
                Point l = new((int)(leftX - width), (int)(leftY + height + blockSize * 17.3f));
                _autoDropLabel.Size = s;
                _autoDropLabel.Location = l;
                _autoDropLabel.Font = new(_autoDropLabel.Font.FontFamily, blockSize * 0.3301f + 1, _autoDropLabel.Font.Style);
                l.Offset(0, (int)(blockSize * 1.5f));
                _autoDropButton.Size = s;
                _autoDropButton.Location = l;
                _autoDropButton.Font = new(_autoDropButton.Font.FontFamily, blockSize * 0.42f + 1, _autoDropButton.Font.Style);
            }

            // Resize the line clear label and total score labe (left)
            {
                _lineClearLabel.Size = new((int)width, (int)blockSize * 2);
                _lineClearLabel.Location = new((int)(leftX - width), (int)(leftY + height + blockSize * 19.3f));
                _lineClearLabel.Font = new(_lineClearLabel.Font.FontFamily, blockSize * 0.228f + 1, _lineClearLabel.Font.Style);

                _totalScoreLabel.Size = new((int)width, (int)blockSize * 2);
                _totalScoreLabel.Location = new((int)(leftX - width), (int)(leftY + height + blockSize * 21.3f));
                _totalScoreLabel.Font = new(_totalScoreLabel.Font.FontFamily, blockSize * 0.228f + 1, _totalScoreLabel.Font.Style);
            }

            // Resize the watch label (left)
            {
                _watchLabel.Size = new((int)width, (int)blockSize * 2);
                _watchLabel.Location = new((int)(leftX - width), (int)(leftY + height + blockSize * 22));
                _watchLabel.Font = new(_watchLabel.Font.FontFamily, (_blockSize * 0.228f + 1), _watchLabel.Font.Style);
            }
        }

        private void UniformSetLabel(Label label)
        {
            if (label == null)
            {
                return;
            }
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.TabStop = false;
            label.BackColor = Color.Transparent;
            label.Font = new(_pfc.Families[1], 1);

            label.UseCompatibleTextRendering = true;
        }

        private void RefushWatchLabel()
        {
            TimeSpan elapsed = _stopwatch.Elapsed;

            if (elapsed.TotalSeconds < 10)
            {
                _watchLabel.Text = elapsed.ToString("\\ s\\.fff\\\"");
            }
            else if (elapsed.TotalMinutes < 1)
            {
                _watchLabel.Text = elapsed.ToString("ss\\.fff\\\"");
            }
            else if (elapsed.TotalMinutes < 10)
            {
                _watchLabel.Text = elapsed.ToString("m\\'ss\\.ff\\\"");
            }
            else if (elapsed.TotalHours < 1)
            {
                _watchLabel.Text = elapsed.ToString("mm\\'ss\\.f\\\"");
            }
            else if (elapsed.TotalHours < 10)
            {
                _watchLabel.Text = elapsed.ToString("h\\:mm\\'ss\\\"");
            }
            else
            {
                _watchLabel.Text = elapsed.ToString("hh\\:mm\\'ss\\'");
            }
        }

        private void RefushLineClearLabel()
        {
            _lineClearLabel.Text = $"Line Clear: {_lineClearCount}";
        }

        private void RefushMessageFontSize()
        {
            _messageLabel.Font = new(
                _messageLabel.Font.FontFamily,
                (_blockSize * 0.612f + 1) * MassageLabelRatio,
                _messageLabel.Font.Style
            );
        }

        private void ThemeButtonClick()
        {
            UsingDark = !UsingDark;
            _themeButton.Text = UsingDark ? "Dark" : "Light";
            _themeButton.UsingDark = UsingDark;
            _pauseButton.UsingDark = UsingDark;
            _restartButton.UsingDark = UsingDark;
            _messageLabel.ForeColor = UsingDark ? Color.FromArgb(255, 223, 100) : Color.Black;
            _watchLabel.ForeColor = UsingDark ? Color.White : Color.Black;
            _howToPlayLabel_1.UsingDark = UsingDark;
            _howToPlayLabel_2.UsingDark = UsingDark;

            _autoEatLabel.ForeColor = UsingDark ? Color.White : Color.Black;
            _autoEatButton.UsingDark = UsingDark;
            _autoDropLabel.ForeColor = UsingDark ? Color.White : Color.Black;
            _autoDropButton.UsingDark = UsingDark;

            this.Invalidate();
        }

        private void PauseButtonClick()
        {
            Pause = !Pause;
            _pauseButton.Text = Pause ? "\ue615" : "\ue616";
            if (Pause)
            {
                _stopwatch.Stop();
                MassageLabelRatio = GameOver ? 1.2f : 2f;
                _messageLabel.Text = GameOver ? "Game Over" : "Pause";
                _messageLabel.Show();
                RefushMessageFontSize();
            }
            else
            {
                _stopwatch.Start();
                _messageLabel.Hide();
            }
            this.Invalidate();
        }

        private void RestartButtonClick() { Restart(); }

        private void AutoEatButtonClick()
        {
            AutoEatEnabled = !AutoEatEnabled;
            _autoEatButton.Text = $"{(AutoEatEnabled ? "On" : "Off")}";
            this.Invalidate();
        }

        private void AutoDropButtonClick()
        {
            AutoDropEnabled = !AutoDropEnabled;
            _autoDropButton.Text = $"{(AutoDropEnabled ? "On" : "Off")}";
            this.Invalidate();
        }

        private void InitHowToPlayLabel()
        {
            UniformSetLabel(_howToPlayLabel_1);
            _howToPlayLabel_1.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(_howToPlayLabel_1);
            UniformSetLabel(_howToPlayLabel_2);
            _howToPlayLabel_2.TextAlign = ContentAlignment.MiddleLeft;
            _howToPlayLabel_2.ForeColor1 = Color.FromArgb(255, 223, 100);
            _howToPlayLabel_2.ForeColor2 = Color.Blue;
            this.Controls.Add(_howToPlayLabel_2);

            List<(string, string)> values = [
                ("Pause",       "P,Esc"),
                ("Restart",     "R"),
                ("Theme",       "T"),
                ("L Move",      "A, ←"),
                ("R Move",      "D, →"),
                ("Soft Drop",   "S, ↓"),
                ("Hard Drop",   "W, ↑"),
                ("L Rotate",    "J, X"),
                ("R Rotate",    "K, C"),
                ("Flip 180°",   "L, V"),
                ("Hold",        "Space")
            ];

            foreach (var (v1, v2) in values)
            {
                _howToPlayLabel_1.Text += $"{v1}\n";
                _howToPlayLabel_2.Text += $"{v2}\n";
            }
        }
    }
}
