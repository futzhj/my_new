using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace my_new;

internal static class Win32API
{
	public struct POINT
	{
		public int X;

		public int Y;

		public POINT(int x, int y)
		{
			X = x;
			Y = y;
		}

		public POINT(Point pt)
		{
			X = Convert.ToInt32(pt.X);
			Y = Convert.ToInt32(pt.Y);
		}
	}

	[DllImport("user32.dll")]
	internal static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

	[DllImport("user32.dll")]
	internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

	[DllImport("user32.dll")]
	public static extern bool SetWindowRgn(IntPtr hwnd, IntPtr hRgn, bool redraw);

	[DllImport("gdi32.dll")]
	public static extern IntPtr CreateRectRgn(int Left, int Top, int RectRightBottom_X, int RectRightBottom_Y);

	[DllImport("gdi32.dll")]
	public static extern IntPtr CreateEllipticRgn(int Left, int Top, int RectRightBottom_X, int RectRightBottom_Y);

	[DllImport("GDI32.dll")]
	public static extern bool DeleteObject(IntPtr objectHandle);

	[DllImport("gdi32.dll")]
	public static extern int CombineRgn(IntPtr hrgnDstm, IntPtr hrgnScr1, IntPtr hrgnScr2, int iMode);
}
