using System;
using System.Drawing;
using System.Windows.Forms;

namespace YugiohCardManager
{
    public class ProgressForm : Form
    {
        private ProgressBar progressBar;
        private Label statusLabel;

        public ProgressForm()
        {
            this.Text = "Loading";
            this.Font = new Font("Segoe UI", 10);
            this.Size = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            progressBar = new ProgressBar
            {
                Left = 20,
                Top = 40,
                Width = 350,
                Style = ProgressBarStyle.Continuous
            };

            statusLabel = new Label
            {
                Left = 20,
                Top = 80,
                Width = 350,
                Text = "Please wait..."
            };

            Controls.Add(progressBar);
            Controls.Add(statusLabel);

            CenterControls();
        }

        private void CenterControls()
        {
            if (progressBar == null || statusLabel == null)
                return;

            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;

            progressBar.Left = centerX - progressBar.Width / 2;
            statusLabel.Left = centerX - statusLabel.Width / 2;
            progressBar.Top = centerY - progressBar.Height / 2 - 20;
            statusLabel.Top = progressBar.Bottom + 10;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CenterControls();
        }

        public void UpdateProgress(int progress, string status)
        {
            progressBar.Value = progress;
            statusLabel.Text = status;
        }
    }
}
