using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace my_new.utils;

internal class WebBrowserUtility : IDisposable
{
	[ComImport]
	[ComVisible(true)]
	[Guid("0000010d-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IViewObject
	{
		[PreserveSig]
		[return: MarshalAs(UnmanagedType.I4)]
		int Draw([MarshalAs(UnmanagedType.U4)] uint dwDrawAspect, int lindex, IntPtr pvAspect, [In] IntPtr ptd, IntPtr hdcTargetDev, IntPtr hdcDraw, [MarshalAs(UnmanagedType.Struct)] ref tagRECT lprcBounds, [MarshalAs(UnmanagedType.Struct)] ref tagRECT lprcWBounds, IntPtr pfnContinue, [MarshalAs(UnmanagedType.U4)] uint dwContinue);
	}

	public WebBrowser WebBrowser { get; private set; }

	public BitmapSource BitmapSource { get; private set; }

	public event EventHandler<WebBrowserImageReadyEvengArgs> WebBrowserImageReady;

	public WebBrowserUtility(int w, int h)
	{
		WebBrowser = new WebBrowser();
		WebBrowser.DocumentCompleted += DocumentCompleted;
		WebBrowser.Width = w;
		WebBrowser.Height = h;
	}

	public void Navigate(Uri url)
	{
		if (WebBrowser != null)
		{
			WebBrowser.Navigate(url);
			return;
		}
		throw new InvalidOperationException();
	}

	private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
	{
		using (WebBrowser webBrowser = (WebBrowser)sender)
		{
			(webBrowser.DocumentText ?? string.Empty).Contains("<pre");
			PixelFormat format = PixelFormat.Format24bppRgb;
			using Bitmap bitmap = new Bitmap(webBrowser.Width, webBrowser.Height, format);
			if (webBrowser.Document.DomDocument is IViewObject viewObject)
			{
				tagRECT lprcWBounds = new tagRECT
				{
					left = 0L,
					top = 0L,
					right = webBrowser.Width,
					bottom = webBrowser.Height
				};
				tagRECT lprcBounds = new tagRECT
				{
					left = 0L,
					top = 0L,
					right = webBrowser.Width,
					bottom = webBrowser.Height
				};
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					IntPtr hdc = graphics.GetHdc();
					try
					{
						viewObject.Draw(1u, -1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, hdc, ref lprcBounds, ref lprcWBounds, IntPtr.Zero, 0u);
					}
					finally
					{
						graphics.ReleaseHdc();
					}
				}
				BitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			}
		}
		WebBrowser = null;
		if (this.WebBrowserImageReady != null)
		{
			this.WebBrowserImageReady(this, new WebBrowserImageReadyEvengArgs(BitmapSource));
		}
	}

	public void Dispose()
	{
		if (WebBrowser != null)
		{
			WebBrowser.Dispose();
			WebBrowser = null;
		}
	}
}
