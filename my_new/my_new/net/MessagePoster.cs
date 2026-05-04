using System;
using System.Runtime.InteropServices;

namespace my_new.net;

internal class MessagePoster
{
	private IntPtr hWnd;

	private int Msg;

	private int wParam;

	private int lParam;

	public MessagePoster(IntPtr h, int msg, int w = 0, int l = 0)
	{
		hWnd = h;
		Msg = msg;
		wParam = w;
		lParam = l;
	}

	[DllImport("user32.dll")]
	public static extern IntPtr PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

	~MessagePoster()
	{
		if (hWnd != IntPtr.Zero)
		{
			PostMessage(hWnd, Msg, wParam, lParam);
		}
	}
}
