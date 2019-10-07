using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace fish
{


    public partial class Form1 : Form
    {
        #region windows的dll引用
        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int x, int y);
        //移动鼠标 
        const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;


        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
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

       

        #endregion

        private Mat fishFloatMat = null;
        private readonly int screenW = Screen.PrimaryScreen.Bounds.Width;
        private readonly int screenH = Screen.PrimaryScreen.Bounds.Height;
        private IWaveIn captureDevice;
        private bool fishStart = false;

 
        public Form1()
        {
            InitializeComponent();
            fishFloatMat = CvInvoke.Imread("../piao2.png");
            timer1.Enabled = true;
            LoadDeviceList();
        }

        private void LoadDeviceList()
        {
            var deviceEnum = new MMDeviceEnumerator();
            var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

            comboWasapiDevices.DataSource = devices;
            comboWasapiDevices.DisplayMember = "FriendlyName";

            var renderDevices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
            comboWasapiDevices.DataSource = renderDevices;
            comboWasapiDevices.DisplayMember = "FriendlyName";
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
           
        }

        private static int times = 0;
        private static float totaldb = 0;
        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            ushort s = BitConverter.ToUInt16(e.Buffer, 0);
            totaldb += s / 32768f;
            times++;
            if (times == 20)
            {
                Console.WriteLine(totaldb.ToString("F"));
                times = 0;
                totaldb = 0;
            }

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            //注册热键F5，Id号为103。   
            HotKey.RegisterHotKey(Handle, 103, HotKey.KeyModifiers.None, Keys.O);
            HotKey.RegisterHotKey(Handle, 104, HotKey.KeyModifiers.None, Keys.P);
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
                            fishStart = true;
                            break;
                        case 104:
                            fishStart = false;
                            break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        private Image<Bgr, byte> getScreenImage()
        {
            Bitmap memory = new Bitmap(screenW, screenH);
            Graphics g = Graphics.FromImage(memory);
            Size mySize = new Size(screenW, screenH);
            g.CopyFromScreen(0, 0, 0, 0, mySize);
            return new Image<Bgr, byte>(memory);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Image<Bgr, byte> image = getScreenImage();
            var floatPoint = getFishFloatPoint(image);
            CvInvoke.Circle(image, floatPoint, 50, new MCvScalar(255, 0, 0));

        }

        private bool TimerNext = true;
        private void startFish()
        {
            TimerNext = false;
            keybd_event(Keys.D1, 0, 0, 0);
            Thread.Sleep(100);
            keybd_event(Keys.D1, 0, 0x2, 0);
            richTextBox1.AppendText("key 1 pressed \n" );
            if (!fishStart)
            {
                return;
            }
            Thread.Sleep(1500);
            var floatPoint = getFishFloatPoint();
            //mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, floatPoint.X, floatPoint.Y, 0, 0);
            SetCursorPos(floatPoint.X, floatPoint.Y);
            richTextBox1.AppendText($"move mouse to X:{floatPoint.X},Y:{floatPoint.Y} \n");
            if (!fishStart)
            {
                return;
            }
            Thread.Sleep(1000);
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, floatPoint.X, floatPoint.Y, 0, 0);
            richTextBox1.AppendText($"mouse right click \n");
            if (!fishStart)
            {
                return;
            }
            Thread.Sleep(1000);
            TimerNext = true;


        }

        private Point getFishFloatPoint()
        {
            var image = getScreenImage();

            return getFishFloatPoint(image);
        }

        private Point getFishFloatPoint(Image<Bgr, byte> source)
        {
            var mat = new Mat();
            CvInvoke.MatchTemplate(source, fishFloatMat, mat, TemplateMatchingType.CcoeffNormed);
            var max_loc = new Point();
            var min_loc = new Point();
            double max = 0, min = 0;
            CvInvoke.MinMaxLoc(mat, ref min, ref max, ref min_loc, ref max_loc);
            var img2Size = fishFloatMat.Size;
            int x = (int)(img2Size.Width * 0.5) + max_loc.X;
            int y = (int)(img2Size.Height * 0.5) + max_loc.Y;

            return new Point(x,y);
        }


        private void button2_Click(object sender, EventArgs e)
        {
            StringBuilder msgBuilder = new StringBuilder("Performance: ");

            //Load the image from file and resize it for display
            Image<Bgr, Byte> img =
               new Image<Bgr, byte>("../300px.png")
               .Resize(400, 400, Emgu.CV.CvEnum.Inter.Linear, true);

            //Convert the image to grayscale and filter out the noise
            UMat uimage = new UMat();
            CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);

            //use image pyr to remove noise
            UMat pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);

            //Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();

            #region circle detection
            Stopwatch watch = Stopwatch.StartNew();
            double cannyThreshold = 180.0;
            double circleAccumulatorThreshold = 120;
            CircleF[] circles = CvInvoke.HoughCircles(uimage, HoughType.Gradient, 2.0, 20.0, cannyThreshold, circleAccumulatorThreshold, 5);

            watch.Stop();
            msgBuilder.Append(String.Format("Hough circles - {0} ms; ", watch.ElapsedMilliseconds));
            #endregion

            #region Canny and edge detection
            watch.Reset(); watch.Start();
            double cannyThresholdLinking = 120.0;
            UMat cannyEdges = new UMat();
            CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);

            LineSegment2D[] lines = CvInvoke.HoughLinesP(
               cannyEdges,
               1, //Distance resolution in pixel-related units
               Math.PI / 45.0, //Angle resolution measured in radians.
               20, //threshold
               30, //min Line width
               10); //gap between lines

            watch.Stop();
            msgBuilder.Append(String.Format("Canny & Hough lines - {0} ms; ", watch.ElapsedMilliseconds));
            #endregion

            #region Find triangles and rectangles
            watch.Reset(); watch.Start();
            List<Triangle2DF> triangleList = new List<Triangle2DF>();
            List<RotatedRect> boxList = new List<RotatedRect>(); //a box is a rotated rectangle

            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    using (VectorOfPoint contour = contours[i])
                    using (VectorOfPoint approxContour = new VectorOfPoint())
                    {
                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                        if (CvInvoke.ContourArea(approxContour, false) > 250) //only consider contours with area greater than 250
                        {
                            if (approxContour.Size == 3) //The contour has 3 vertices, it is a triangle
                            {
                                Point[] pts = approxContour.ToArray();
                                triangleList.Add(new Triangle2DF(
                                   pts[0],
                                   pts[1],
                                   pts[2]
                                   ));
                            }
                            else if (approxContour.Size == 4) //The contour has 4 vertices.
                            {
                                #region determine if all the angles in the contour are within [80, 100] degree
                                bool isRectangle = true;
                                Point[] pts = approxContour.ToArray();
                                LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

                                for (int j = 0; j < edges.Length; j++)
                                {
                                    double angle = Math.Abs(
                                       edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                    if (angle < 80 || angle > 100)
                                    {
                                        isRectangle = false;
                                        break;
                                    }
                                }
                                #endregion

                                if (isRectangle) boxList.Add(CvInvoke.MinAreaRect(approxContour));
                            }
                        }
                    }
                }
            }

            watch.Stop();
            msgBuilder.Append(String.Format("Triangles & Rectangles - {0} ms; ", watch.ElapsedMilliseconds));
            #endregion

            //box4.Image = img;
            this.Text = msgBuilder.ToString();

            #region draw triangles and rectangles
            Image<Bgr, Byte> triangleRectangleImage = img.CopyBlank();
            foreach (Triangle2DF triangle in triangleList)
                triangleRectangleImage.Draw(triangle, new Bgr(Color.DarkBlue), 2);
            foreach (RotatedRect box in boxList)
                triangleRectangleImage.Draw(box, new Bgr(Color.DarkOrange), 2);
            //box1.Image = triangleRectangleImage;
            #endregion

            #region draw circles
            Image<Bgr, Byte> circleImage = img.CopyBlank();
            foreach (CircleF circle in circles)
                circleImage.Draw(circle, new Bgr(Color.Brown), 2);
            //box2.Image = circleImage;
            #endregion

            #region draw lines
            Image<Bgr, Byte> lineImage = img.CopyBlank();
            foreach (LineSegment2D line in lines)
                lineImage.Draw(line, new Bgr(Color.Green), 2);
            //box4.Image = lineImage;
            #endregion

            GC.Collect();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (fishStart && TimerNext)
            {
                startFish();
            }
        }

        private IWaveIn CreateWaveInDevice()
        {
            var device = (MMDevice)comboWasapiDevices.SelectedItem;
            IWaveIn newWaveIn = new WasapiLoopbackCapture(device);
            newWaveIn.DataAvailable += OnDataAvailable;
            newWaveIn.RecordingStopped += OnRecordingStopped;
            return newWaveIn;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (captureDevice != null)
            {
                Cleanup();
            }

            if (captureDevice == null)
            {
                captureDevice = CreateWaveInDevice();
            }

            captureDevice.StartRecording();
        }

        private void Cleanup()
        {
            if (captureDevice != null)
            {
                captureDevice.Dispose();
                captureDevice = null;
            }
        }

        private void comboWasapiLoopbackDevices_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
