using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using SURFFeatureExample;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCvSharp.Extensions;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Security.Principal;
using System.Diagnostics;

namespace SURFFeature
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //Emgu.CV.Util.VectorOfKeyPoint model;
        //Emgu.CV.Util.VectorOfKeyPoint real;
        [DllImport("user32", EntryPoint = "SetWindowsHookExA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, int hMod, int dwThreadId);
        [DllImport("user32", EntryPoint = "UnhookWindowsHookEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int UnhookWindowsHookEx(int hHook);
        public delegate int LowLevelKeyboardProcDelegate(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);
        [DllImport("user32", EntryPoint = "CallNextHookEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int CallNextHookEx(int hHook, int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);
        public const int WH_KEYBOARD_LL = 13;

        /*code needed to disable start menu*/
        [DllImport("user32.dll")]
        private static extern int FindWindow(string className, string windowText);
        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;
        public struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        public static int intLLKey;

        public int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            bool blnEat = false;

            switch (wParam)
            {
                case 256:
                case 257:
                case 260:
                case 261:
                    //Alt+Tab, Alt+Esc, Ctrl+Esc, Windows Key,
                    blnEat = ((lParam.vkCode == 9) && (lParam.flags == 32)) | ((lParam.vkCode == 27) && (lParam.flags == 32)) | ((lParam.vkCode == 27) && (lParam.flags == 0)) | ((lParam.vkCode == 91) && (lParam.flags == 1)) | ((lParam.vkCode == 92) && (lParam.flags == 1)) | ((lParam.vkCode == 73) && (lParam.flags == 0));
                    break;
            }

            if (blnEat == true)
            {
                return 1;
            }
            else
            {
                return CallNextHookEx(0, nCode, wParam, ref lParam);
            }
        }
        public void KillStartMenu()
        {
            int hwnd = FindWindow("Shell_TrayWnd", "");
            ShowWindow(hwnd, SW_HIDE);
        }

        public static void ShowStartMenu()
        {
            int hwnd = FindWindow("Shell_TrayWnd", "");
            ShowWindow(hwnd, SW_SHOW);
        }
        public void KillCtrlAltDelete()
        {
            RegistryKey regkey;
            string keyValueInt = "1";
            string subKey = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";

            try
            {
                regkey = Registry.CurrentUser.CreateSubKey(subKey);
                regkey.SetValue("DisableTaskMgr", keyValueInt);
                regkey.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void EnableCTRLALTDEL()
        {
            try
            {
                string subKey = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";
                RegistryKey rk = Registry.CurrentUser;
                RegistryKey sk1 = rk.OpenSubKey(subKey);
                if (sk1 != null)
                    rk.DeleteSubKeyTree(subKey);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);
        private void Form1_Load(object sender, EventArgs e)
        {
            //var inst = LoadLibrary("user32.dll").ToInt32();
            //intLLKey = SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKeyboardProc, inst, 0);
            //KillStartMenu();
            //KillCtrlAltDelete();
            //Application.Idle += Application_Idle;
            backgroundWorker1.RunWorkerAsync();
            //OpenCD();
            //fd.DetectRaw(modelImage, model);
            gr1 = pictureBox3.CreateGraphics();
        }
        
        

        // Example use:     
        private const double surfHessianThresh = 300;
        private const bool surfExtendedFlag = true;
        //Emgu.CV.XFeatures2D.SURF surf = new Emgu.CV.XFeatures2D.SURF(500);
        Mat modelImage = CvInvoke.Imread("box.jpg", LoadImageType.Grayscale);
        //Emgu.CV.Features2D.DescriptorMatcher dm;
        //Emgu.CV.Features2D.FastDetector fd = new Emgu.CV.Features2D.FastDetector();

        int n = 0;
        private void Application_Idle(object sender, EventArgs e)
        {
            
                  

        }

        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi)]
        protected static extern int mciSendString
           (string mciCommand,
           StringBuilder returnValue,
           int returnLength,
           IntPtr callback);

        void OpenCD()
        {
            int result = mciSendString("set cdaudio door open", null, 0, IntPtr.Zero);
            //Thread.Sleep(1000);
            //result = mciSendString("set cdaudio door closed", null, 0, IntPtr.Zero);
        }

        
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            n = 0;

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            
        }

        private void Form1_TextChanged(object sender, EventArgs e)
        {

        }

        string comm = "";

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            comm += e.KeyChar;
            if (comm.Contains("tester")|| comm.Contains("еуыеук"))
            {
                Application.ExitThread();
                this.Close();
            }
        }
        Graphics gr1, gr2;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
            UnhookWindowsHookEx(intLLKey);
        }

        Random rnd = new Random();
        int row = 0;
        double q;
        int w = 0;

        private void timer2_Tick(object sender, EventArgs e)
        {
            listBox1.Items.Add(Guid.NewGuid().ToString());
            if (listBox1.Items.Count > 20)
            {
                listBox1.Items.RemoveAt(0);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var capture = new Capture(); //create a camera captue
            long matchTime;
            do
            {
                w = (int)q;
                if (row>=pictureBox3.Height )
                {
                    row = 0;
                }
                row++;
                if (rnd.Next(0,100)>50)
                {
                    w+=rnd.Next(10);
                }
                else
                {
                    w -= rnd.Next(10);
                }
                if (w<0)
                {
                    w = 0;
                }
                if (w>pictureBox3.Width )
                {
                    w = pictureBox3.Width;
                }
                gr1.DrawLine(new Pen(new SolidBrush(Color.Black   )), new Point(0, row), new Point(pictureBox3.Width, row));
                gr1.DrawLine(new Pen(new SolidBrush(Color.Green)),new Point(0,row),new Point((int)w,row));
               
                try
                {
                    using (Mat observedImage = capture.QueryFrame())
                    {
                        Rectangle Rect;
                       
                        Bitmap res2;
                        Mat result = DrawMatches.Draw(modelImage, observedImage, out matchTime, out Rect, out res2, out q);

                        try
                        {
                            this.Invoke(new Action(() =>
                            {
                                if (Rect.Width > 0 && Rect.Height > 0)
                                {
                                    if (Rect.Left + Rect.Width < observedImage.Width)
                                    {
                                        if (Rect.Top + Rect.Height < observedImage.Height)
                                        {
                                            if (Rect.Height / Rect.Width >= 1 && Rect.Height / Rect.Width < 3)
                                            {
                                                this.Text = q.ToString();
                                                if (q < 120&&q>20)
                                                {
                                                    n++;
                                                    if (n > 10)
                                                    {
                                                        //OpenCD();
                                                        pictureBox2.BackgroundImage = res2;
                                                        pictureBox2.Visible = true;
                                                        
                                                        pictureBox2.Visible = true;
                                                        label1.Text = "Пользователь опознан! Доступ разрешен.";
                                                        Opened op = new Opened();
                                                        op.Show();
                                                    }
                                                    else
                                                    {
                                                        pictureBox2.BackgroundImage = res2;
                                                        pictureBox2.Visible = true;
                                                        label1.Text = "Идентификация пользователя..." + n * 10 + "%";
                                                    }

                                                }
                                                else
                                                {
                                                    label1.Text = "Пользователь не опознан! Доступ запрещен.";
                                                }

                                            }
                                            else
                                            {
                                                pictureBox2.Visible = false;
                                                label1.Text = "Пользователь не опознан! Доступ запрещен.";
                                            }
                                        }
                                        else
                                        {
                                            pictureBox2.Visible = false;
                                            label1.Text = "Пользователь не опознан! Доступ запрещен.";
                                        }
                                    }
                                    else
                                    {
                                        pictureBox2.Visible = false;
                                        label1.Text = "Пользователь не опознан! Доступ запрещен.";
                                    }
                                }
                                else
                                {
                                    pictureBox2.Visible = false;
                                    label1.Text = "Пользователь не опознан! Доступ запрещен.";
                                }
                                pictureBox1.BackgroundImage = result.Bitmap;
                                this.Text = String.Format("Matched using {0} in {1} milliseconds:{2}", CudaInvoke.HasCuda ? "GPU" : "CPU", matchTime, q);



                            }));
                        }
                        catch (Exception ee)
                        {
                            break;
                        }

                    }
                    GC.Collect();
                }
                catch (Exception)
                {

                }
            } while (true);

        }
    }
}
