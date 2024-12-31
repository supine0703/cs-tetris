namespace Tetris
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(768, 1024);
            MinimumSize = new Size(500, 800);
            Name = "MainForm";
            byte[] Author = [
                0xe6, 0x9d, 0x8e, 0xe5, 0xae, 0x97, 0xe9, 0x9c, 0x96
            ];
            Text = $"Tetris Author: {System.Text.Encoding.UTF8.GetString(Author)} Email: supine0703@outlook.com"; 
            Load += MainForm_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}
