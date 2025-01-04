using System.Reflection;

namespace Tetris
{
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
            byte[] Author = [0xe6, 0x9d, 0x8e, 0xe5, 0xae, 0x97, 0xe9, 0x9c, 0x96];
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Text = $"Tetris v{version} | Author: {System.Text.Encoding.UTF8.GetString(Author)} Email: supine0703@outlook.com";
        }
    }
}
