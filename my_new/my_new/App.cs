using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Windows;
using my_new.log;
using my_new.net;
using my_new.utils;

namespace my_new;

public class App : Application
{
	private bool m_nArgs = true;

	private RemoteChecker remoteChecker;

	private bool _contentLoaded;

	private bool InitApp()
	{
		return true;
	}

	private void Application_Startup(object sender, StartupEventArgs e)
	{
		Updater.set_p1_xxsj_async(GlobalVariable.SJF_P1_START_MY_NEW_EXE);
		try
		{
			GlobalVariable.g_isRunningOnWine = Updater.is_inside_wine();
		}
		catch (Exception ex)
		{
			GlobalVariable.SetLogRootPath(".");
			LogAPI.LogRegistChannel(0, "gpatch.log", "GPatchTool");
			LogAPI.LogRegistChannelAdd(1, "click.log", "ClickLog");
			LogAPI.LogWrite(ex.Message);
			Shutdown();
		}
		if (!InitApp())
		{
			MessageBox.Show("程序初始化失败", "温馨提示", MessageBoxButton.OK);
			Shutdown();
			return;
		}
		HttpPoster.SendGet(ConstDef.SERVER_INI, "", "gbk");
		UtilsMethod.GetBrowserVersion();
		int cmdType = 0;
		if (GetNGPArg(e, ref cmdType, ref GlobalVariable.g_ngp_port) && OnNGPProcess(e, cmdType, GlobalVariable.g_ngp_port) == 0)
		{
			Shutdown();
			return;
		}
		int res = 0;
		int res2 = 0;
		int res3 = 1;
		int num = 0;
		GlobalVariable.g_subver = -1;
		GlobalVariable.g_switch_game_style = 0;
		GlobalVariable.g_autorun_arg = 0;
		if (m_nArgs && e.Args.Length >= 4 && e.Args[0] == GlobalVariable.SWITCH_ARG)
		{
			if (UtilsMethod.SaveToInt(e.Args[1], out var res4) && UtilsMethod.SaveToInt(e.Args[2], out var res5) && UtilsMethod.SaveToInt(e.Args[3], out res))
			{
				if (res5 < 1)
				{
					res5 = 1;
				}
				if (res5 > 2)
				{
					res5 = 2;
				}
				GlobalVariable.g_subver = res4;
				GlobalVariable.g_switch_game_style = res5;
				GlobalVariable.g_autorun_arg = res;
				m_nArgs = false;
			}
		}
		else if (m_nArgs && e.Args.Length >= 3 && e.Args[0] == GlobalVariable.AUTORUN_ARG && UtilsMethod.SaveToInt(e.Args[1], out res3) && UtilsMethod.SaveToInt(e.Args[2], out res2))
		{
			if (res3 != 1 && res3 != 2)
			{
				res3 = 1;
			}
			if (res2 != 0 && res2 != 1)
			{
				res2 = 0;
			}
			num = 255;
			GlobalVariable.g_autorun_arg = 0x1000000 | (num << 16) | (res2 << 8) | res3;
			m_nArgs = false;
		}
		if (!m_nArgs || e.Args.Length == 0)
		{
			GlobalVariable.SetLogRootPath(".");
			LogAPI.LogRegistChannel(0, "pre_gpatch.log", "GPatchTool");
		}
		if (!UtilsMethod.IsValidLaunchPath())
		{
			MessageBox.Show("当前目录不是《梦幻西游ONLINE》目录", "温馨提示", MessageBoxButton.OK);
			Shutdown();
			return;
		}
		if (!UtilsMethod.CheckMaxInstance())
		{
			Shutdown();
			return;
		}
		if (GlobalVariable.g_autorun_arg != 0 && (Convert.ToInt32(UtilsMethod.GetSettingEx("xy1.ini", "Net", "inner_test", "0")) & 1) != 0)
		{
			string settingEx = UtilsMethod.GetSettingEx("update.ini", "Setting", "UpdateURL", "http://update.xyq.163.com/patch_list15.txt?");
			string settingEx2 = UtilsMethod.GetSettingEx("xy1.ini", "Setting", "UpdateURL", settingEx);
			UtilsMethod.WriteSettingEx("update.ini", "Setting", "UpdateURL", settingEx2);
		}
		if (!m_nArgs || e.Args.Length == 0)
		{
			LogAPI.LogWrite("ERROR EXE BY");
			MessageBox.Show("请使用my.exe启动游戏", "温馨提示", MessageBoxButton.OK);
			Shutdown();
			return;
		}
		bool flag = false;
		if (e.Args.Length == 6 && e.Args[0] == GlobalVariable.LAUNCH_ARG && e.Args[1].Length > 2)
		{
			flag = true;
		}
		if (e.Args.Length == 7 && e.Args[0] == GlobalVariable.SATAT_NEW_MY_ARG && e.Args[1].Length > 2)
		{
			GlobalVariable.gPartentWnd = (IntPtr)Convert.ToInt32(e.Args[6]);
			flag = true;
		}
		if (!flag)
		{
			MessageBox.Show("请使用my.exe启动游戏", "温馨提示", MessageBoxButton.OK);
			Shutdown();
			return;
		}
		GlobalVariable.g_dst_path = (GlobalVariable.XY_PATH = UtilsMethod.NormalizePath(e.Args[1], isDir: true));
		GlobalVariable.SetLogRootPath(GlobalVariable.g_dst_path);
		LogAPI.LogRegistChannel(0, "gpatch.log", "GPatchTool");
		LogAPI.LogRegistChannelAdd(1, "click.log", "ClickLog");
		GlobalVariable.g_subver = Convert.ToInt32(e.Args[2]);
		GlobalVariable.g_switch_game_style = Convert.ToInt32(e.Args[3]);
		GlobalVariable.g_autorun_arg = Convert.ToInt32(e.Args[4]);
		GlobalVariable.g_ngp_port = Convert.ToInt32(e.Args[5]);
		if (GlobalVariable.g_isRunningOnWine && Updater.get_subclient_mode(GlobalVariable.g_dst_path) != 0)
		{
			Updater.set_subclient_mode(0, GlobalVariable.g_dst_path);
		}
		if (GlobalVariable.g_subver == -1 && Updater.get_subclient_mode(GlobalVariable.g_dst_path) != 0)
		{
			UtilsMethod.SaveToInt(UtilsMethod.GetSettingEx("client.ini", "Setting", "CurrentSubClient", "0"), out GlobalVariable.g_subver);
		}
		if (GlobalVariable.g_switch_game_style == 0)
		{
			try
			{
				if (Array.IndexOf(Directory.GetAccessControl("redata.wdf").AccessRightType.GetEnumValues(), FileSystemRights.Read) <= 0)
				{
					GlobalVariable.g_switch_game_style = 1;
				}
			}
			catch
			{
				GlobalVariable.g_switch_game_style = 0;
			}
		}
		if (GlobalVariable.g_ngp_port == -1)
		{
			GlobalVariable.g_ngp_port = 0;
		}
		GlobalVariable.g_restart = false;
		Directory.SetCurrentDirectory(GlobalVariable.XY_PATH);
		LogAPI.LogWrite("客户端路径: {0}", GlobalVariable.XY_PATH);
		LogAPI.LogWrite("客户端类型: {0}", "1");
		Application.Current.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
	}

	private bool GetNGPArg(StartupEventArgs e, ref int cmdType, ref int ngp_port)
	{
		if (e.Args.Length >= 2)
		{
			string text = e.Args[0];
			if (int.TryParse(e.Args[1], out var result))
			{
				if (text == GlobalVariable.GAME_PLATFORM_CHECK_UPDATE_ARG)
				{
					cmdType = 0;
					ngp_port = result;
					return true;
				}
				if (text == GlobalVariable.GAME_PLATFORM_UPDATE_ARG)
				{
					if (result == 0)
					{
						return false;
					}
					m_nArgs = true;
					cmdType = 1;
					ngp_port = result;
					return true;
				}
				if (text == GlobalVariable.GAME_PLATFORM_LAUNCH_ARG)
				{
					if (result == 0)
					{
						return false;
					}
					cmdType = 2;
					ngp_port = result;
				}
			}
		}
		return false;
	}

	private int OnNGPProcess(StartupEventArgs e, int cmdType, int ngp_port)
	{
		GlobalVariable.XY_PATH = UtilsMethod.NormalizePath(Directory.GetCurrentDirectory(), isDir: true);
		GlobalVariable.g_dst_path = GlobalVariable.XY_PATH;
		GlobalVariable.SetLogRootPath(GlobalVariable.XY_PATH);
		LogAPI.LogRegistChannel(0, "gpatch.log", "GPatchTool");
		switch (cmdType)
		{
		case 0:
			OnNGPCheckUpdate();
			return 0;
		case 1:
			return 1;
		case 2:
			return OnNGPLaunch(e, ngp_port);
		default:
			return 0;
		}
	}

	private void OnNGPCheckUpdate()
	{
		NetServer.GetClientNeedUpdateSize();
		Environment.Exit(0);
	}

	private bool OpenExeFile(string exe_name, string args)
	{
		if (args.Length > 0)
		{
			_ = $"{exe_name} {args}";
		}
		Process process = new Process();
		process.StartInfo.FileName = exe_name;
		process.StartInfo.Arguments = args;
		process.StartInfo.WorkingDirectory = GlobalVariable.XY_PATH;
		bool result = false;
		try
		{
			result = process.Start();
		}
		catch
		{
		}
		return result;
	}

	private int OnNGPLaunch(StartupEventArgs e, int port)
	{
		string text = UtilsMethod.ReadConfig("launchtype", "0");
		string args;
		if (!GlobalVariable.g_isRunningOnWine && text == "0")
		{
			args = "___only_for_xyq_welcome_start_game___ " + port;
			OpenExeFile("mhtab.exe", args);
			return 0;
		}
		args = "__start_by_mh_launcher__ 0 " + port;
		OpenExeFile("mhmain.exe", args);
		return 0;
	}

	private void OnLoading()
	{
		GlobalVariable.XY_PATH = Directory.GetCurrentDirectory();
		if (!GlobalVariable.XY_PATH.EndsWith("\\"))
		{
			GlobalVariable.XY_PATH += "\\";
		}
		if (GlobalVariable.g_ngp_port != 0)
		{
			NetServer.ExecuteCopyProcess();
			return;
		}
		LogAPI.LogWrite("RemoteChecker window");
		Application.Current.StartupUri = new Uri("RemoteChecker.xaml", UriKind.Relative);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			base.Startup += Application_Startup;
			Uri resourceLocator = new Uri("/my_new;component/app.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[STAThread]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public static void Main()
	{
		App app = new App();
		app.InitializeComponent();
		app.Run();
	}
}
