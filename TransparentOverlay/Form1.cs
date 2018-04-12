using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TransparentOverlay
{
	public partial class Form1 : Form
	{
		#region DllImports
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr FindWindow(string strClassName, string strWindowName);

		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

		[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
		public static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);

		[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
		public static extern int SetWindowLong(IntPtr hWnd, GWL nIndex, int dwNewLong);

		[DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
		public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, int crKey, byte alpha, LWA dwFlags);
		#endregion

		#region ShittyNeededStructsEnums
		public struct Rect
		{
			public int Left { get; set; }
			public int Top { get; set; }
			public int Right { get; set; }
			public int Bottom { get; set; }
		}

		public enum GWL
		{
			ExStyle = -20
		}

		public enum WS_EX
		{
			Transparent = 0x20,
			Layered = 0x80000
		}

		public enum LWA
		{
			ColorKey = 0x1,
			Alpha = 0x2
		}
		#endregion

		public Form1()
		{
			InitializeComponent();

			//SetClickthrough
			int initialStyle = GetWindowLong(this.Handle, GWL.ExStyle);
			SetWindowLong(this.Handle, GWL.ExStyle, initialStyle | (int)WS_EX.Layered | (int)WS_EX.Transparent);
			SetLayeredWindowAttributes(this.Handle, 0, 128, LWA.Alpha);

			//New onRun Thread
			new Thread(() =>
			{
				onRun();
			}).Start();
		}

		private void onRun()
		{
			Process[] processes = Process.GetProcessesByName("notepad");
			Process process = processes.SingleOrDefault();
			IntPtr ptr = process.MainWindowHandle;
			Rect NotepadRect = new Rect();
			var graphics = this.CreateGraphics();
			Font font = new Font("Arial", 12);

			while (process != default(Process) && !process.HasExited) //Check if process was found & still running
			{
				//Get Size & Location of window
				GetWindowRect(ptr, ref NotepadRect);
				int h = NotepadRect.Bottom - NotepadRect.Top, w = NotepadRect.Right - NotepadRect.Left;

				graphics = this.CreateGraphics();
				MethodInvoker mi = delegate () //Delegate so we can access the form elements
				{
					this.Location = new Point(NotepadRect.Left, NotepadRect.Top);
					this.Size = new Size(w, h);
					this.TopMost = true;
					this.TransparencyKey = this.BackColor = Color.LightSlateGray;
					//Draw here shit on the window
					graphics.DrawLine(Pens.Red, 0, 0, this.Size.Width, this.Size.Height);
					graphics.DrawString("Test", font, Brushes.Black, new Point(this.Size.Width/2, this.Size.Height/2));
				};
				this.Invoke(mi);

				//Sleep
				Thread.Sleep(5);
			}

			Application.Exit();
		}
	}
}
