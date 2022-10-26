using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu;
using Emgu.Util;
using Emgu.CV.Util;
using DirectShowLib;

namespace sii_lab1
{
    public partial class Form1 : Form
    {
        private static CascadeClassifier classifierFace = new CascadeClassifier("haarcascade_frontalface_alt_tree.xml");
        private static CascadeClassifier classifierEye = new CascadeClassifier("haarcascade_eye.xml");
        private VideoCapture capture = null;
        private DsDevice[] webCams = null;
        private int selectedCameraId = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private Bitmap Find(Bitmap image)
        {
            Bitmap bitmap = new Bitmap(image);
            Image<Bgr, byte> grayImage = new Image<Bgr, byte>(bitmap);

            Rectangle[] faces = classifierFace.DetectMultiScale(grayImage, 1.01, 2);
            foreach (Rectangle face in faces)
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    using (Pen pen = new Pen(Color.Red, 3))
                    {
                        graphics.DrawRectangle(pen, face);
                    }
                }
            }

            int recCount = 9;
            Rectangle[] eyes = classifierEye.DetectMultiScale(grayImage, 1.08, recCount); ;
            while (eyes.Count() % 2 != 0 || eyes.Count() > 20)
            {
                eyes = classifierEye.DetectMultiScale(grayImage, 1.08, recCount);
                recCount++;
            }

            foreach (Rectangle eye in eyes)
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    using (Pen pen = new Pen(Color.Yellow, 3))
                    {
                        graphics.DrawRectangle(pen, eye);
                    }
                }
            }
            return bitmap;
        }



        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                DialogResult res = openFileDialog1.ShowDialog();

                if(res == DialogResult.OK)
                {
                    string path = openFileDialog1.FileName;
                    pictureBox1.Image = Image.FromFile(path);
                    Bitmap bitmap = new Bitmap(pictureBox1.Image);
                    bitmap = Find(bitmap);

                    pictureBox1.Image = bitmap;


                    pictureBox1.Image = bitmap;

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webCams = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            for(int i = 0; i < webCams.Length; i++)
            {
                toolStripComboBox1.Items.Add(webCams[i].Name);
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedCameraId = toolStripComboBox1.SelectedIndex;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                if (webCams.Length == 0) throw new Exception("Нет доступных камер!");
                else if (toolStripComboBox1.SelectedItem == null) throw new Exception("Необходимо выбрать камеру!!");
                else if (capture != null) capture.Start();
                else
                {
                    capture = new VideoCapture(selectedCameraId);
                    capture.ImageGrabbed += Capture_ImageGrabbed;
                    capture.Start();
                }


            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                Mat m = new Mat();
                capture.Retrieve(m);
                Bitmap outImage = Find(m.ToImage<Bgr, byte>().Flip(Emgu.CV.CvEnum.FlipType.Horizontal).Bitmap);
                pictureBox1.Image = outImage;

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try 
            {
                if(capture != null)
                {
                    capture.Pause();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (capture != null)
                {
                    capture.Pause();
                    capture.Dispose();
                    capture = null;
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                    selectedCameraId = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
