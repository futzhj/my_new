#define TRACE
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using my_new.log;
using my_new.utils;

namespace my_new;

public class WebBrowserOverLay : Window, IComponentConnector
{
	private FrameworkElement _placementTarget;

	private bool m_bInited;

	private DispatcherTimer _init_timer;

	private IntPtr C1;

	private DispatcherTimer m_timer_draw;

	internal Grid _base;

	internal System.Windows.Controls.WebBrowser _wb;

	internal System.Windows.Controls.Image img;

	private bool _contentLoaded;

	public bool IsInited
	{
		get
		{
			return m_bInited;
		}
		set
		{
			m_bInited = value;
			_init_timer = new DispatcherTimer();
			_init_timer.Interval = TimeSpan.FromSeconds(0.01);
			_init_timer.Tick += OnSizeLocationChanged;
			_init_timer.Start();
		}
	}

	public System.Windows.Controls.WebBrowser WebBrowser => _wb;

	public WebBrowserOverLay(FrameworkElement placementTarget, Window win)
	{
		InitializeComponent();
		_placementTarget = placementTarget;
		Window owner = win;
		owner.LocationChanged += delegate
		{
			OnSizeLocationChanged();
		};
		owner.IsVisibleChanged += delegate
		{
			OnSizeLocationChanged();
		};
		_placementTarget.SizeChanged += delegate
		{
			OnSizeLocationChanged();
		};
		if (owner.IsVisible)
		{
			base.Owner = owner;
			Show();
		}
		else
		{
			owner.IsVisibleChanged += delegate
			{
				if (owner.IsVisible)
				{
					base.Owner = owner;
					Show();
				}
			};
		}
		base.Opacity = 0.5;
		SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
	}

	private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
	{
		Console.WriteLine("DisplaySettingsChanged");
		OnSizeLocationChanged();
	}

	private void _DrawTimer(object sender, EventArgs e)
	{
		new RenderTargetBitmap((int)base.Width, (int)base.Height, 96.0, 96.0, PixelFormats.Pbgra32);
		((_FieldInfo)WebBrowser.GetType().GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic)).GetValue((object)WebBrowser);
		System.Windows.Forms.WebBrowser web_ = new System.Windows.Forms.WebBrowser();
		web_.Show();
		web_.Width = (int)base.Width;
		web_.Height = (int)base.Height;
		web_.Navigate("https://xyq.163.com/client/23v1.html");
		web_.DocumentCompleted += delegate
		{
			while (web_.ReadyState != WebBrowserReadyState.Complete)
			{
			}
			if (web_.ReadyState == WebBrowserReadyState.Complete)
			{
				Bitmap bitmap = new Bitmap(web_.Width, web_.Height);
				bitmap.Save("F:\\tools_other\\server\\t.png", ImageFormat.Png);
				bitmap = UtilsMethod.CaptureWindow(_wb.Handle);
				bitmap.Save("F:\\tools_other\\server\\tt.png", ImageFormat.Png);
				img.Source = UtilsMethod.ToBitmapSource(bitmap);
				_wb.Visibility = Visibility.Hidden;
			}
		};
		m_timer_draw.Stop();
		m_timer_draw = null;
	}

	private void Window_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			base.Topmost = false;
		}
	}

	private void SetDrawRect()
	{
		IntPtr handle = _wb.Handle;
		Win32API.DeleteObject(C1);
		Win32API.SetWindowRgn(handle, IntPtr.Zero, redraw: true);
		Rect rect = new Rect(new System.Windows.Size(base.ActualWidth, base.ActualHeight));
		C1 = Win32API.CreateRectRgn(0, 0, (int)rect.BottomRight.X, (int)rect.BottomRight.Y);
		Win32API.SetWindowRgn(handle, C1, redraw: true);
	}

	private void OnSizeLocationChanged(object sender = null, EventArgs e = null)
	{
		if (_init_timer != null)
		{
			_init_timer.Stop();
			_init_timer = null;
		}
		if (base.Owner == null)
		{
			return;
		}
		Trace.WriteLine($"RenderSize : {_placementTarget.RenderSize}");
		System.Windows.Point point = _placementTarget.TranslatePoint(default(System.Windows.Point), null);
		System.Windows.Point point2 = new System.Windows.Point(_placementTarget.ActualWidth, _placementTarget.ActualHeight);
		Trace.WriteLine($"Actual SIZE : {point2.ToString()}");
		HwndSource hwndSource = (HwndSource)PresentationSource.FromVisual(base.Owner);
		System.Windows.Point point3 = new System.Windows.Point(base.Owner.Left + point.X, base.Owner.Top + point.Y);
		if (hwndSource == null)
		{
			return;
		}
		HwndTarget compositionTarget = hwndSource.CompositionTarget;
		point = compositionTarget.TransformToDevice.Transform(point);
		System.Windows.Point point4 = point2;
		point2 = compositionTarget.TransformToDevice.Transform(point2);
		Trace.WriteLine($"SIZE : {point2.ToString()}");
		Win32API.POINT lpPoint = new Win32API.POINT(point);
		Win32API.ClientToScreen(hwndSource.Handle, ref lpPoint);
		Win32API.POINT lpPoint2 = new Win32API.POINT(point2);
		Win32API.ClientToScreen(hwndSource.Handle, ref lpPoint2);
		if (IsInited)
		{
			double num = 100.0;
			if (point2.X != point4.X)
			{
				num = point2.X / point4.X * (point2.Y / point4.Y) * 100.0;
			}
			_FieldInfo field = WebBrowser.GetType().GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field != null)
			{
				object value = field.GetValue(WebBrowser);
				if (value != null)
				{
					try
					{
						object[] args = new object[4]
						{
							63,
							0,
							(int)num,
							IntPtr.Zero
						};
						value.GetType().InvokeMember("ExecWB", BindingFlags.InvokeMethod, null, value, args);
					}
					catch (Exception ex)
					{
						LogAPI.LogWrite("try ExecWB Error");
						LogAPI.LogWrite(ex.Message);
					}
				}
			}
		}
		if ((HwndSource)PresentationSource.FromVisual(this) != null)
		{
			Trace.WriteLine("========= RESIZE ============");
			Trace.WriteLine("Location And Size: " + lpPoint.X + " " + lpPoint.Y + " " + lpPoint2.X + " " + lpPoint2.Y);
			base.Width = _placementTarget.ActualWidth;
			base.Height = _placementTarget.ActualHeight;
			base.Top = point3.Y;
			base.Left = point3.X;
		}
		if (base.Owner.IsVisible && base.Owner.WindowState != WindowState.Minimized)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		OnSizeLocationChanged();
	}

	private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		OnSizeLocationChanged();
	}

	private void Window_StateChanged(object sender, EventArgs e)
	{
		Console.WriteLine("web StateChanged");
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/my_new;component/webbrowseroverlay.xaml", UriKind.Relative);
			System.Windows.Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			((WebBrowserOverLay)target).MouseDown += Window_MouseDown;
			((WebBrowserOverLay)target).StateChanged += Window_StateChanged;
			((WebBrowserOverLay)target).Loaded += Window_Loaded;
			((WebBrowserOverLay)target).SizeChanged += Window_SizeChanged;
			break;
		case 2:
			_base = (Grid)target;
			break;
		case 3:
			_wb = (System.Windows.Controls.WebBrowser)target;
			break;
		case 4:
			img = (System.Windows.Controls.Image)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
