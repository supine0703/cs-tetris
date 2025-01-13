using System.Reflection;

namespace Tetris
{
    public static class Info
    {
        public static string Author => System.Text.Encoding.UTF8.GetString([0xe6, 0x9d, 0x8e, 0xe5, 0xae, 0x97, 0xe9, 0x9c, 0x96]);
        public static string Email => "supine0703@outlook.com";
        public static string Github => "https://github.com/supine0703";
        public static string Repository => $"{Github}/cs-tetris";
    }

    internal partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Controls.Add(new GameControl()
            {
                Dock = DockStyle.Fill, // Fill the entire form
            });
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Text = $"Tetris v{version} | Author: {Info.Author} Email: {Info.Email}";
        }
    }
}
