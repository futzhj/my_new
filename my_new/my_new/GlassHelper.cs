using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace my_new;

public class GlassHelper
{
	public static bool ExtendGlassFrame(Window window, Thickness margin)
	{
		if (!DwmIsCompositionEnabled())
		{
			return false;
		}
		IntPtr handle = new WindowInteropHelper(window).Handle;
		if (handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("The Window must be shown before extending glass.");
		}
		window.Background = Brushes.Transparent;
		HwndSource.FromHwnd(handle).CompositionTarget.BackgroundColor = Colors.Transparent;
		MARGINS margins = new MARGINS(margin);
		DwmExtendFrameIntoClientArea(handle, ref margins);
		return true;
	}

	[DllImport("dwmapi.dll", PreserveSig = false)]
	private static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

	[DllImport("dwmapi.dll", PreserveSig = false)]
	private static extern bool DwmIsCompositionEnabled();
}
