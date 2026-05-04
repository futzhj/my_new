using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shell;
using System.Windows.Threading;
using Microsoft.Win32;
using my_new.log;
using my_new.utils;

namespace my_new;

public class MainWindow : Window, IComponentConnector
{
	[ComImport]
	[Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IOleServiceProvider
	{
		[PreserveSig]
		int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
	}

	private launch m_Launch;

	private static MainWindow m_Instance;

	private WebBrowserOverLay m_web_window;

	private Window m_web_window2;

	private DispatcherTimer m_timer;

	private DispatcherTimer m_timer_draw;

	private bool m_first_draw = true;

	private ProcessBeforeClose m_close_event;

	private bool m_show = true;

	private WebBrowser m_web;

	private const int WM_USER = 1024;

	private const int MSG_CLIENT_LOADING_END = 11025;

	internal TaskbarItemInfo taskItem;

	internal Grid _base;

	internal Frame frame;

	internal Grid g_pre;

	internal System.Windows.Controls.Image img_frame;

	internal System.Windows.Controls.Image img0;

	private bool _contentLoaded;

	public static MainWindow Instance => m_Instance;

	public MainWindow()
	{
		LogAPI.LogWrite($"InitMainWindow");
		UtilsMethod.SetWebBrowserFeatures(11);
		m_show = true;
		InitializeComponent();
		double primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
		double primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
		double width = base.Width;
		double height = base.Height;
		base.Left = primaryScreenWidth / 2.0 - width / 2.0;
		base.Top = primaryScreenHeight / 2.0 - height / 2.0;
		if (GlobalVariable.g_ngp_port != 0 && UtilsMethod.ReadConfig("ngp_window", "0") == "0")
		{
			m_show = false;
		}
		if (!m_show)
		{
			base.ShowInTaskbar = false;
		}
		m_Instance = this;
		m_timer_draw = new DispatcherTimer();
		m_timer_draw.Interval = TimeSpan.FromMilliseconds(1000.0);
		m_timer_draw.Tick += _DrawTimer;
		m_timer_draw.Start();
		SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
		LogAPI.LogWrite($"InitMainWindowDone");
	}

	private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
	{
		double primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
		double primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
		double width = base.Width;
		double height = base.Height;
		base.Left = (primaryScreenWidth - width) / 2.0;
		base.Top = (primaryScreenHeight - height) / 2.0;
		LogAPI.LogWrite($"SystemEvents_DisplaySettingsChanged {(int)base.Width} {(int)base.Height}");
	}

	private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
	{
		if (msg == 11025)
		{
			m_Launch.OnClientLoadingEnd();
		}
		return IntPtr.Zero;
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		m_Launch?.ExitSelf();
	}

	public void CloseLater(TimeSpan time, ProcessBeforeClose processBeforeClose = null)
	{
		if (m_timer != null)
		{
			m_timer.Stop();
		}
		m_close_event = processBeforeClose;
		m_timer = new DispatcherTimer();
		m_timer.Interval = time;
		m_timer.Tick += _TimerTick;
		m_timer.Start();
	}

	private void _TimerTick(object sender, EventArgs e)
	{
		m_timer.Stop();
		m_timer = null;
		if (m_close_event != null)
		{
			m_close_event();
		}
		Close();
		m_Launch.Close();
		Environment.Exit(Environment.ExitCode);
	}

	private void _DrawTimer(object sender, EventArgs e)
	{
		if (m_first_draw)
		{
			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)m_Launch.ActualWidth, (int)m_Launch.ActualHeight, 96.0, 96.0, PixelFormats.Pbgra32);
			renderTargetBitmap.Render(m_Launch);
			img0.Source = renderTargetBitmap;
			m_first_draw = false;
		}
		if (m_web != null)
		{
			Bitmap bmp = UtilsMethod.CaptureWindow(m_web.Handle);
			img_frame.Source = UtilsMethod.ToBitmapSource(bmp);
		}
		System.Windows.Point point = img0.TranslatePoint(new System.Windows.Point(0.0, 0.0), this);
		double x = point.X;
		double y = point.Y;
		double right = base.Width - 270.0 - x;
		double bottom = base.Height - 160.0 - y;
		taskItem.ThumbnailClipMargin = new Thickness(x, y, right, bottom);
	}

	public static void ShowMessageBox(string msg, string title, MessageBoxButton msgType)
	{
		if (m_Instance != null)
		{
			MessageBox.Show(m_Instance, msg, title, msgType);
		}
	}

	protected override void OnSourceInitialized(EventArgs e)
	{
		base.OnSourceInitialized(e);
		m_Launch = new launch();
		m_Launch.Owner = this;
		m_Launch.window = this;
		m_Launch.Show();
		m_Launch.Top = base.Top;
		m_Launch.Left = base.Left;
		(PresentationSource.FromVisual(this) as HwndSource).AddHook(WndProc);
		double primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
		double primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
		double width = base.Width;
		double height = base.Height;
		base.Left = (primaryScreenWidth - width) / 2.0;
		base.Top = (primaryScreenHeight - height) / 2.0;
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		LogAPI.LogWrite($"Window_Loaded");
		string text = "https://xyq.163.com/client/24v1.html";
		WebBrowserOverLay webBrowserOverLay = new WebBrowserOverLay(frame, this);
		LogAPI.LogWrite($"Created WebBrowserOverLay");
		m_web_window = webBrowserOverLay;
		WebBrowser webBrowser = webBrowserOverLay.WebBrowser;
		m_Launch.Owner = webBrowserOverLay;
		m_web = webBrowser;
		webBrowser.Navigated += wbMain_Navigated;
		LogAPI.LogWrite($"WebBrowserOverLay Navigate {text}");
		try
		{
			webBrowser.Navigate(new Uri(text));
		}
		catch (Exception ex)
		{
			LogAPI.LogWrite($"WebBrowserOverLay Navigate Crash");
			LogAPI.LogWrite(string.Format(ex.Message));
		}
		LogAPI.LogWrite($"WebBrowserOverLay Navigate CallEnd");
	}

	private void Window_LocationChanged(object sender, EventArgs e)
	{
		if (m_Launch != null)
		{
			m_Launch.Top = base.Top + base.Height / 2.0 - m_Launch.Height / 2.0;
			m_Launch.Left = base.Left + base.Width / 2.0 - m_Launch.Width / 2.0;
		}
	}

	private void wbMain_Navigated(object sender, NavigationEventArgs e)
	{
		LogAPI.LogWrite($"WebBrowserOverLay wbMain_Navigated");
		SetSilent(m_web, silent: true);
		LogAPI.LogWrite($"WebBrowserOverLay SetSilent");
		m_web.ObjectForScripting = new CustomDocHandler();
		LogAPI.LogWrite($"WebBrowserOverLay SetIsInited");
		m_web_window.IsInited = true;
	}

	public static void SetSilent(WebBrowser browser, bool silent)
	{
		if (browser == null)
		{
			throw new ArgumentNullException("browser");
		}
		if (!(browser.Document is IOleServiceProvider oleServiceProvider))
		{
			return;
		}
		Guid guidService = new Guid("0002DF05-0000-0000-C000-000000000046");
		Guid riid = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");
		oleServiceProvider.QueryService(ref guidService, ref riid, out var ppvObject);
		if (ppvObject != null)
		{
			try
			{
				ppvObject.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, ppvObject, new object[1] { silent });
			}
			catch (Exception ex)
			{
				LogAPI.LogWrite("try Silent Error");
				LogAPI.LogWrite(ex.Message);
			}
		}
	}

	private void Window_StateChanged(object sender, EventArgs e)
	{
		if (m_Launch != null)
		{
			m_Launch.Top = base.Top + base.Height / 2.0 - m_Launch.Height / 2.0;
			m_Launch.Left = base.Left + base.Width / 2.0 - m_Launch.Width / 2.0;
		}
	}

	private void Window_Activated(object sender, EventArgs e)
	{
		m_Launch.WindowState = WindowState.Normal;
		base.WindowState = WindowState.Normal;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/my_new;component/mainwindow.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
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
			((MainWindow)target).Loaded += Window_Loaded;
			((MainWindow)target).LocationChanged += Window_LocationChanged;
			((MainWindow)target).StateChanged += Window_StateChanged;
			((MainWindow)target).Activated += Window_Activated;
			break;
		case 2:
			taskItem = (TaskbarItemInfo)target;
			break;
		case 3:
			_base = (Grid)target;
			break;
		case 4:
			frame = (Frame)target;
			break;
		case 5:
			g_pre = (Grid)target;
			break;
		case 6:
			img_frame = (System.Windows.Controls.Image)target;
			break;
		case 7:
			img0 = (System.Windows.Controls.Image)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
