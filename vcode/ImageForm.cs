using System;
using System.Drawing;
using System.Windows.Forms;

namespace YugiohCardManager
{
    public class ImageForm : Form
    {
        private PictureBox pictureBox;

        public ImageForm(Image image)
        {
            this.Text = "Full Size Image";
            this.WindowState = FormWindowState.Maximized;

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = image,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            this.Controls.Add(pictureBox);
        }
    }
}
