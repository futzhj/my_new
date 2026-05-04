using System;
using System.Windows.Media.Imaging;

namespace my_new.utils;

internal class WebBrowserImageReadyEvengArgs : EventArgs
{
	public BitmapSource Bms { get; private set; }

	public WebBrowserImageReadyEvengArgs(BitmapSource b)
	{
		Bms = b;
	}
}
