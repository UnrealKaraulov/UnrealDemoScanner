using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoScanner
{
    public partial class EntitiesPreviewWindow : Form
    {
        public EntitiesPreviewWindow(string path)
        {
            InitializeComponent();
        }

        bool Playing = false;

        private void playBtn_Click(object sender, EventArgs e)
        {
            Playing = !Playing;
        }

        private void entPreviewPicture_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Image tmpImage = new Bitmap(2048, 2048);

            Graphics graphics = Graphics.FromImage(tmpImage);
            
            graphics.Clear(Color.White);

            // Координаты центра круга
            float centerX = 50.0f;
            float centerY = 50.0f;

            // Радиус круга
            float radius = 10;

            // Отрисовка круга
            Pen myPen = new Pen(Color.Black, 2.0f);
            graphics.DrawEllipse(myPen, centerX - radius, centerY - radius, radius * 2, radius * 2);

            g.DrawImageUnscaledAndClipped(tmpImage, e.ClipRectangle);
        }

        private void fpsSpeed_TextChanged(object sender, EventArgs e)
        {
            Playing = false;

            int fps = int.Parse(fpsSpeed.Text);

            bool reset = false;

            if (fps < 1)
            {
                fps = 100;
                reset = true;
            }
            else if (fps > 1000)
            {
                fps = 100;
                reset = true;
            }

            fpsSpeed.Text = fps.ToString();

            if (reset)
            {
                this.ActiveControl = null;
                this.Focus();
            }
        }

        private void fpsTimer_Tick(object sender, EventArgs e)
        {

        }

        private void EntitiesPreviewWindow_Load(object sender, EventArgs e)
        {

        }
    }
}
