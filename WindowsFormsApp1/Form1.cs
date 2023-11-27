using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Vallecera_Image_Processing : Form
    {
        Bitmap loaded;
        Bitmap processed;
        Bitmap subResult;
        bool subMode;
        bool camMode;
        private Device webcam;

        public Vallecera_Image_Processing()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            subtractMode(false);
            camMode = false;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg";
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            loaded = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = loaded;
        }

        private void basicCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Color pixel;
            processed = new Bitmap(loaded.Width,loaded.Height);
            for(int i = 0; i < loaded.Width; i++)
            {
                for(int j = 0; j < loaded.Height; j++)
                {
                    pixel = loaded.GetPixel(i, j);
                    processed.SetPixel(i, j, pixel);
                }
            }
            pictureBox2.Image = processed;
        }

        private void greyscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Color pixel;
            processed = new Bitmap(loaded.Width, loaded.Height);
            for (int i = 0; i < loaded.Width; i++)
            {
                for (int j = 0; j < loaded.Height; j++)
                {
                    pixel = loaded.GetPixel(i, j);
                    int greyValue = (pixel.R + pixel.G + pixel.B) / 3;
                    processed.SetPixel(i, j, Color.FromArgb(greyValue,greyValue,greyValue));
                }
            }
            pictureBox2.Image = processed;
        }

        private void colorInversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Color pixel;
            processed = new Bitmap(loaded.Width, loaded.Height);
            for (int i = 0; i < loaded.Width; i++)
            {
                for (int j = 0; j < loaded.Height; j++)
                {
                    pixel = loaded.GetPixel(i, j);
                    processed.SetPixel(i, j, Color.FromArgb(255-pixel.R, 255-pixel.G, 255-pixel.B));
                }
            }
            pictureBox2.Image = processed;
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int[] histogram = new int[256];
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    Color c = loaded.GetPixel(x, y);
                    int intensity = (c.R + c.G + c.B) / 3;
                    histogram[intensity]++;
                }
            }

            int maxCount = histogram.Max();

            processed = new Bitmap(256, 300);
            for(int x = 0; x < 256; x++)
            {
                double percentile = ((double) histogram[x] / (double) maxCount) * 300;
                for(int y = 0; y < 300; y++)
                {
                    if (percentile > 299-y)
                    {
                        processed.SetPixel(x, y, Color.Red);
                    } else
                    {
                        processed.SetPixel(x, y, Color.Black);
                    }
                }
            }
            pictureBox2.Image = processed;
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Color pixel;
            processed = new Bitmap(loaded.Width, loaded.Height);
            for (int i = 0; i < loaded.Width; i++)
            {
                for (int j = 0; j < loaded.Height; j++)
                {
                    pixel = loaded.GetPixel(i, j);
                    int greyValue = (pixel.R + pixel.G + pixel.B) / 3;
                    int sepiaR = (int)(greyValue * 1.07);
                    int sepiaG = (int)(greyValue * 0.74);
                    int sepiaB = (int)(greyValue * 0.43);
                    if (sepiaR > 255) sepiaR = 255;
                    if (sepiaG > 255) sepiaG = 255;
                    if (sepiaB > 255) sepiaB = 255;
                    processed.SetPixel(i, j, Color.FromArgb(sepiaR, sepiaG, sepiaB));
                }
            }
            pictureBox2.Image = processed;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "JPEG Image|.jpg|PNG Image|.png|All Files|.";
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            pictureBox2.Image.Save(saveFileDialog1.FileName);
        }

        private string loadImage()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
                openFileDialog.Title = "Open an Image File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }
            }

            return null;
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            processed = new Bitmap(loadImage());
            pictureBox2.Image = processed;
        }

        private void btnLoadBg_Click(object sender, EventArgs e)
        {
            loaded = new Bitmap(loadImage());
            pictureBox1.Image = loaded;
        }

        private void btnSubtract_Click(object sender, EventArgs e)
        {
            subResult = new Bitmap(processed.Width, processed.Height);
            Color mygreen = Color.FromArgb(0,0,255); // assume top leftmost pixel is green
            int greygreen = (mygreen.R + mygreen.G +  mygreen.B) / 3;
            int threshold = 5;

            for(int x = 0; x < processed.Width; x++)
            {
                for(int y = 0; y < processed.Height; y++)
                {
                    Color pixel = processed.GetPixel(x, y);
                    Color backpixel = loaded.GetPixel(x, y);
                    int grey = (pixel.R + pixel.G + pixel.B) / 3;
                    int subValue = Math.Abs(grey - greygreen);
                    if (subValue < threshold)
                        subResult.SetPixel(x, y, backpixel);
                    else
                        subResult.SetPixel(x, y, pixel);
                }
            }
            pictureBox3.Image = subResult;
        }

        private void subtractMode()
        {
            subMode = !subMode;
            pictureBox3.Visible = subMode;
            btnLoadBg.Visible = subMode;
            btnLoadImage.Visible = subMode;
            btnSubtract.Visible = subMode;
        }

        private void subtractMode(bool isSubMode)
        {
            pictureBox3.Visible = isSubMode;
            btnLoadBg.Visible = isSubMode;
            btnLoadImage.Visible = isSubMode;
            btnSubtract.Visible = isSubMode;
            subMode = isSubMode;
        }

        private void subtractionModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            subtractMode();
            webcam.Stop();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            subtractMode(false);
            webcam.Stop();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            subtractMode(false);
            webcam.Stop();
        }

        private void cameraModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            camMode = !camMode;
            Device[] devices = DeviceManager.GetAllDevices();

            if (devices.Length > 0 && camMode)
            {
                webcam = devices[0]; // Assuming you want to use the first detected device
                webcam.ShowWindow(pictureBox1);
            }
            else
            {
                webcam.Stop();
            }
        }
    }
}
