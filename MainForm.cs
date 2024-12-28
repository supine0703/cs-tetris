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
            this.Controls.Add(new Panel
            {
                Dock = DockStyle.Fill, // Fill the entire form
            });
        }
    }
}
