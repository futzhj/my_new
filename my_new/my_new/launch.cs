using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using myRes;
using my_new.log;
using my_new.net;
using my_new.utils;

namespace my_new;

public class launch : Window, IComponentConnector
{
	private Window m_Wind;

	private System.Windows.Controls.WebBrowser webBrowser;

	private static launch m_Instance = null;

	private Task m_task_update;

	private Task m_task_send_info;

	private object m_lock_object = new object();

	private int m_partProgressCount = 1;

	private int m_curPartProgressIdx;

	private float m_real_prog;

	private bool m_client_launched;

	private bool m_error;

	private string m_cookie;

	private int m_resolution = -1;

	private bool m_resolution_changed;

	private bool m_disableIE;

	private bool m_show_wallpaper;

	private int m_text_index;

	private Process m_pGame;

	private System.Threading.Timer m_timer_launchclient;

	private SpineController m_spine;

	private DispatcherTimer m_text_timer;

	private List<List<System.Windows.Controls.Label>> m_labels = new List<List<System.Windows.Controls.Label>>();

	private List<DispatcherOperation> m_dispatcherOperations = new List<DispatcherOperation>();

	private bool m_enable_dispatcher = true;

	private FrameSequence m_frame_test;

	private PatchUpdater m_updater;

	private static readonly Dictionary<int, string> resolution_desc = new Dictionary<int, string>
	{
		{ 0, "640" },
		{ 1, "800" },
		{ 2, "1024" },
		{ 3, "1024HD" },
		{ 4, "1280HD" }
	};

	internal Grid _base;

	internal Grid Web;

	internal Image img_banner;

	internal Image img_LabelBg;

	internal Border webHint;

	internal Grid JuanZhou;

	internal Image img_juanzhoudi;

	internal Image img_progress;

	internal System.Windows.Controls.ProgressBar prog_download;

	internal Image img_progressHead;

	internal TranslateTransform mask_x;

	internal Grid Spine;

	internal Image img_Door;

	internal Canvas SpineCanvas;

	internal Grid Player;

	internal Image img_juese;

	internal Grid UI;

	internal CustomImageButton btn_chongzhi;

	internal CustomImageButton btn_shezhi;

	internal CustomImageButton btn_zhuce;

	internal CustomImageButton btn_guanwang;

	internal CustomImageButton btn_sign;

	internal CustomImageButton btn_loading;

	internal Rectangle testRect;

	internal Image img_loading;

	internal System.Windows.Controls.Label lbl_loading;

	internal Image img_logodi;

	internal Image img_logo;

	internal Image img_wylogo1;

	internal CustomImageButton img_wangyilogo;

	internal CustomImageButton btn_close;

	internal CustomImageButton btn_min;

	internal System.Windows.Controls.Label lbl_huoquziyuanbao;

	internal System.Windows.Controls.Label lbl_daxiao;

	internal System.Windows.Controls.Label lbl_xiazaisudu;

	internal System.Windows.Controls.Label lbl_shengyushijian;

	internal TextBlock lbl_wenti;

	internal System.Windows.Controls.Label lbl_fangchenmi1;

	internal System.Windows.Controls.Label lbl_fangchenmi2;

	internal System.Windows.Controls.Label lbl_fangchenmi3;

	internal System.Windows.Controls.Label lbl_fangchenmi4;

	internal System.Windows.Controls.Label lbl_fangchenmi5;

	internal System.Windows.Controls.Label lbl_fangchenmi6;

	internal System.Windows.Controls.Label lbl_fangchenmi7;

	internal System.Windows.Controls.Label lbl_fangchenmi8;

	internal CustomImageButton img_ageTips;

	private bool _contentLoaded;

	public Window window
	{
		get
		{
			return m_Wind;
		}
		set
		{
			m_Wind = value;
		}
	}

	public static launch Instance => m_Instance;

	public launch()
	{
		Updater.set_p1_xxsj_async(GlobalVariable.SJF_P1_INIT_MY_EXE);
		InitializeComponent();
		m_text_index = 0;
		m_disableIE = UtilsMethod.ReadConfig("disableie", "0") == "0";
		string text = UtilsMethod.ReadConfig("wallpaper", "");
		if (text.Length == 0)
		{
			UtilsMethod.WriteConfig("wallpaper", "3");
			m_show_wallpaper = true;
		}
		else
		{
			int result = 0;
			if (int.TryParse(text, out result))
			{
				if (result > 1)
				{
					result--;
					UtilsMethod.WriteConfig("wallpaper", $"{result}");
					m_show_wallpaper = true;
				}
				else
				{
					m_show_wallpaper = false;
				}
			}
			else
			{
				m_show_wallpaper = false;
			}
		}
		GetResolutionFromLastedSubClient();
		if (m_resolution == -1)
		{
			string text2 = UtilsMethod.ReadConfig($"resolution{GetGameStyle()}", "");
			if (text2.Length != 0)
			{
				m_resolution = Convert.ToInt32(text2);
				if (m_resolution < 0)
				{
					m_resolution = 0;
				}
				if (m_resolution > 4)
				{
					m_resolution = 4;
				}
				SyncResolutionToSubClient();
			}
		}
		m_labels.Add(new List<System.Windows.Controls.Label> { lbl_fangchenmi1, lbl_fangchenmi2 });
		m_labels.Add(new List<System.Windows.Controls.Label> { lbl_fangchenmi3, lbl_fangchenmi4 });
		m_labels.Add(new List<System.Windows.Controls.Label> { lbl_fangchenmi5 });
		m_labels.Add(new List<System.Windows.Controls.Label> { lbl_fangchenmi6 });
		m_labels.Add(new List<System.Windows.Controls.Label> { lbl_fangchenmi7, lbl_fangchenmi8 });
		foreach (List<System.Windows.Controls.Label> label in m_labels)
		{
			foreach (System.Windows.Controls.Label item in label)
			{
				item.Visibility = Visibility.Hidden;
			}
		}
		foreach (FrameworkElement item2 in new List<FrameworkElement> { lbl_huoquziyuanbao, lbl_daxiao, lbl_xiazaisudu, lbl_shengyushijian, lbl_wenti })
		{
			item2.Visibility = Visibility.Hidden;
		}
		img_progress.Visibility = Visibility.Hidden;
		prog_download.Value = 0.0;
		_SetSignEnable(b: true);
		m_text_timer = new DispatcherTimer();
		m_text_timer.Interval = TimeSpan.FromSeconds(3.0);
		m_text_timer.Tick += _OnTextTimerTick;
		m_text_timer.Start();
		m_frame_test = new FrameSequence(testRect, "登录按钮流光/anim_流光", 15f);
		testRect.Visibility = Visibility.Hidden;
		_StartDownloadPatchForUI();
		_LoadStyle();
		m_Instance = this;
		m_error = false;
		m_updater = new PatchUpdater(this);
		m_updater.FinishEvent += _OnDownloadFinished;
		m_updater.WarmupEvent += _OnWarmup;
		m_updater.Start();
		LogAPI.LogWrite("Start Update");
		SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
		UtilsMethod.onStartSuccess();
		Updater.set_p1_xxsj_async(GlobalVariable.SJF_P1_INIT_MY_EXE_DONE);
	}

	public string FindLatestVersionFile(List<string> directories, string filename)
	{
		DateTime dateTime = DateTime.MinValue;
		string text = "";
		foreach (string directory in directories)
		{
			string path = System.IO.Path.Combine(directory, filename);
			if (File.Exists(path))
			{
				DateTime lastWriteTime = File.GetLastWriteTime(path);
				if (string.IsNullOrEmpty(text) || lastWriteTime > dateTime)
				{
					dateTime = lastWriteTime;
					text = directory;
				}
			}
		}
		return text;
	}

	public void GetResolutionFromLastedSubClient()
	{
		int num = Updater.get_subclient_mode(GlobalVariable.g_dst_path);
		if (num <= 0)
		{
			return;
		}
		string g_dst_path = GlobalVariable.g_dst_path;
		List<string> list = new List<string> { g_dst_path };
		int num2 = 1;
		while (num != 0)
		{
			int num3 = num2 * (num & 1);
			if (num3 != 0)
			{
				char[] array = new char[256];
				Updater.get_subclient_path(GlobalVariable.g_dst_path, num3, array);
				string text = UtilsMethod.OutputToString(array);
				if (text != g_dst_path)
				{
					list.Add(text);
				}
			}
			num >>= 1;
			num2++;
			if (num2 > 2)
			{
				break;
			}
		}
		if (list.Count <= 1)
		{
			return;
		}
		foreach (string item in new List<string> { "xy1.ini" })
		{
			string text2 = FindLatestVersionFile(list, item);
			if (!string.IsNullOrEmpty(text2) && text2 != g_dst_path)
			{
				string key = $"resolution{1}";
				string text3 = UtilsMethod.ReadConfigEx(key, "", text2);
				if (!string.IsNullOrEmpty(text3) && int.TryParse(text3, out var result) && result >= 0 && result <= 4)
				{
					string text4 = result.ToString();
					UtilsMethod.WriteConfig(key, text4);
					string strValue = ((result == 3 || result == 4) ? "1.25" : "1.0");
					UtilsMethod.WriteSettingEx("xy3.ini", "Net", "high_definition", strValue);
					LogAPI.ClickLogWrite("sync_resolution_from:{0}, resolution:{1}", text2, text4);
				}
			}
		}
	}

	public void SyncResolutionToSubClient()
	{
		int num = Updater.get_subclient_mode(GlobalVariable.g_dst_path);
		if (num <= 0)
		{
			return;
		}
		string g_dst_path = GlobalVariable.g_dst_path;
		List<string> list = new List<string>();
		int num2 = 1;
		while (num != 0)
		{
			int num3 = num2 * (num & 1);
			if (num3 != 0 && num3 != 10)
			{
				char[] array = new char[256];
				Updater.get_subclient_path(GlobalVariable.g_dst_path, num3, array);
				string text = UtilsMethod.OutputToString(array);
				if (text != g_dst_path)
				{
					list.Add(text);
				}
			}
			num >>= 1;
			num2++;
		}
		if (list.Count == 0)
		{
			return;
		}
		string key = $"resolution{1}";
		string val = $"{m_resolution}";
		string val2 = ((m_resolution == 3 || m_resolution == 4) ? "1.25" : "1.0");
		foreach (string item in list)
		{
			UtilsMethod.WriteConfigEx(item, "xy1.ini", "Setting", key, val);
			UtilsMethod.WriteConfigEx(item, "xy3.ini", "Net", "high_definition", val2);
		}
	}

	private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
	{
		double primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
		double primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
		double width = base.Width;
		double height = base.Height;
		base.Left = (primaryScreenWidth - width) / 2.0;
		base.Top = (primaryScreenHeight - height) / 2.0;
		LogAPI.LogWrite($"SystemEvents_DisplaySettingsChanged launch {(int)base.Width} {(int)base.Height}");
	}

	private void _LoadStyle()
	{
		ResourceDictionary resourceDictionary = new ResourceDictionary();
		resourceDictionary.Source = new Uri("/myRes;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute);
		img_wangyilogo.NormalImage = (ImageBrush)resourceDictionary[StaticResource.KefuNormalBrushKey];
		img_wangyilogo.HoverImage = (ImageBrush)resourceDictionary[StaticResource.KefuHoverBrushKey];
		img_wangyilogo.PressedImage = (ImageBrush)resourceDictionary[StaticResource.KefuPressedBrushKey];
		img_Door.Source = new BitmapImage(new Uri("pack://application:,,,/myRes;Component/Images/img_背景房子.png"));
		Thickness margin = img_juese.Margin;
		try
		{
			StaticResource.SetRoleImage(img_juese);
		}
		catch
		{
			img_juese.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/roles/img_骨精灵.png"));
			img_juese.Margin = margin;
		}
	}

	public int GetGameStyle()
	{
		return 1;
	}

	public void ExitSelf(ProcessBeforeClose processBeforeClose = null)
	{
		if (m_task_send_info != null && !m_task_send_info.IsCompleted)
		{
			Task.WaitAny(new Task[1] { m_task_send_info }, 1000);
			m_task_send_info = null;
		}
		m_enable_dispatcher = false;
		if (m_dispatcherOperations.Count > 0)
		{
			foreach (DispatcherOperation dispatcherOperation in m_dispatcherOperations)
			{
				dispatcherOperation?.Abort();
			}
		}
		if (m_updater.isDownLoading())
		{
			m_updater.WaitForEnd();
		}
		m_updater.WaitForWarmupEnd();
		if (m_pGame != null)
		{
			m_pGame.Kill();
		}
		if (m_spine != null)
		{
			m_spine.Release();
			m_spine = null;
		}
		if (m_text_timer != null)
		{
			m_text_timer.Stop();
		}
		m_Wind.Hide();
		Hide();
		((MainWindow)m_Wind).CloseLater(TimeSpan.FromMilliseconds(1000.0), processBeforeClose);
	}

	public static MessageBoxResult ShowMyMessageBox(string msg, string tilte, MessageBoxButton messageBoxButton, Window ownerWindow = null)
	{
		if (ownerWindow != null)
		{
			return System.Windows.MessageBox.Show(ownerWindow, msg, tilte, messageBoxButton);
		}
		if (m_Instance != null)
		{
			return System.Windows.MessageBox.Show(m_Instance, msg, tilte, messageBoxButton);
		}
		return System.Windows.MessageBox.Show(msg, tilte, messageBoxButton);
	}

	private bool _OpenExeFile(string filename, string args = "", bool isGameHandle = false, string workdir = "")
	{
		Process process = new Process();
		process.StartInfo.FileName = filename;
		process.StartInfo.Arguments = args;
		process.StartInfo.WorkingDirectory = workdir ?? GlobalVariable.XY_PATH;
		bool flag = false;
		try
		{
			flag = process.Start();
		}
		catch
		{
			flag = false;
		}
		if (isGameHandle)
		{
			m_pGame = process;
		}
		if (!flag)
		{
			ShowMyMessageBox("启动程序时失败", "梦幻西游更新程序", MessageBoxButton.OK);
		}
		return flag;
	}

	private bool _OpenClientExe(string filename, string arg, bool isGameHandle = false)
	{
		if (GlobalVariable.g_subver > 0 && !GlobalVariable.g_isRunningOnWine)
		{
			UtilsMethod.SyncXyConfig(GlobalVariable.g_subver);
			char[] array = new char[256];
			Updater.get_subclient_path(GlobalVariable.g_dst_path, GlobalVariable.g_subver, array);
			string text = UtilsMethod.OutputToString(array);
			string filename2 = text + "/" + filename;
			UtilsMethod.WriteSettingEx("client.ini", "Setting", "CurrentSubClient", GlobalVariable.g_subver.ToString());
			return _OpenExeFile(filename2, arg, isGameHandle, text);
		}
		UtilsMethod.WriteSettingEx("client.ini", "Setting", "CurrentSubClient", "0");
		return _OpenExeFile(filename, arg, isGameHandle);
	}

	public void Restart()
	{
		ProcessBeforeClose processBeforeClose = delegate
		{
			if (m_updater.NeedRestartOther())
			{
				_OpenExeFile(GlobalVariable.XY_PATH + "\\" + GlobalVariable.g_other_client_name + "\\my.exe", "", isGameHandle: false, GlobalVariable.XY_PATH + "\\" + GlobalVariable.g_other_client_name);
			}
			else
			{
				_OpenExeFile("my.exe");
			}
		};
		ExitSelf(processBeforeClose);
	}

	public void LaunchAutoRunClient(uint arg, int tabtype)
	{
		SetDlgPatchStatus("正在启动客户端...", thread: false);
		_StartLoadingGameForUI();
		if (tabtype != 0 && tabtype != 1)
		{
			tabtype = Convert.ToInt32(UtilsMethod.ReadConfig("launchtype", "0"));
		}
		if (tabtype == 0 && !GlobalVariable.g_isRunningOnWine)
		{
			string arg2 = $"{GlobalVariable.AUTORESTART_ARG} {arg}";
			if (_OpenClientExe("mhtab.exe", arg2))
			{
				m_client_launched = true;
				ExitSelf();
			}
			else
			{
				_ResetLoadingGameForUI();
				SetDlgPatchStatus("启动客户端失败，请重试", thread: false);
			}
		}
		else
		{
			string arg2 = $"{GlobalVariable.AUTORUN_ARG} {arg}";
			if (_OpenClientExe("mhmain.exe", arg2))
			{
				m_client_launched = true;
				ExitSelf();
			}
			else
			{
				_ResetLoadingGameForUI();
				SetDlgPatchStatus("启动客户端失败，请重试", thread: false);
			}
		}
	}

	public void LaunchClient(int gameStyle)
	{
		SetDlgPatchStatus("正在启动客户端...", thread: false);
		_StartLoadingGameForUI();
		if (m_task_send_info == null)
		{
			m_cookie = _GetCookie();
			m_task_send_info = new Task(_SendMacInfo);
			m_task_send_info.Start();
		}
		string text = UtilsMethod.ReadConfig("launchtype", "0");
		string text2 = "";
		if (text == "0")
		{
			text2 = "___only_for_xyq_welcome_start_game___ 0";
			if (_OpenClientExe("mhtab.exe", text2))
			{
				m_client_launched = true;
				ExitSelf();
			}
			else
			{
				_ResetLoadingGameForUI();
				SetDlgPatchStatus("启动客户端失败，请重试", thread: false);
			}
			return;
		}
		IntPtr handle = new WindowInteropHelper(m_Wind).Handle;
		text2 = $"__start_by_mh_launcher__ {handle} 0";
		if (_OpenClientExe("mhmain.exe", text2, isGameHandle: true))
		{
			m_client_launched = true;
			m_timer_launchclient = new System.Threading.Timer(_CheckClientLaunch, null, 100, -1);
		}
		else
		{
			_ResetLoadingGameForUI();
			SetDlgPatchStatus("启动客户端失败，请重试", thread: false);
		}
	}

	public void OnClientLoadingEnd()
	{
		m_pGame = null;
		if (m_timer_launchclient != null)
		{
			m_timer_launchclient.Change(-1, -1);
		}
		ExitSelf();
	}

	private void _SetSignEnable(bool b)
	{
		if (b)
		{
			btn_sign.Visibility = Visibility.Visible;
			btn_loading.Visibility = Visibility.Hidden;
			img_loading.Visibility = Visibility.Hidden;
			lbl_loading.Visibility = Visibility.Hidden;
		}
		else
		{
			btn_sign.Visibility = Visibility.Hidden;
			btn_loading.Visibility = Visibility.Visible;
			img_loading.Visibility = Visibility.Visible;
			lbl_loading.Visibility = Visibility.Visible;
		}
	}

	private void _StartLoadingGameForUI()
	{
		_SetSignEnable(b: false);
		btn_shezhi.IsEnabled = false;
		lbl_huoquziyuanbao.Visibility = Visibility.Hidden;
		lbl_daxiao.Visibility = Visibility.Hidden;
		lbl_wenti.Visibility = Visibility.Hidden;
	}

	private void _ResetLoadingGameForUI()
	{
		_SetSignEnable(b: true);
		btn_shezhi.IsEnabled = true;
		lbl_huoquziyuanbao.Visibility = Visibility.Hidden;
		lbl_daxiao.Visibility = Visibility.Hidden;
		lbl_wenti.Visibility = Visibility.Hidden;
	}

	private void _StartDownloadPatchForUI()
	{
		_SetSignEnable(b: false);
		btn_shezhi.IsEnabled = true;
		lbl_wenti.Visibility = Visibility.Visible;
	}

	private void _FinishDownloadPatchForUI()
	{
		_SetSignEnable(b: true);
		btn_shezhi.IsEnabled = true;
		lbl_huoquziyuanbao.Visibility = Visibility.Hidden;
		lbl_daxiao.Visibility = Visibility.Hidden;
		lbl_wenti.Visibility = Visibility.Hidden;
	}

	public void SetDlgPatchStatus(string info, bool thread)
	{
		Action action = delegate
		{
			if (info.Length > 0)
			{
				_ShowTextNow(-1);
				if (info.IndexOf("kB/s") >= 0)
				{
					lbl_shengyushijian.Content = info;
					lbl_xiazaisudu.Visibility = Visibility.Hidden;
					lbl_shengyushijian.Visibility = Visibility.Visible;
				}
				else
				{
					lbl_xiazaisudu.Content = info;
					lbl_xiazaisudu.Visibility = Visibility.Visible;
					lbl_shengyushijian.Visibility = Visibility.Hidden;
				}
			}
			else
			{
				lbl_xiazaisudu.Visibility = Visibility.Hidden;
				lbl_shengyushijian.Visibility = Visibility.Hidden;
				_ShowTextNow(0);
			}
		};
		if (thread)
		{
			_AddASyncAction(action);
		}
		else
		{
			action();
		}
	}

	public void SetMainProgress(float prog, bool thread)
	{
		m_real_prog = prog;
		Action action = delegate
		{
			if (m_real_prog >= 0f && m_real_prog <= 100f)
			{
				if (prog <= 0.001f)
				{
					img_progress.Visibility = Visibility.Hidden;
					prog_download.Value = 0.0;
					img_progressHead.Visibility = Visibility.Hidden;
				}
				else if (m_real_prog <= 10f)
				{
					img_progress.Visibility = Visibility.Visible;
					prog_download.Value = 0.0;
					img_progressHead.Visibility = Visibility.Hidden;
				}
				else
				{
					img_progress.Visibility = Visibility.Visible;
					prog_download.Value = (m_real_prog - 10f) * 1000f / 90f;
					img_progressHead.Visibility = Visibility.Visible;
					Thickness margin = img_progressHead.Margin;
					img_progressHead.Margin = new Thickness(790f * (m_real_prog - 10f) / 90f + 19f, margin.Top, 0.0, 0.0);
					mask_x.X = 182f - 790f * (m_real_prog - 10f) / 90f;
				}
				lbl_loading.Content = (int)m_real_prog + "%";
			}
		};
		if (thread)
		{
			_AddASyncAction(action);
		}
		else
		{
			action();
		}
	}

	public void SetCurrentPartProgress(int idx)
	{
		m_curPartProgressIdx = idx;
	}

	public void SetDlgPatchName(string s, bool thread)
	{
		Action action = delegate
		{
			lbl_huoquziyuanbao.Visibility = Visibility.Visible;
			lbl_huoquziyuanbao.Content = s;
		};
		if (thread)
		{
			_AddASyncAction(action);
		}
		else
		{
			action();
		}
	}

	public void SetDlgPatchSize(string s, bool thread)
	{
		Action action = delegate
		{
			lbl_daxiao.Visibility = Visibility.Visible;
			lbl_daxiao.Content = s;
		};
		if (thread)
		{
			_AddASyncAction(action);
		}
		else
		{
			action();
		}
	}

	public void SetPartProgress(int progress, bool thread)
	{
		float num = 100f / (float)m_partProgressCount;
		float num2 = num * (float)m_curPartProgressIdx + num * (float)progress / 100f;
		GlobalVariable.g_UpdateDetail.progress = (int)num2;
		SetMainProgress(num2, thread);
	}

	public void SetPartProgressCount(int count)
	{
		m_partProgressCount = count;
	}

	private void _OnDownloadFinished(int returncode)
	{
		LogAPI.LogWrite("OnDownloadFinished");
		_FinishDownloadPatchForUI();
		if (Updater.need_warm_up())
		{
			m_updater.StartWarmup();
		}
		m_updater.WaitForEnd();
		m_error |= m_updater.GetError();
		if (!m_error)
		{
			SetMainProgress(100f, thread: false);
			SetDlgPatchStatus("客户端已经是最新版本。", thread: false);
			GlobalVariable.g_UpdateDetail.status = "finished";
			if (m_updater.NeedRestart())
			{
				if (Updater.get_subclient_mode(GlobalVariable.g_dst_path) > 0)
				{
					UtilsMethod.WriteSettingEx("client.ini", "Setting", "CurrentSubClient", GlobalVariable.g_subver.ToString());
				}
				Restart();
			}
			else if (GlobalVariable.g_autorun_arg != 0)
			{
				int tabtype = (GlobalVariable.g_autorun_arg >> 8) & 0xFF;
				int num = (GlobalVariable.g_autorun_arg & 0xFF) - 1;
				if (num != 0 && num != 1)
				{
					num = 0;
				}
				uint arg = (uint)((GlobalVariable.g_autorun_arg & -65536) | num);
				LaunchAutoRunClient(arg, tabtype);
			}
			else if (GlobalVariable.g_switch_game_style != 0)
			{
				if (Updater.get_subclient_mode(GlobalVariable.g_dst_path) > 0)
				{
					int tabtype2 = (GlobalVariable.g_switch_game_style >> 8) & 0xFF;
					LaunchAutoRunClient((uint)GlobalVariable.g_switch_game_style, tabtype2);
				}
				else
				{
					LaunchClient(GlobalVariable.g_switch_game_style);
				}
			}
			SetDlgPatchStatus("", thread: false);
		}
		else
		{
			LogAPI.LogWrite("更新发生错误");
			if (ShowMyMessageBox("更新发生错误，是否重试？取消将退出游戏", "梦幻西游更新程序", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
			{
				m_error = false;
				m_updater.Start();
			}
			else
			{
				ExitSelf();
			}
		}
		if (UtilsMethod.NeedOpenWallpaper())
		{
			_OpenExeFile("wallpaper\\XYQWallPaper.exe");
		}
	}

	private void _OnWarmup(int prog)
	{
		if (prog >= 100)
		{
			SetDlgPatchStatus("", thread: false);
			SetMainProgress(100f, thread: false);
			LogAPI.LogWrite("OnWarmup done");
		}
		else
		{
			string text = $"检测到机械硬盘，正在缓存数据至内存以提升流畅度…{prog}%";
			SetDlgPatchStatus(text, thread: false);
			SetMainProgress(prog, thread: false);
			LogAPI.LogWrite("OnWarmup:" + text);
		}
	}

	private void _Play()
	{
		int gameStyle = GetGameStyle();
		if (m_resolution == -1)
		{
			SettingDialog settingDialog = new SettingDialog(m_resolution, Updater.get_subclient_mode(GlobalVariable.g_dst_path));
			settingDialog.SwitchTab(1);
			settingDialog.Owner = window;
			settingDialog.ShowDialog();
			if (settingDialog.CloseSelf)
			{
				_TryEixtWallpaper();
				ExitSelf();
				return;
			}
			m_resolution_changed = settingDialog.Resolution != m_resolution;
			m_resolution = settingDialog.Resolution;
			if (m_resolution_changed)
			{
				string key = $"resolution{GetGameStyle()}";
				string val = m_resolution.ToString();
				UtilsMethod.WriteConfig(key, val);
				string strValue = ((m_resolution == 3 || m_resolution == 4) ? "1.25" : "1.0");
				UtilsMethod.WriteSettingEx("xy3.ini", "Net", "high_definition", strValue);
				SyncResolutionToSubClient();
			}
			if (settingDialog.SubclientMode != Updater.get_subclient_mode(GlobalVariable.g_dst_path))
			{
				Updater.set_subclient_mode(settingDialog.SubclientMode, GlobalVariable.g_dst_path);
				Restart();
				return;
			}
		}
		if (m_resolution_changed)
		{
			string key2 = $"resolution{gameStyle}";
			string val2 = $"{m_resolution}";
			UtilsMethod.WriteConfig(key2, val2);
			string strValue2 = ((m_resolution == 3 || m_resolution == 4) ? "1.25" : "1.0");
			UtilsMethod.WriteSettingEx("xy3.ini", "Net", "high_definition", strValue2);
			SyncResolutionToSubClient();
		}
		LaunchClient(gameStyle);
	}

	private void _TryEixtWallpaper()
	{
		string excludeFile = UtilsMethod.NormalizePath($"{GlobalVariable.g_dst_path}\\bin\\{UtilsMethod.GetMyName()}");
		List<Process> list = new List<Process>();
		List<Process> list2 = new List<Process>();
		UtilsMethod.CheckClientProcess(GlobalVariable.g_dst_path, list, excludeFile);
		list2.Clear();
		foreach (Process item in list)
		{
			if (item.ProcessName == "XYQWallPaper" || item.ProcessName == "mhRCPlayer")
			{
				list2.Add(item);
			}
		}
		if (list2.Count > 0)
		{
			UtilsMethod.KillClientProcess(list2);
		}
	}

	private void _OpenPage(string url)
	{
		UtilsMethod.OpenPage(url);
	}

	private string _GetCookie()
	{
		if (webBrowser == null)
		{
			return "";
		}
		HtmlDocument htmlDocument = (HtmlDocument)webBrowser.Document;
		if (htmlDocument == null)
		{
			return "";
		}
		HtmlElement body = htmlDocument.Body;
		if (body == null)
		{
			return "";
		}
		return body.GetAttribute("URSCK");
	}

	private void _SendMacInfo()
	{
		string mac_str = "";
		if (!UtilsMethod.GetMacAddr(ref mac_str) || mac_str.Length < 6)
		{
			return;
		}
		string cookie = m_cookie;
		if (cookie.Length == 0)
		{
			return;
		}
		mac_str = mac_str.Replace('-', ':').ToLower();
		PostPrams postPrams = new PostPrams();
		postPrams.AddSection("point_id", "1379").AddSection("s", "Psv5cprrGsoiAdPP4i5RnoFx99Q%3D").AddSection("mac", mac_str)
			.AddSection("login_user_id", cookie);
		HttpPoster.SendGet(ConstDef.MAC_POST, postPrams.getParam(), "gbk");
		string val = mac_str.Replace("-", "");
		char[] array = new char[256];
		if (Updater.get_flash_cookie(array))
		{
			string text = UtilsMethod.OutputToString(array);
			if (text.Length != 0)
			{
				postPrams = new PostPrams();
				postPrams.AddSection("point_id", "1379").AddSection("s", GlobalVariable.S_KEY).AddSection("mac", val)
					.AddSection("login_user_id", text);
				HttpPoster.SendGet(ConstDef.MAC_POST, postPrams.getParam(), "gbk");
			}
		}
	}

	private void _CheckClientLaunch(object state)
	{
		m_timer_launchclient = null;
		if (!m_pGame.HasExited)
		{
			return;
		}
		int exitCode = m_pGame.ExitCode;
		if (exitCode != 259)
		{
			m_timer_launchclient.Dispose();
			if (exitCode == -1073741515)
			{
				System.Windows.MessageBox.Show("启动程序失败(缺失dll)", "梦幻西游更新程序", MessageBoxButton.OK);
			}
			else
			{
				System.Windows.MessageBox.Show($"启动程序失败(错误码：{exitCode})", "梦幻西游更新程序", MessageBoxButton.OK);
			}
		}
	}

	private void _OnTextTimerTick(object sender, EventArgs e)
	{
		if (m_text_index >= 0)
		{
			int num = m_text_index + 1;
			num %= m_labels.Count;
			_ShowTextNow(num);
		}
		Console.WriteLine("launch ZIndex " + System.Windows.Controls.Panel.GetZIndex(this));
	}

	private void _ShowTextNow(int index)
	{
		if (m_text_index >= 0)
		{
			foreach (System.Windows.Controls.Label item in m_labels[m_text_index])
			{
				item.Visibility = Visibility.Hidden;
			}
		}
		m_text_index = index;
		if (m_text_index < 0)
		{
			return;
		}
		foreach (System.Windows.Controls.Label item2 in m_labels[m_text_index])
		{
			item2.Visibility = Visibility.Visible;
		}
	}

	private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		Window window = Window.GetWindow(m_Wind);
		if (e.ChangedButton == MouseButton.Left)
		{
			window.DragMove();
		}
	}

	private void Hyperlink_Click(object sender, RoutedEventArgs e)
	{
		Console.WriteLine("Hyperlink_Click");
		_OpenPage("https://probe.netease.com/mhxy?platform=pc");
	}

	private void _AddASyncAction(Action action)
	{
		if (!m_enable_dispatcher)
		{
			action();
			return;
		}
		DispatcherOperation dispatcherOperation = System.Windows.Application.Current.Dispatcher.BeginInvoke(action);
		m_dispatcherOperations.Add(dispatcherOperation);
		dispatcherOperation.Completed += delegate(object sender, EventArgs args)
		{
			DispatcherOperation doper = (DispatcherOperation)sender;
			if (m_dispatcherOperations.Contains(doper))
			{
				m_dispatcherOperations.RemoveAll((DispatcherOperation x) => x == doper);
			}
		};
	}

	private void btn_chongzhi_Click(object sender, RoutedEventArgs e)
	{
		Console.WriteLine("btn_chongzhi_Click");
		LogAPI.ClickLogWrite("bc_money");
		_OpenPage("http://ecard.163.com/script/index/?=xyqload");
	}

	private void btn_shezhi_Click(object sender, RoutedEventArgs e)
	{
		Console.WriteLine("btn_shezhi_Click");
		LogAPI.ClickLogWrite("bc_setting");
		SettingDialog settingDialog = new SettingDialog(m_resolution, Updater.get_subclient_mode(GlobalVariable.g_dst_path));
		settingDialog.Owner = window;
		settingDialog.ShowDialog();
		if (!settingDialog.CloseSelf)
		{
			m_resolution_changed = settingDialog.Resolution != m_resolution;
			int resolution = m_resolution;
			m_resolution = settingDialog.Resolution;
			if (m_resolution_changed)
			{
				string key = $"resolution{GetGameStyle()}";
				string val = m_resolution.ToString();
				UtilsMethod.WriteConfig(key, val);
				string strValue = ((m_resolution == 3 || m_resolution == 4) ? "1.25" : "1.0");
				UtilsMethod.WriteSettingEx("xy3.ini", "Net", "high_definition", strValue);
				SyncResolutionToSubClient();
				if (resolution_desc.ContainsKey(resolution) && resolution_desc.ContainsKey(m_resolution))
				{
					LogAPI.ClickLogWrite("bc_change_resolution_{0}_to_{1}", resolution_desc[resolution], resolution_desc[m_resolution]);
				}
				else if (resolution_desc.ContainsKey(m_resolution))
				{
					LogAPI.ClickLogWrite("bc_set_resolution_{0}", resolution_desc[m_resolution]);
				}
			}
			if (settingDialog.SubclientMode != Updater.get_subclient_mode(GlobalVariable.g_dst_path))
			{
				Updater.set_subclient_mode(settingDialog.SubclientMode, GlobalVariable.g_dst_path);
				Restart();
			}
		}
		else
		{
			_TryEixtWallpaper();
			ExitSelf();
		}
	}

	private void btn_zhuce_Click(object sender, RoutedEventArgs e)
	{
		Console.WriteLine("btn_zhuce_Click");
		LogAPI.ClickLogWrite("bc_register");
		_OpenPage("https://zc.reg.163.com/regInitialized?pd=xyq&pkid=AOYqLVS&pkht=xyq.163.com");
	}

	private void btn_guanwang_Click(object sender, RoutedEventArgs e)
	{
		LogAPI.ClickLogWrite("bc_mainpage");
		Console.WriteLine("btn_guanwang_Click");
		_OpenPage("http://xyq.163.com/?=xyqload");
	}

	private void btn_sign_Click(object sender, RoutedEventArgs e)
	{
		Console.WriteLine("btn_sign_Click");
		LogAPI.ClickLogWrite("bc_login");
		LogAPI.ClickLogWrite("bc_login_new");
		Updater.set_p1_xxsj_async(GlobalVariable.SJF_P1_LOGIN);
		if (m_curPartProgressIdx >= m_curPartProgressIdx - 1 || btn_sign.IsVisible)
		{
			_Play();
		}
	}

	private void btn_loading_Click(object sender, RoutedEventArgs e)
	{
		Console.WriteLine("btn_loading_Click");
	}

	private void btn_close_Click(object sender, RoutedEventArgs e)
	{
		Console.WriteLine("btn_close_Click");
		if (m_updater.isDownLoading())
		{
			if (ShowMyMessageBox("关闭窗口会中断更新，是否确认？", "梦幻西游更新程序", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
			{
				ExitSelf();
			}
		}
		else
		{
			ExitSelf();
		}
	}

	private void btn_min_Click(object sender, RoutedEventArgs e)
	{
		base.WindowState = WindowState.Minimized;
	}

	private void img_ageTips_Click(object sender, RoutedEventArgs e)
	{
		UtilsMethod.OpenPage("http://xyq.163.com/org_bg/20210609/15743_952582.html");
	}

	private void Window_StateChanged(object sender, EventArgs e)
	{
		window.WindowState = base.WindowState;
	}

	private void btn_sign_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		testRect.Visibility = btn_sign.Visibility;
	}

	private void img_wangyilogo_Click(object sender, RoutedEventArgs e)
	{
		UtilsMethod.OpenPage("https://xyq.gm.163.com/");
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/my_new;component/launch.xaml", UriKind.Relative);
			System.Windows.Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			((launch)target).StateChanged += Window_StateChanged;
			break;
		case 2:
			_base = (Grid)target;
			_base.MouseLeftButtonDown += Grid_MouseLeftButtonDown;
			break;
		case 3:
			Web = (Grid)target;
			break;
		case 4:
			img_banner = (Image)target;
			break;
		case 5:
			img_LabelBg = (Image)target;
			break;
		case 6:
			webHint = (Border)target;
			break;
		case 7:
			JuanZhou = (Grid)target;
			break;
		case 8:
			img_juanzhoudi = (Image)target;
			break;
		case 9:
			img_progress = (Image)target;
			break;
		case 10:
			prog_download = (System.Windows.Controls.ProgressBar)target;
			break;
		case 11:
			img_progressHead = (Image)target;
			break;
		case 12:
			mask_x = (TranslateTransform)target;
			break;
		case 13:
			Spine = (Grid)target;
			break;
		case 14:
			img_Door = (Image)target;
			break;
		case 15:
			SpineCanvas = (Canvas)target;
			break;
		case 16:
			Player = (Grid)target;
			break;
		case 17:
			img_juese = (Image)target;
			break;
		case 18:
			UI = (Grid)target;
			break;
		case 19:
			btn_chongzhi = (CustomImageButton)target;
			break;
		case 20:
			btn_shezhi = (CustomImageButton)target;
			break;
		case 21:
			btn_zhuce = (CustomImageButton)target;
			break;
		case 22:
			btn_guanwang = (CustomImageButton)target;
			break;
		case 23:
			btn_sign = (CustomImageButton)target;
			break;
		case 24:
			btn_loading = (CustomImageButton)target;
			break;
		case 25:
			testRect = (Rectangle)target;
			break;
		case 26:
			img_loading = (Image)target;
			break;
		case 27:
			lbl_loading = (System.Windows.Controls.Label)target;
			break;
		case 28:
			img_logodi = (Image)target;
			break;
		case 29:
			img_logo = (Image)target;
			break;
		case 30:
			img_wylogo1 = (Image)target;
			break;
		case 31:
			img_wangyilogo = (CustomImageButton)target;
			break;
		case 32:
			btn_close = (CustomImageButton)target;
			break;
		case 33:
			btn_min = (CustomImageButton)target;
			break;
		case 34:
			lbl_huoquziyuanbao = (System.Windows.Controls.Label)target;
			break;
		case 35:
			lbl_daxiao = (System.Windows.Controls.Label)target;
			break;
		case 36:
			lbl_xiazaisudu = (System.Windows.Controls.Label)target;
			break;
		case 37:
			lbl_shengyushijian = (System.Windows.Controls.Label)target;
			break;
		case 38:
			lbl_wenti = (TextBlock)target;
			break;
		case 39:
			((Hyperlink)target).Click += Hyperlink_Click;
			break;
		case 40:
			lbl_fangchenmi1 = (System.Windows.Controls.Label)target;
			break;
		case 41:
			lbl_fangchenmi2 = (System.Windows.Controls.Label)target;
			break;
		case 42:
			lbl_fangchenmi3 = (System.Windows.Controls.Label)target;
			break;
		case 43:
			lbl_fangchenmi4 = (System.Windows.Controls.Label)target;
			break;
		case 44:
			lbl_fangchenmi5 = (System.Windows.Controls.Label)target;
			break;
		case 45:
			lbl_fangchenmi6 = (System.Windows.Controls.Label)target;
			break;
		case 46:
			lbl_fangchenmi7 = (System.Windows.Controls.Label)target;
			break;
		case 47:
			lbl_fangchenmi8 = (System.Windows.Controls.Label)target;
			break;
		case 48:
			img_ageTips = (CustomImageButton)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
