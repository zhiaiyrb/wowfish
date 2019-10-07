using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace fish
{
	public class Form1 : Form
	{
		public enum FishingState
		{
			FishingStart,
			FindFloat,
			FishBite,
			Waiting
		}

		private const int MOUSEEVENTF_MOVE = 1;

		private const int MOUSEEVENTF_LEFTDOWN = 2;

		private const int MOUSEEVENTF_LEFTUP = 4;

		private const int MOUSEEVENTF_RIGHTDOWN = 8;

		private const int MOUSEEVENTF_RIGHTUP = 16;

		private const int MOUSEEVENTF_MIDDLEDOWN = 32;

		private const int MOUSEEVENTF_MIDDLEUP = 64;

		private const int MOUSEEVENTF_ABSOLUTE = 32768;

		private Mat fishFloatMat = null;

		private readonly int screenW = Screen.PrimaryScreen.Bounds.Width;

		private readonly int screenH = Screen.PrimaryScreen.Bounds.Height;

		private IWaveIn captureDevice;

		private bool fishStart = false;

		private FishingState currentState;

		private static int times = 0;

		private static float totaldb = 0f;

		private DateTime timerRest;

		private Point floatPoint = default(Point);

		private IContainer components = null;

		private Button button1;

		private RichTextBox richTextBox1;

		private System.Windows.Forms.Timer timer1;
        private Label label1;
        private Label label2;
        private Emgu.CV.UI.ImageBox imageBox1;
        private ComboBox comboWasapiDevices;

		public event EventHandler FishBiteEvent;

		[DllImport("user32.dll", SetLastError = true)]
		public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

		[DllImport("user32")]
		private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

		[DllImport("User32.dll")]
		private static extern bool SetCursorPos(int x, int y);

		[DllImport("User32.dll")]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rectangle rect);

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

		[DllImport("gdi32.dll")]
		private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

		[DllImport("gdi32.dll")]
		private static extern int DeleteDC(IntPtr hdc);

		[DllImport("user32.dll")]
		private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

		[DllImport("user32.dll")]
		private static extern IntPtr GetWindowDC(IntPtr hwnd);

		public static Bitmap GetWindowCapture(IntPtr hWnd)
		{
			IntPtr windowDC = GetWindowDC(hWnd);
			Rectangle rect = default(Rectangle);
			GetWindowRect(hWnd, ref rect);
			int nWidth = Math.Abs(rect.X - rect.Width);
			int nHeight = Math.Abs(rect.Y - rect.Height);
			IntPtr intPtr = CreateCompatibleBitmap(windowDC, nWidth, nHeight);
			IntPtr intPtr2 = CreateCompatibleDC(windowDC);
			SelectObject(intPtr2, intPtr);
			PrintWindow(hWnd, intPtr2, 0u);
			Bitmap result = Image.FromHbitmap(intPtr);
			DeleteDC(windowDC);
			DeleteDC(intPtr2);
			return result;
		}



        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public Point ptScreenPos;
        }

		public Form1()
		{
			InitializeComponent();
			fishFloatMat = CvInvoke.Imread("../piao2.png");
			timer1.Enabled = true;
			LoadDeviceList();
		}

		private EventHandler OnFishBite()
		{
			currentState = FishingState.FishBite;
			return null;
		}

		private void LoadDeviceList()
		{
			MMDeviceEnumerator mMDeviceEnumerator = new MMDeviceEnumerator();
			List<MMDevice> dataSource = mMDeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();
			comboWasapiDevices.DataSource = dataSource;
			comboWasapiDevices.DisplayMember = "FriendlyName";
			List<MMDevice> dataSource2 = mMDeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
			comboWasapiDevices.DataSource = dataSource2;
			comboWasapiDevices.DisplayMember = "FriendlyName";
		}

		private void OnRecordingStopped(object sender, StoppedEventArgs e)
		{
		}

		private void OnDataAvailable(object sender, WaveInEventArgs e)
		{
			ushort num = BitConverter.ToUInt16(e.Buffer, 0);
			totaldb += (float)(int)num / 32768f;
			times++;
			if (times == 20)
			{
				Console.WriteLine(totaldb.ToString("F"));
				if (totaldb > 4f)
				{
					currentState = FishingState.FishBite;
				}
				times = 0;
				totaldb = 0f;
			}
		}

		private void Form1_Load_1(object sender, EventArgs e)
		{
			HotKey.RegisterHotKey(base.Handle, 103, HotKey.KeyModifiers.None, Keys.O);
			HotKey.RegisterHotKey(base.Handle, 104, HotKey.KeyModifiers.None, Keys.P);
            imageBox1.Image = fishFloatMat;
            //s = new Zgke.CopyScreen();
            //s.GetScreenImage += new Zgke.CopyScreen.GetImage(GetScreenImage);
        }

        private void GetScreenImage(Image p_image)
        {
            var bmpImage = new Bitmap(p_image);
            var img = new Image<Bgr, byte>(bmpImage);
            imageBox1.Image = img;
        }

        protected override void WndProc(ref Message m)
		{
			int msg = m.Msg;
			if (msg == 786)
			{
				switch (m.WParam.ToInt32())
				{
				case 103:
					fishStart = true;
					break;
				case 104:
					fishStart = false;
					break;
				}
			}
			base.WndProc(ref m);
		}

		private Image<Bgr, byte> getScreenImage()
		{
			Bitmap bitmap = new Bitmap(screenW, screenH);
			Graphics graphics = Graphics.FromImage(bitmap);
			Size blockRegionSize = new Size(screenW, screenH);
			graphics.CopyFromScreen(0, 0, 0, 0, blockRegionSize);
			return new Image<Bgr, byte>(bitmap);
		}

        private Zgke.CopyScreen s;
        private void button1_Click(object sender, EventArgs e)
        {
            s.GerScreenFormRectangle();
        }




        private void startFish()
		{
			if (currentState == FishingState.FishingStart)
			{
				keybd_event(Keys.D1, 0, 0u, 0u);
				Thread.Sleep(100);
				keybd_event(Keys.D1, 0, 2u, 0u);
				richTextBox1.AppendText("key 1 pressed \n");
				nextState(2000f, FishingState.FindFloat);
				currentState = FishingState.Waiting;
				timerRest = DateTime.Now;
			}
			if (currentState == FishingState.FindFloat)
			{
				floatPoint = getFishFloatPoint();
				SetCursorPos(floatPoint.X, floatPoint.Y);
				startCaptureSound();
				richTextBox1.AppendText($"move mouse to X:{floatPoint.X},Y:{floatPoint.Y} \n");
				currentState = FishingState.Waiting;
			}
			if (currentState == FishingState.FishBite)
			{
				stopCaptureSound();
				mouse_event(24, floatPoint.X, floatPoint.Y, 0, 0);
				richTextBox1.AppendText("mouse right click \n");
				currentState = FishingState.Waiting;
				nextState(1500f, FishingState.FishingStart);
			}
			if (currentState == FishingState.Waiting && (DateTime.Now - timerRest).TotalSeconds > 30.0)
			{
				stopCaptureSound();
				currentState = FishingState.FishingStart;
			}
		}

		private void nextState(float timeInterval, FishingState nextState)
		{
			System.Timers.Timer timer = new System.Timers.Timer((double)timeInterval);
			timer.AutoReset = false;
			timer.Elapsed += delegate
			{
				currentState = nextState;
			};
			timer.Start();
		}

		private Point getFishFloatPoint()
		{
			Image<Bgr, byte> screenImage = getScreenImage();
			return getFishFloatPoint(screenImage);
		}

		private Point getFishFloatPoint(Image<Bgr, byte> source)
		{
			Mat mat = new Mat();
			CvInvoke.MatchTemplate(source, fishFloatMat, mat, TemplateMatchingType.CcoeffNormed);
			Point maxLoc = default(Point);
			Point minLoc = default(Point);
			double maxVal = 0.0;
			double minVal = 0.0;
			CvInvoke.MinMaxLoc(mat, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
			Size size = fishFloatMat.Size;
			int x = (int)((double)size.Width * 0.5) + maxLoc.X;
			int y = (int)((double)size.Height * 0.5) + maxLoc.Y;
			return new Point(x, y);
		}

		private void button2_Click(object sender, EventArgs e)
		{
			StringBuilder stringBuilder = new StringBuilder("Performance: ");
			Image<Bgr, byte> image = new Image<Bgr, byte>("../300px.png").Resize(400, 400, Inter.Linear, preserveScale: true);
			UMat uMat = new UMat();
			CvInvoke.CvtColor(image, uMat, ColorConversion.Bgr2Gray);
			UMat uMat2 = new UMat();
			CvInvoke.PyrDown(uMat, uMat2);
			CvInvoke.PyrUp(uMat2, uMat);
			Stopwatch stopwatch = Stopwatch.StartNew();
			double num = 180.0;
			double param = 120.0;
			CircleF[] array = CvInvoke.HoughCircles(uMat, HoughType.Gradient, 2.0, 20.0, num, param, 5);
			stopwatch.Stop();
			stringBuilder.Append($"Hough circles - {stopwatch.ElapsedMilliseconds} ms; ");
			stopwatch.Reset();
			stopwatch.Start();
			double threshold = 120.0;
			UMat uMat3 = new UMat();
			CvInvoke.Canny(uMat, uMat3, num, threshold);
			LineSegment2D[] array2 = CvInvoke.HoughLinesP(uMat3, 1.0, 0.069813170079773182, 20, 30.0, 10.0);
			stopwatch.Stop();
			stringBuilder.Append($"Canny & Hough lines - {stopwatch.ElapsedMilliseconds} ms; ");
			stopwatch.Reset();
			stopwatch.Start();
			List<Triangle2DF> list = new List<Triangle2DF>();
			List<RotatedRect> list2 = new List<RotatedRect>();
			using (VectorOfVectorOfPoint vectorOfVectorOfPoint = new VectorOfVectorOfPoint())
			{
				CvInvoke.FindContours(uMat3, vectorOfVectorOfPoint, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
				int size = vectorOfVectorOfPoint.Size;
				for (int i = 0; i < size; i++)
				{
					using (VectorOfPoint curve = vectorOfVectorOfPoint[i])
					{
						using (VectorOfPoint vectorOfPoint = new VectorOfPoint())
						{
							CvInvoke.ApproxPolyDP(curve, vectorOfPoint, CvInvoke.ArcLength(curve, isClosed: true) * 0.05, closed: true);
							if (CvInvoke.ContourArea(vectorOfPoint) > 250.0)
							{
								if (vectorOfPoint.Size == 3)
								{
									Point[] array3 = vectorOfPoint.ToArray();
									list.Add(new Triangle2DF(array3[0], array3[1], array3[2]));
								}
								else if (vectorOfPoint.Size == 4)
								{
									bool flag = true;
									Point[] points = vectorOfPoint.ToArray();
									LineSegment2D[] array4 = PointCollection.PolyLine(points, closed: true);
									for (int j = 0; j < array4.Length; j++)
									{
										double num2 = Math.Abs(array4[(j + 1) % array4.Length].GetExteriorAngleDegree(array4[j]));
										if (num2 < 80.0 || num2 > 100.0)
										{
											flag = false;
											break;
										}
									}
									if (flag)
									{
										list2.Add(CvInvoke.MinAreaRect(vectorOfPoint));
									}
								}
							}
						}
					}
				}
			}
			stopwatch.Stop();
			stringBuilder.Append($"Triangles & Rectangles - {stopwatch.ElapsedMilliseconds} ms; ");
			Text = stringBuilder.ToString();
			Image<Bgr, byte> image2 = image.CopyBlank();
			foreach (Triangle2DF item in list)
			{
				image2.Draw(item, new Bgr(Color.DarkBlue), 2);
			}
			foreach (RotatedRect item2 in list2)
			{
				image2.Draw(item2, new Bgr(Color.DarkOrange), 2);
			}
			Image<Bgr, byte> image3 = image.CopyBlank();
			CircleF[] array5 = array;
			foreach (CircleF circle in array5)
			{
				image3.Draw(circle, new Bgr(Color.Brown), 2);
			}
			Image<Bgr, byte> image4 = image.CopyBlank();
			LineSegment2D[] array6 = array2;
			foreach (LineSegment2D line in array6)
			{
				image4.Draw(line, new Bgr(Color.Green), 2);
			}
			GC.Collect();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			if (fishStart)
			{
				startFish();
			}
		}

		private IWaveIn CreateWaveInDevice()
		{
			MMDevice mMDevice = (MMDevice)comboWasapiDevices.SelectedItem;
			IWaveIn waveIn = new WasapiLoopbackCapture(mMDevice);
			waveIn.DataAvailable += OnDataAvailable;
			waveIn.RecordingStopped += OnRecordingStopped;
			return waveIn;
		}

		private void button2_Click_1(object sender, EventArgs e)
		{
		}

		private void startCaptureSound()
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

		private void stopCaptureSound()
		{
			captureDevice?.StopRecording();
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

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.button1 = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.comboWasapiDevices = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.imageBox1 = new Emgu.CV.UI.ImageBox();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 61);
            this.button1.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(121, 27);
            this.button1.TabIndex = 0;
            this.button1.Text = "截取鱼漂";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(568, 12);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(275, 450);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // comboWasapiDevices
            // 
            this.comboWasapiDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboWasapiDevices.FormattingEnabled = true;
            this.comboWasapiDevices.Location = new System.Drawing.Point(12, 12);
            this.comboWasapiDevices.Name = "comboWasapiDevices";
            this.comboWasapiDevices.Size = new System.Drawing.Size(121, 20);
            this.comboWasapiDevices.TabIndex = 15;
            this.comboWasapiDevices.SelectedIndexChanged += new System.EventHandler(this.comboWasapiLoopbackDevices_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("SimSun", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(139, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 19);
            this.label1.TabIndex = 16;
            this.label1.Text = "选择声音输出设备";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("SimSun", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(13, 432);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(162, 19);
            this.label2.TabIndex = 17;
            this.label2.Text = "O键开始，p键结束";
            // 
            // imageBox1
            // 
            this.imageBox1.Location = new System.Drawing.Point(143, 61);
            this.imageBox1.Name = "imageBox1";
            this.imageBox1.Size = new System.Drawing.Size(157, 132);
            this.imageBox1.TabIndex = 2;
            this.imageBox1.TabStop = false;
            this.imageBox1.Click += new System.EventHandler(this.imageBox1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(855, 474);
            this.Controls.Add(this.imageBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboWasapiDevices);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        private void imageBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
