using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Reflection;

namespace Tetris
{
    internal partial class GameControl
    {
        // For the game font
        private readonly List<string> _fontResources = ["iconfont", "JetBrainsMono",];
        private PrivateFontCollection _pfc = new();
        private readonly object _fontLoadLock = new(); // Maybe load font exception if not locked, unknown reason

        private void InitializeFontResources()
        {
            lock (_fontLoadLock)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();

                foreach (var value in _fontResources)
                {
                    var name = resourceNames.FirstOrDefault(n => n.Contains($"{value}"))
                        ?? throw new Exception($"Cannot find the {value} font resource");

                    using var stream = assembly.GetManifestResourceStream(name)
                        ?? throw new Exception($"Failed to load the font resource: {name}");

                    var data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);

                    unsafe
                    {
                        fixed (byte* ptr = data)
                        {
                            _pfc.AddMemoryFont((IntPtr)ptr, data.Length);
                        }
                    }
                }

                //Debug.WriteLine($"Font count: {_pfc.Families.Length}");
                //foreach (var f in _pfc.Families)
                //{
                //    Debug.WriteLine(f.Name);
                //}
            }
        }

        // === === ===

        private readonly CustomButton _themeButton = new();
        private readonly CustomButton _pauseButton = new();
        private readonly CustomButton _restartButton = new();
        private readonly CustomButton _bgmButton = new();
        private readonly CustomLabel _fpsLabel = new();
        //private readonly Label _watchLabel = new();
        //private readonly Label _messageLabel = new();
        //private readonly Label _lineClearLabel = new();
        //private readonly Label _totalScoreLabel = new();
        //private float MassageLabelRatio { get; set; } = 1.0f;

        //private readonly Label _autoEatLabel = new();
        //private readonly CustomButton _autoEatButton = new();

        //private readonly Label _autoDropLabel = new();
        //private readonly CustomButton _autoDropButton = new();

        //private readonly CustomLabel _howToPlayLabel_1 = new();
        //private readonly CustomLabel _howToPlayLabel_2 = new();

        private void InitializeSettings()
        {
            // Initialize the restart button
            _restartButton.Text = "\ue617";
            _restartButton.Font = new Font(_pfc.Families[0], 1);
            _restartButton.Click += (sender, e) => RestartButtonClick();
            this.Controls.Add(_restartButton);

            // Initialize the pause button
            _pauseButton.Font = new Font(_pfc.Families[0], 1);
            _pauseButton.Click += (sender, e) => PauseButtonClick();
            GameStateChanged += PauseButtonSateChanged;
            this.Controls.Add(_pauseButton);
            _pauseButton.Hide();
            
            // Initialize the bgm button
            _bgmButton.Text = BgmMute ? "\ue619" : "\ue618";
            _bgmButton.Font = new Font(_pfc.Families[0], 1);
            _bgmButton.Click += (sender, e) => BgmButtonClick();
            this.Controls.Add(_bgmButton);

            // Initialize the theme button
            _themeButton.Click += (sender, e) => _bitmapManager.NextTheme();
            _themeButton.Font = new(_pfc.Families[1], 1);
            this.Controls.Add(_themeButton);

            // Initialize the fps label
            _fpsLabel.Font = new(_pfc.Families[1], 1);
            _fpsLabel.TextAlign = ContentAlignment.TopLeft;
            this.Controls.Add(_fpsLabel);

            // Initialize the watch label
            //UniformSetLabel(_watchLabel);
            //this.Controls.Add(_watchLabel);

            // Initialize the line clear label and total score label
            //UniformSetLabel(_lineClearLabel);
            //this.Controls.Add(_lineClearLabel);
            //UniformSetLabel(_totalScoreLabel);
            //this.Controls.Add(_totalScoreLabel);

            // Initialize the message label
            //_messageLabel.Hide();
            //UniformSetLabel(_messageLabel);
            //this.Controls.Add(_messageLabel);



            // Initialize the how to play label
            InitHowToPlayLabel();

            // Initialize the auto eat label, button, ...
            //UniformSetLabel(_autoEatLabel);
            //_autoEatLabel.Text = "Auto Lock";
            //_autoEatLabel.Font = new(_pfc.Families[1], 1);
            //_autoEatLabel.UseCompatibleTextRendering = true;
            //this.Controls.Add(_autoEatLabel);
            //_autoEatButton.Text = $"{(AutoEatEnabled ? "On" : "Off")}";
            //_autoEatButton.Font = new(_pfc.Families[1], 1);
            //_autoEatButton.UseCompatibleTextRendering = true;
            //_autoEatButton.Click += (sender, e) => AutoEatButtonClick();
            //this.Controls.Add(_autoEatButton);

            // Initialize the auto drop label, button, ...
            //UniformSetLabel(_autoDropLabel);
            //_autoDropLabel.Text = "Auto Drop";
            //_autoDropLabel.Font = new(_pfc.Families[1], 1);
            //_autoDropLabel.UseCompatibleTextRendering = true;
            //this.Controls.Add(_autoDropLabel);
            //_autoDropButton.Text = $"{(AutoEatEnabled ? "On" : "Off")}";
            //_autoDropButton.Font = new(_pfc.Families[1], 1);
            //_autoDropButton.UseCompatibleTextRendering = true;
            //_autoDropButton.Click += (sender, e) => AutoDropButtonClick();
            //this.Controls.Add(_autoDropButton);

            // Initialize some ...
            RefushTotalScore();

            // Test ... TODO ...
            //_autoEatLabel.Hide();
            //_autoEatButton.Hide();
            //_autoDropButton.Click += (sender, e) => AutoEatButtonClick();

            InitializeSettingsTheme();
            RefushFpsLabel();
        }

        private void InitializeSettingsTheme()
        {
            _bitmapManager.ThemeChanged += (sender, theme) =>
            {
                Color commonForeColor;
                Color commonBorderColor;
                Color commonHoverBackColor;
                Color commonPressedBackColor;
                Color strongForeColor;
                switch (theme)
                {
                    case BitmapManager.E_Theme.Dark:
                        _themeButton.Text = "Dark";
                        commonForeColor = Color.FromArgb(255, 255, 255, 255);
                        commonBorderColor = Color.FromArgb(242, 242, 242, 242);
                        commonHoverBackColor = Color.FromArgb(37, 0, 0, 0);
                        commonPressedBackColor = Color.FromArgb(94, 0, 0, 0);
                        strongForeColor = Color.FromArgb(255, 223, 100);
                        break;
                    case BitmapManager.E_Theme.Light:
                        _themeButton.Text = "Light";
                        commonForeColor = Color.FromArgb(255, 0, 0, 0);
                        commonBorderColor = Color.FromArgb(242, 12, 12, 12);
                        commonHoverBackColor = Color.FromArgb(37, 255, 255, 255);
                        commonPressedBackColor = Color.FromArgb(94, 255, 255, 255);
                        strongForeColor = Color.DarkBlue;
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(theme));
                }
                void SetButtonStyle(CustomButton button)
                {
                    button.ForeColor = commonForeColor;
                    button.BorderColor = commonBorderColor;
                    button.HoverBackColor = commonHoverBackColor;
                    button.PressedBackColor = commonPressedBackColor;
                }
                SetButtonStyle(_themeButton);
                SetButtonStyle(_pauseButton);
                SetButtonStyle(_restartButton);
                SetButtonStyle(_bgmButton);

                // other ...
                _fpsLabel.ForeColor = strongForeColor;
                _fpsLabel.Refresh();
            };
        }

        private void ResizeSettings()
        {
            // Define the block size for the preview, and the start position
            Size boardSize = new(_MatrixColumns * CellSize, _MatrixRows * CellSize);
            var leftX = (ClientSize.Width - boardSize.Width - Padding.Left) * 0.5f;
            var leftY = (ClientSize.Height - boardSize.Height + Padding.Top) * 0.5f;
            var rightX = (ClientSize.Width + boardSize.Width + Padding.Right) * 0.5f;
            var rightY = (ClientSize.Height - boardSize.Height + Padding.Top) * 0.5f;
            var blockSize = CellSize * 0.672f;
            var width = blockSize * 4.56f;
            var height = blockSize * 4.28f;

            // Resize the message label (center)
            {
                //_messageLabel.Size = new(boardSize.Width, boardSize.Height / 3);
                //_messageLabel.Location = new((int)(leftX + Padding.Left * 0.5f), (int)(leftY) + boardSize.Height / 5);
                RefushMessageFontSize();
            }

            // Resize the restart button (left)
            {
                var s = (int)(blockSize * 1.92f);
                _restartButton.Size = new(s, s);
                _restartButton.Location = new((int)leftX - s, (int)leftY - s);
                _restartButton.Font = new Font(_restartButton.Font.FontFamily, blockSize * 0.593f + 1, _restartButton.Font.Style);
            }

            // Resize the pause button (right)
            {
                var s = (int)(blockSize * 1.92f);
                _pauseButton.Size = new((int)s, (int)s);
                _pauseButton.Location = new((int)rightX, (int)rightY - s);
                _pauseButton.Font = new Font(_pauseButton.Font.FontFamily, blockSize * 0.593f + 1, _pauseButton.Font.Style);
            }

            // Resize the bgm button (right)
            {
                var s = (int)(blockSize * 1.92f);
                _bgmButton.Size = new(s, s);
                _bgmButton.Location = new((int)(rightX + s * 1.35f), (int)rightY - s);
                _bgmButton.Font = new Font(_bgmButton.Font.FontFamily, blockSize * 0.593f + 1, _bgmButton.Font.Style);
            }
            
            // Resize the theme button (left)
            {
                _themeButton.Size = new((int)width, (int)blockSize * 2);
                _themeButton.Location = new((int)(leftX - width), (int)(leftY + height + blockSize));
                _themeButton.Font = new Font(_themeButton.Font.FontFamily, blockSize * 0.47f + 1, _themeButton.Font.Style);
            }

            // Resize the fps label(left)
            {
                var s = (int)(blockSize * 1.92f);
                _fpsLabel.Size = new((int)width / 2, s);
                _fpsLabel.Location = new((int)(leftX - s * 2.5f), (int)leftY - s);
                _fpsLabel.Font = new(_fpsLabel.Font.FontFamily, (CellSize * 0.172f + 1), _fpsLabel.Font.Style);
            }

            // Resize border
            _restartButton.BorderThickness = (int)(BorderThickness * 1.5f);
            _pauseButton.BorderThickness = (int)(BorderThickness * 1.5f);
            _bgmButton.BorderThickness = (int)(BorderThickness * 1.5f);
            _themeButton.BorderThickness = (int)(BorderThickness * 1.5f);


            // Resize the how to play label (right)
            //{
            //    _howToPlayLabel_1.Size = new((int)(width * 0.72), (int)(blockSize * 8.8f));
            //    _howToPlayLabel_1.Location = new((int)(leftX - width), (int)(leftY + height + blockSize * 4));
            //    _howToPlayLabel_1.Font = new(_howToPlayLabel_1.Font.FontFamily, blockSize * 0.211f + 1, _howToPlayLabel_1.Font.Style);
            //    _howToPlayLabel_2.Size = new((int)(width * 0.42), (int)(blockSize * 8.8f));
            //    _howToPlayLabel_2.Location = new((int)(leftX - width * 0.28), (int)(leftY + height + blockSize * 4));
            //    _howToPlayLabel_2.Font = new(_howToPlayLabel_2.Font.FontFamily, blockSize * 0.211f + 1, _howToPlayLabel_2.Font.Style);
            //}

            // Resize the auto eat label, button, ... (left)
            //{
            //    Size s = new((int)width, (int)(blockSize * 1.5f));
            //    Point l = new((int)(leftX - width), (int)(leftY + height + blockSize * 12));

            //    _autoEatLabel.Size = s;
            //    _autoEatLabel.Location = l;
            //    _autoEatLabel.Font = new(_autoEatLabel.Font.FontFamily, blockSize * 0.3376f + 1, _autoEatLabel.Font.Style);
            //    l.Offset(0, (int)(blockSize * 1.5f));
            //    _autoEatButton.Size = s;
            //    _autoEatButton.Location = l;
            //    _autoEatButton.Font = new(_autoEatButton.Font.FontFamily, blockSize * 0.397f + 1, _autoEatButton.Font.Style);
            //}

            // Resize the auto drop label, button, ... (left)
            //{
            //    Size s = new((int)width, (int)(blockSize * 1.5f));
            //    Point l = new((int)(leftX - width), (int)(leftY + height + blockSize * 15.3f));
            //    _autoDropLabel.Size = s;
            //    _autoDropLabel.Location = l;
            //    _autoDropLabel.Font = new(_autoDropLabel.Font.FontFamily, blockSize * 0.3301f + 1, _autoDropLabel.Font.Style);
            //    l.Offset(0, (int)(blockSize * 1.5f));
            //    _autoDropButton.Size = s;
            //    _autoDropButton.Location = l;
            //    _autoDropButton.Font = new(_autoDropButton.Font.FontFamily, blockSize * 0.397f + 1, _autoDropButton.Font.Style);
            //}

            // Resize the line clear label and total score labe (left)
            //{
            //    _lineClearLabel.Size = new((int)width, (int)(blockSize * 1.6f));
            //    _lineClearLabel.Location = new((int)(leftX - width), (int)(leftY + height + blockSize * 20.515f));
            //    _lineClearLabel.Font = new(_lineClearLabel.Font.FontFamily, blockSize * 0.297f + 1, _lineClearLabel.Font.Style);

            //    _totalScoreLabel.Size = new((int)width, (int)(blockSize * 1.6f));
            //    _totalScoreLabel.Location = new((int)(leftX - width), (int)(leftY + height + blockSize * 19.333f));
            //    _totalScoreLabel.Font = new(_totalScoreLabel.Font.FontFamily, blockSize * 0.297f + 1, _totalScoreLabel.Font.Style);
            //}

            // Resize the watch label (left)
            //{
            //    _watchLabel.Size = new((int)width, (int)(blockSize * 1.6f));
            //    _watchLabel.Location = new((int)(leftX - width), (int)(leftY + height + blockSize * 22));
            //    _watchLabel.Font = new(_watchLabel.Font.FontFamily, (CellSize * 0.228f + 1), _watchLabel.Font.Style);
            //}
        }
        private void RestartButtonClick() { RestartGame(); }

        private void PauseButtonSateChanged(E_GameState state)
        {
            switch (state)
            {
                case E_GameState.Paused:
                    _pauseButton.Show();
                    _pauseButton.Text = "\ue615";
                    break;
                case E_GameState.Playing:
                    _pauseButton.Show();
                    _pauseButton.Text = "\ue616";
                    break;
                default:
                    _pauseButton.Hide();
                    break;
            }
            _pauseButton.Invalidate();
        }

        private void PauseButtonClick()
        {
            if (GameState == E_GameState.Paused)
            {
                GameState = E_GameState.Playing;
            }
            else if (GameState == E_GameState.Playing)
            {
                GameState = E_GameState.Paused;
            }

            //Pause = !Pause;
            //_pauseButton.Text = Pause ? "\ue615" : "\ue616";
            //if (Pause)
            //{
            //    _stopwatch.Stop();
            //    MassageLabelRatio = GameOver ? 1.2f : 2f;
            //    _messageLabel.Text = GameOver ? "Game Over" : "Pause";
            //    _messageLabel.Show();
            //    RefushMessageFontSize();
            //}
            //else
            //{
            //    _stopwatch.Start();
            //    _messageLabel.Hide();
            //}
            //this.Invalidate();
        }

        private void BgmButtonClick()
        {
            BgmMuteSwitch();
            _bgmButton.Text = BgmMute ? "\ue619" : "\ue618";
            _bgmButton.Invalidate();
        }

        private void RefushFpsLabel()
        {
            _frameTimer.DelayToDo(15, RefushFpsLabel);
            _fpsLabel.Text = $"{_frameTimer.Fps:F2}";
            _fpsLabel.Refresh();
        }

        private void RefushWatchLabel()
        {
            //var elapsed = _stopwatch.Elapsed;

            //if (elapsed.TotalSeconds < 10)
            //{
            //    _watchLabel.Text = elapsed.ToString("\\ s\\.fff\\\"");
            //}
            //else if (elapsed.TotalMinutes < 1)
            //{
            //    _watchLabel.Text = elapsed.ToString("ss\\.fff\\\"");
            //}
            //else if (elapsed.TotalMinutes < 10)
            //{
            //    _watchLabel.Text = elapsed.ToString("m\\'ss\\.ff\\\"");
            //}
            //else if (elapsed.TotalHours < 1)
            //{
            //    _watchLabel.Text = elapsed.ToString("mm\\'ss\\.f\\\"");
            //}
            //else if (elapsed.TotalHours < 10)
            //{
            //    _watchLabel.Text = elapsed.ToString("h\\:mm\\'ss\\\"");
            //}
            //else
            //{
            //    _watchLabel.Text = elapsed.ToString("hh\\:mm\\'ss\\'");
            //}
        }

        private void RefushTotalScore()
        {
            //_lineClearLabel.Text = $"Clear: {LineClearCount}";
            //_totalScoreLabel.Text = $"{TotalScore}";
        }

        private void RefushMessageFontSize()
        {
            //_messageLabel.Font = new(
            //    _messageLabel.Font.FontFamily,
            //    (CellSize * 0.612f + 1) * MassageLabelRatio,
            //    _messageLabel.Font.Style
            //);
        }

        
        private void AutoEatButtonClick()
        {
            //AutoEatEnabled = !AutoEatEnabled;
            //_autoEatButton.Text = $"{(AutoEatEnabled ? "On" : "Off")}";
            this.Invalidate();
        }

        private void AutoDropButtonClick()
        {
            //AutoDropEnabled = !AutoDropEnabled;
            //_autoDropButton.Text = $"{(AutoDropEnabled ? "On" : "Off")}";
            this.Invalidate();
        }

        private void InitHowToPlayLabel()
        {
            //UniformSetLabel(_howToPlayLabel_1);
            //_howToPlayLabel_1.TextAlign = ContentAlignment.MiddleLeft;
            //this.Controls.Add(_howToPlayLabel_1);
            //UniformSetLabel(_howToPlayLabel_2);
            //_howToPlayLabel_2.TextAlign = ContentAlignment.MiddleLeft;
            //_howToPlayLabel_2.ForeColor1 = Color.FromArgb(255, 223, 100);
            //_howToPlayLabel_2.ForeColor2 = Color.Blue;
            //this.Controls.Add(_howToPlayLabel_2);

            //List<(string, string)> values = [
            //    ("Pause",       "P,Esc"),
            //    ("Restart",     "R"),
            //    ("Theme",       "T"),
            //    ("BGM",         "B"),
            //    ("L Move",      "A, ←"),
            //    ("R Move",      "D, →"),
            //    ("Soft Drop",   "S, ↓"),
            //    ("Hard Drop",   "W, ↑"),
            //    ("L Rotate",    "J, X"),
            //    ("R Rotate",    "K, C"),
            //    ("Flip 180°",   "L, V"),
            //    ("Hold",        "Space")
            //];

            //foreach (var (v1, v2) in values)
            //{
            //    _howToPlayLabel_1.Text += $"{v1}\n";
            //    _howToPlayLabel_2.Text += $"{v2}\n";
            //}
            //}
        }
    }
}
