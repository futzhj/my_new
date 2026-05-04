using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using my_new.log;
using my_new.net;
using my_new.utils;

namespace my_new;

public class RemoteChecker : Window, IComponentConnector
{
	private Task m_copy_exe;

	private WndProcFunc wndProcFunc;

	private Timer m_timer;

	internal TextBlock messageLbl;

	private bool _contentLoaded;

	public void SetMessage(string str)
	{
		messageLbl.Text = str;
	}

	public void Center()
	{
	}

	public void SetWndProcFunc(WndProcFunc wndProcFunc)
	{
		this.wndProcFunc = wndProcFunc;
	}

	public void CopyExe()
	{
		uint result = NetServer.ExecuteCopyProcess();
		LogAPI.LogWrite("copy res {0}!!", result.ToString());
		Application.Current.Dispatcher.Invoke((Action)delegate
		{
			onFinishCopyExe(result);
		}, new object[0]);
	}

	public void onFinishCopyExe(uint copy_result)
	{
		m_copy_exe = null;
		if (copy_result == 1)
		{
			LogAPI.LogWrite("启动程序失败，error code:{0}", Marshal.GetLastWin32Error().ToString());
			UtilsMethod.ShowNGPMessageBox("启动《梦幻西游 ONLINE》失败!", "温馨提醒", MessageBoxButton.OK);
			Close();
			Environment.Exit(1);
		}
		else
		{
			Close();
			Environment.Exit(Environment.ExitCode);
		}
	}

	public RemoteChecker()
	{
		InitializeComponent();
		LogAPI.LogWrite("starting copy!!");
		m_timer = new Timer(TimerCallback, null, 1000, -1);
	}

	public void TimerCallback(object state)
	{
		m_copy_exe = new Task(CopyExe);
		m_copy_exe.Start();
		m_timer = null;
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		if (m_copy_exe != null)
		{
			m_copy_exe = null;
		}
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		m_copy_exe = null;
		if (m_timer != null)
		{
			m_timer.Change(-1, -1);
			m_timer = null;
		}
		Close();
		Environment.Exit(Environment.ExitCode);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/my_new;component/remotechecker.xaml", UriKind.Relative);
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
			((RemoteChecker)target).Closing += Window_Closing;
			break;
		case 2:
			messageLbl = (TextBlock)target;
			break;
		case 3:
			((Button)target).Click += Button_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
