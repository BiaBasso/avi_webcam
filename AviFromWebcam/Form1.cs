using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.VFW;
using AForge;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace cameraHVP
{


    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        VideoCaptureDevice cam;


        Bitmap frame = new Bitmap(1080, 720);

        FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        AVIWriter aviWriter;

        bool recording = false;
        bool paused = false;
        private void Form1_Load(object sender, EventArgs e)
        {
            recordBtn.Enabled = false;
            this.recordBtn.BackColor = Color.Aquamarine;
            statusTxt.Text = "";
            aviWriter = new AVIWriter();

            aviWriter.FrameRate = 800 / timer1.Interval;




            foreach (FilterInfo info in videoDevices)
            {
                listBox1.Items.Add(info.Name);
            }
            if (listBox1.Items.Count > 0)
                listBox1.SelectedIndex = 0;
        }

        private void startWebcam(String deviceMoniker)
        {

            if (cam != null && cam.IsRunning)
            {
                stopWebcam();
            }
            cam = new VideoCaptureDevice(deviceMoniker);
            cam.DesiredFrameSize = pictureBox1.Size;

            cam.NewFrame += new NewFrameEventHandler(cam_NewFrame);

            cam.Start();
            timer1.Enabled = true;
        }
        void cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            frame = (Bitmap)eventArgs.Frame.Clone();
        }
        private void stopWebcam()
        {
            cam.SignalToStop();
            timer1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            startWebcam(videoDevices[listBox1.SelectedIndex].MonikerString);
            recordBtn.Enabled = true;
            button1.Enabled = false;
            textBox1.Focus();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cam != null && cam.IsRunning)
            {
                cam.SignalToStop();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo info in videoDevices)
            {
                listBox1.Items.Add(info.Name);
            }
            if (listBox1.Items.Count > 0)
                listBox1.SelectedIndex = 0;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            process();
        }

        private void process()
        {
            if (frame == null) return;
            OilPainting op = new OilPainting();

            Pixellate p = new Pixellate();
            SaturationCorrection sc = new SaturationCorrection(0.0);
            BrightnessCorrection bc = new BrightnessCorrection(0.0);
            HueModifier hm = new HueModifier(50);




            Bitmap cannedFrame = frame;



            if (hueBar.Value != 0)
                new HueModifier(hueBar.Value).ApplyInPlace(cannedFrame);
            if (satBar.Value != 0)
                new SaturationCorrection(satBar.Value / 100.0).ApplyInPlace(cannedFrame);
            if (brightnessBar.Value != 0)
                new BrightnessCorrection(brightnessBar.Value / 100.0).ApplyInPlace(cannedFrame);
            if (pixBar.Value != 0)
                new Pixellate(pixBar.Value).ApplyInPlace(cannedFrame);

            pictureBox1.Image = cannedFrame;
            if (recording)
            {
                if (paused)
                {
                    statusTxt.Text = "Paused... " + aviWriter.Position + " frames taken";
                }
                else
                {

                    aviWriter.AddFrame(cannedFrame);
                    statusTxt.Text = "Recording... " + aviWriter.Position + " frames";
                }

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (recordBtn.Text == "Start Recording")
            {

                this.recordBtn.BackColor = Color.Red;
            }
            if (recordBtn.Text == "Stop Recording")
            {
                this.recordBtn.BackColor = Color.Aquamarine;
            }
            if (recording)
            {


                aviWriter.Close();
                recordBtn.Text = "Start Recording";
                recording = false;
                statusTxt.Text = "Stopped.";
            }
            else
            {

                SaveFileDialog sfd = new SaveFileDialog();

                sfd.InitialDirectory = (@"C:\Users\localuser\Videos\Captures\");
                //sfd.InitialDirectory = (@"C:\Users\bbasso\Videos\Captures\");

                sfd.FileName = textBox1.Text + DateTime.Now.ToString("_ddMMyyyy_HHmmss");

                sfd.Filter = "Audio Video Interleave File | *.avi";
                sfd.Title = "Save1";
                sfd.AddExtension = true;
                sfd.RestoreDirectory = true;

                SendKeys.Send("{ENTER}");
                DialogResult result = sfd.ShowDialog();


                if (result == DialogResult.OK)
                {
                    statusTxt.Text = "Recording...";
                    aviWriter.Close();
                    String file = sfd.FileName;
                    aviWriter.Open(file, pictureBox1.Size.Width, pictureBox1.Size.Height);
                    recordBtn.Text = "Stop Recording";
                    recording = true;
                    paused = false;
                }
            }
        }        

        private void hueBar_Scroll(object sender, EventArgs e)
        {
            hueText.Text = hueBar.Value.ToString();
        }

        private void satBar_Scroll(object sender, EventArgs e)
        {
            satText.Text = satBar.Value.ToString();
        }

        private void brightnessBar_Scroll(object sender, EventArgs e)
        {
            brightnessText.Text = brightnessBar.Value.ToString();
        }

        private void pixBar_Scroll(object sender, EventArgs e)
        {
            pixText.Text = pixBar.Value.ToString();
        }

        private void pixText_TextChanged(object sender, EventArgs e)
        {
            try
            {
                pixBar.Value = int.Parse(pixText.Text);
            }
            catch
            {
                pixBar.Value = 0;
            }
        }

        private void brightnessText_TextChanged(object sender, EventArgs e)
        {
            try
            {
                brightnessBar.Value = int.Parse(brightnessText.Text);
            }
            catch
            {
                brightnessBar.Value = 0;
            }
        }

        private void satText_TextChanged(object sender, EventArgs e)
        {
            try
            {
                satBar.Value = int.Parse(satText.Text);
            }
            catch
            {
                satBar.Value = 0;
            }
        }

        private void hueText_TextChanged(object sender, EventArgs e)
        {
            try
            {
                hueBar.Value = int.Parse(hueText.Text);
            }
            catch
            {
                hueBar.Value = 0;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (recordBtn.Text == "Start Recording")
                {
                    recordBtn.PerformClick();

                    return;
                }
                if (recordBtn.Text == "Stop Recording")
                {
                    recordBtn.PerformClick();
                    textBox1.Clear();
                    textBox1.Focus();
                    return;
                }
            }  
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.TextLength == 12)
            {
                
                if (textBox1.Text != textBox2.Text)
                {
                    recordBtn.PerformClick();
                    textBox2.Text = textBox1.Text;
                    textBox1.SelectAll();
                }
                else
                {
                    recordBtn.PerformClick();
                    textBox1.SelectAll();
                }
            }
        }
    }
}