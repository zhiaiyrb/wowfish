using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace fish
{


    public partial class Form1 : Form
    {
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        #region GetWindowCapture的dll引用
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rectangle rect);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(
            IntPtr hdc // handle to DC
        );
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(
            IntPtr hdc,         // handle to DC
            int nWidth,      // width of bitmap, in pixels
            int nHeight      // height of bitmap, in pixels
        );
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(
            IntPtr hdc,           // handle to DC
            IntPtr hgdiobj    // handle to object
        );
        [DllImport("gdi32.dll")]
        private static extern int DeleteDC(
            IntPtr hdc           // handle to DC
        );
        [DllImport("user32.dll")]
        private static extern bool PrintWindow(
            IntPtr hwnd,                // Window to copy,Handle to the window that will be copied.
            IntPtr hdcBlt,              // HDC to print into,Handle to the device context.
            UInt32 nFlags               // Optional flags,Specifies the drawing options. It can be one of the following values.
        );
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(
                IntPtr hwnd
            );
        #endregion


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            //注册热键F5，Id号为103。   
            HotKey.RegisterHotKey(Handle, 103, HotKey.KeyModifiers.None, Keys.F11);
        }
        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            //按快捷键    
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    switch (m.WParam.ToInt32())
                    {

                        case 103:
                            button1_Click(null, null);
                            break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Bitmap memory = new Bitmap(1920, 1080);
            Graphics g = Graphics.FromImage(memory);
            Size mySize = new Size(1920, 1080);
            g.CopyFromScreen(0, 0, 0, 0, mySize);


           // string lpszParentWindow = "魔兽世界"; //窗口标题

           // IntPtr ParenthWnd = new IntPtr(0);

            //查到窗体，得到整个窗体
           // ParenthWnd = FindWindow(null, lpszParentWindow);
           // var bitmap = GetWindowCapture(ParenthWnd);

            Image<Bgr, byte> image = new Image<Bgr, byte>(memory);

            Mat img1 = image.Mat;//CvInvoke.Imread("../fish.jpg");
            Mat img2 = CvInvoke.Imread("../piao2.png");

            imageBox3.Image = img1;
            imageBox2.Image = img2;

            Mat imgout = new Mat();
            CvInvoke.MatchTemplate(img1, img2, imgout,TemplateMatchingType.CcoeffNormed);
            var max_loc = new Point();
            var min_loc = new Point();
            double max = 0, min = 0;
            CvInvoke.MinMaxLoc(imgout, ref min, ref max, ref min_loc, ref max_loc);
            imageBox1.Image = img1;
            Size img2Size = img2.Size;
            int x = (int) (img2Size.Width * 0.5) + max_loc.X;
            int y = (int)(img2Size.Height * 0.5) + max_loc.Y;

            CvInvoke.Circle(img1, new Point(x,y), 50,new MCvScalar(255,0,0));
            
        }

        private void imageBox1_Click(object sender, EventArgs e)
        {

        }


        public static Bitmap GetWindowCapture(IntPtr hWnd)
        {
            IntPtr hscrdc = GetWindowDC(hWnd);
            Rectangle windowRect = new Rectangle();
            GetWindowRect(hWnd, ref windowRect);
            int width = Math.Abs(windowRect.X - windowRect.Width);
            int height = Math.Abs(windowRect.Y - windowRect.Height);
            IntPtr hbitmap = CreateCompatibleBitmap(hscrdc, width, height);
            IntPtr hmemdc = CreateCompatibleDC(hscrdc);
            SelectObject(hmemdc, hbitmap);
            PrintWindow(hWnd, hmemdc, 0);
            Bitmap bmp = Image.FromHbitmap(hbitmap);
            DeleteDC(hscrdc);//删除用过的对象
            DeleteDC(hmemdc);//删除用过的对象
            return bmp;
        }


    }
}
