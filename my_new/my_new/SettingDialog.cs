using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using my_new.log;
using my_new.net;
using my_new.utils;

namespace my_new;

public class SettingDialog : Window, IComponentConnector
{
	private int m_resolution;

	private int m_subclientmode;

	private bool m_inited;

	private bool m_exit_process;

	private List<ImageBrush> m_changguiBrushes = new List<ImageBrush>();

	private List<ImageBrush> m_qitaBrushes = new List<ImageBrush>();

	internal new Grid Background;

	internal Image img_bg;

	internal CustomImageButton btn_close;

	internal CustomImageButton btn_changgui;

	internal CustomImageButton btn_qita;

	internal Grid Changgui;

	internal Label lbl_fenbianlv;

	internal ComboBox ComboBox_Fenbianlv;

	internal Label lbl_fenbianlv1;

	internal Label lbl_Screenmode;

	internal ComboBox ComboBox_Screenmode;

	internal Label lbl_Screenmode1;

	internal Label lbl_Record;

	internal ComboBox ComboBox_Record;

	internal Label lbl_Record1;

	internal Label lbl_Switch;

	internal ComboBox ComboBox_Switch;

	internal Label lbl_Switch1;

	internal Label lbl_Open;

	internal ComboBox ComboBox_Open;

	internal Label lbl_Open1;

	internal Label lbl_Banben;

	internal ComboBox ComboBox_Banben;

	internal Grid Qita;

	internal CustomImageButton btn_repair;

	internal CustomImageButton btn_videoplayer;

	internal CustomImageButton btn_diagnostics;

	internal CustomImageButton btn_dongtaizhuomian;

	private bool _contentLoaded;

	public int Resolution => m_resolution;

	public int SubclientMode => m_subclientmode;

	public bool CloseSelf => m_exit_process;

	public SettingDialog(int resolution, int subclientmode)
	{
		m_resolution = resolution;
		m_subclientmode = subclientmode;
		if (m_resolution == -1)
		{
			m_resolution = 3;
		}
		for (int i = 0; i < 8; i++)
		{
			ImageBrush imageBrush = new ImageBrush();
			BitmapImage imageSource = new BitmapImage(new Uri($"pack://application:,,,/Resources/Images/tab_常规设置{i:D4}.png"));
			imageBrush.Stretch = Stretch.None;
			imageBrush.ImageSource = imageSource;
			m_changguiBrushes.Add(imageBrush);
			imageBrush = new ImageBrush();
			imageSource = new BitmapImage(new Uri($"pack://application:,,,/Resources/Images/tab_其他工具{i:D4}.png"));
			imageBrush.ImageSource = imageSource;
			imageBrush.Stretch = Stretch.None;
			m_qitaBrushes.Add(imageBrush);
		}
		InitializeComponent();
		m_inited = false;
		ComboBox_Fenbianlv.SelectedIndex = m_resolution;
		_SetComboBoxConfig(ComboBox_Screenmode, "fullscreen", "0", "0");
		_SetComboBoxConfig(ComboBox_Record, "gamerecord", "0", "1");
		_SetComboBoxConfig(ComboBox_Banben, "forceoldstyle", "0", "0");
		if (m_subclientmode > 0)
		{
			ComboBox_Switch.SelectedIndex = 1;
		}
		else
		{
			ComboBox_Switch.SelectedIndex = 0;
		}
		_SetComboBoxConfig(ComboBox_Open, "launchtype", "0", "0");
		ComboBox_Screenmode.IsEnabled = false;
		showTab(0);
		m_inited = true;
	}

	private void showTab(int i)
	{
		if (i == 1)
		{
			Changgui.Visibility = Visibility.Visible;
			Qita.Visibility = Visibility.Hidden;
			btn_changgui.NormalImage = m_changguiBrushes[4];
			btn_changgui.PressedImage = m_changguiBrushes[5];
			btn_changgui.HoverImage = m_changguiBrushes[6];
			btn_qita.NormalImage = m_qitaBrushes[0];
			btn_qita.PressedImage = m_qitaBrushes[1];
			btn_qita.HoverImage = m_qitaBrushes[2];
		}
		else
		{
			Changgui.Visibility = Visibility.Hidden;
			Qita.Visibility = Visibility.Visible;
			btn_changgui.NormalImage = m_changguiBrushes[0];
			btn_changgui.PressedImage = m_changguiBrushes[1];
			btn_changgui.HoverImage = m_changguiBrushes[2];
			btn_qita.NormalImage = m_qitaBrushes[4];
			btn_qita.PressedImage = m_qitaBrushes[5];
			btn_qita.HoverImage = m_qitaBrushes[6];
		}
	}

	public bool SwitchTab(int i)
	{
		if (i < 0 || i > 1)
		{
			return false;
		}
		showTab(i);
		return true;
	}

	private void _SetComboBoxConfig(ComboBox inst, string key, string defaultKey, string firstKey)
	{
		if (UtilsMethod.GetSettingEx("xy1.ini", "Setting", key, defaultKey) == firstKey)
		{
			inst.SelectedIndex = 0;
		}
		else
		{
			inst.SelectedIndex = 1;
		}
	}

	private void _WriteConfig(ComboBox inst, string key, bool opposite = false)
	{
		string text = "0";
		if (inst.SelectedIndex != 0)
		{
			text = "1";
		}
		if (opposite)
		{
			if (text == "0")
			{
				text = "1";
			}
			else if (text == "1")
			{
				text = "0";
			}
		}
		UtilsMethod.WriteSettingEx("xy1.ini", "Setting", key, text);
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
			LogAPI.LogError("权限失败");
		}
		if (!flag)
		{
			launch.ShowMyMessageBox("启动程序时失败", "梦幻西游更新程序", MessageBoxButton.OK, this);
		}
		return flag;
	}

	private void btn_close_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void ComboBox_Screenmode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (m_inited)
		{
			Console.WriteLine("ComboBox_Screenmode " + ComboBox_Screenmode.SelectedIndex);
			_WriteConfig(ComboBox_Screenmode, "fullscreen");
		}
	}

	private void ComboBox_Record_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (m_inited)
		{
			Console.WriteLine("ComboBox_Record " + ComboBox_Record.SelectedIndex);
			_WriteConfig(ComboBox_Record, "gamerecord", opposite: true);
		}
	}

	private void ComboBox_Switch_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!m_inited)
		{
			return;
		}
		Console.WriteLine("ComboBox_Switch " + ComboBox_Switch.SelectedIndex);
		if (m_subclientmode == 0 && ComboBox_Switch.SelectedIndex == 1)
		{
			string settingEx = UtilsMethod.GetSettingEx("client.ini", "Setting", "OptLv", "65535");
			int num = 0;
			num = ((settingEx == "0") ? 200 : ((!(settingEx == "1")) ? 1000 : 300));
			if (MessageBox.Show($"自动切换模式下，客户端将额外占用约100-1000M存储空间，确认切换吗？", "设置切换模式", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(GlobalVariable.g_dst_path));
				long num2 = driveInfo.AvailableFreeSpace / 1048576;
				_ = driveInfo.TotalSize / 1048576;
				if (num2 < num + num / 10)
				{
					MessageBox.Show("自动切换模式将额外占用约100-1000M存储空间，当前磁盘下存储空间不足，请清理后再设置。", "设置切换模式失败", MessageBoxButton.OK);
					ComboBox_Switch.SelectedIndex = 0;
				}
				else
				{
					m_subclientmode = 1;
					NetServer.SaveAutoSwitchLog(bAutoSwitchOn: true);
					Close();
				}
			}
			else
			{
				ComboBox_Switch.SelectedIndex = 0;
			}
		}
		else
		{
			if (m_subclientmode == 0 || ComboBox_Switch.SelectedIndex != 0)
			{
				return;
			}
			if (MessageBox.Show("手动确认模式可缩减客户端所占存储空间，但无法在不同服务器版本游戏客户端之间自动切换， 是否切换？", "设置切换模式", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				int num3 = m_subclientmode;
				int num4 = 1;
				_ = string.Empty;
				string excludeFile = UtilsMethod.NormalizePath(Path.Combine(GlobalVariable.g_dst_path, "bin", UtilsMethod.GetMyName()));
				List<Process> list = new List<Process>();
				new List<Process>();
				bool flag = UtilsMethod.CheckClientProcess(GlobalVariable.g_dst_path, list, excludeFile);
				excludeFile = string.Empty;
				while (num3 != 0 && !flag)
				{
					int num5 = num4 * (num3 & 1);
					num3 >>= 1;
					if (num5 > 0)
					{
						string g_dst_path = GlobalVariable.g_dst_path;
						g_dst_path = ((!Updater.is_shiwan_subversion(num5)) ? Path.Combine(g_dst_path, "subclient" + num5) : Path.Combine(g_dst_path, "subclientshiwan"));
						flag = UtilsMethod.CheckClientProcess(g_dst_path, list, excludeFile);
					}
					num4++;
				}
				if (flag)
				{
					string text = "发现运行的客户端有程序正在运行，请关闭以下程序后再重新选择切换模式：\n";
					foreach (Process item in list)
					{
						text = text + "\n" + item.ProcessName;
					}
					MessageBox.Show(text, "错误", MessageBoxButton.OKCancel);
					ComboBox_Switch.SelectedIndex = 1;
				}
				else
				{
					m_subclientmode = 0;
					NetServer.SaveAutoSwitchLog(bAutoSwitchOn: false);
					Close();
				}
			}
			else
			{
				ComboBox_Switch.SelectedIndex = 1;
			}
		}
	}

	private void ComboBox_Open_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (m_inited)
		{
			Console.WriteLine("ComboBox_Open " + ComboBox_Open.SelectedIndex);
			_WriteConfig(ComboBox_Open, "launchtype");
		}
	}

	private void ComboBox_Fenbianlv_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (m_inited)
		{
			Console.WriteLine("ComboBox_Fenbianlv" + ComboBox_Fenbianlv.SelectedIndex);
			m_resolution = ComboBox_Fenbianlv.SelectedIndex;
		}
	}

	private void btn_repair_Click(object sender, RoutedEventArgs e)
	{
		LogAPI.ClickLogWrite("bc_repair");
		if (launch.ShowMyMessageBox("启动该工具需要关闭当前程序，是否继续？", "梦幻西游更新程序", MessageBoxButton.YesNo, this) == MessageBoxResult.Yes && _OpenExeFile("MyRepair.exe"))
		{
			m_exit_process = true;
			Close();
		}
	}

	private void btn_videoplayer_Click(object sender, RoutedEventArgs e)
	{
		LogAPI.ClickLogWrite("bc_record");
		_OpenExeFile("xyqplayer2.exe");
	}

	private void btn_diagnostics_Click(object sender, RoutedEventArgs e)
	{
		LogAPI.ClickLogWrite("bc_netdiagnotor");
		UtilsMethod.OpenPage("https://probe.netease.com/mhxy?platform=pc");
	}

	private void btn_changgui_Click(object sender, RoutedEventArgs e)
	{
		showTab(1);
	}

	private void btn_qita_Click(object sender, RoutedEventArgs e)
	{
		showTab(0);
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		Console.WriteLine("test");
	}

	private void btn_dongtaizhuomian_Click(object sender, RoutedEventArgs e)
	{
		string text = Path.Combine(GlobalVariable.g_dst_path, "wallpaper", "XYQWallPaper.exe");
		if (UtilsMethod.IsMini() && !File.Exists(text))
		{
			if (launch.ShowMyMessageBox("迷你版需要下载扩展包，是否下载？", "梦幻西游更新程序", MessageBoxButton.YesNo, this) == MessageBoxResult.Yes)
			{
				UtilsMethod.WriteSettingEx("xy3.ini", "Net", "micro_ver", "2");
				_OpenExeFile("MyRepair.exe", "__extupdate_mode__");
			}
		}
		else
		{
			_OpenExeFile(text, "open");
			UtilsMethod.WriteConfig("wallpaper", "1");
		}
	}

	private void ComboBox_Banben_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (m_inited)
		{
			Console.WriteLine("ComboBox_Banben " + ComboBox_Banben.SelectedIndex);
			_WriteConfig(ComboBox_Banben, "forceoldstyle");
			if (ComboBox_Banben.SelectedIndex == 1)
			{
				LogAPI.ClickLogWrite("bc_switch_old_launch");
			}
			else
			{
				LogAPI.ClickLogWrite("bc_switch_new_launch");
			}
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/my_new;component/settingdialog.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
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
			Background = (Grid)target;
			break;
		case 2:
			img_bg = (Image)target;
			break;
		case 3:
			btn_close = (CustomImageButton)target;
			break;
		case 4:
			btn_changgui = (CustomImageButton)target;
			break;
		case 5:
			btn_qita = (CustomImageButton)target;
			break;
		case 6:
			Changgui = (Grid)target;
			break;
		case 7:
			lbl_fenbianlv = (Label)target;
			break;
		case 8:
			ComboBox_Fenbianlv = (ComboBox)target;
			ComboBox_Fenbianlv.SelectionChanged += ComboBox_Fenbianlv_SelectionChanged;
			break;
		case 9:
			lbl_fenbianlv1 = (Label)target;
			break;
		case 10:
			lbl_Screenmode = (Label)target;
			break;
		case 11:
			ComboBox_Screenmode = (ComboBox)target;
			ComboBox_Screenmode.SelectionChanged += ComboBox_Screenmode_SelectionChanged;
			break;
		case 12:
			lbl_Screenmode1 = (Label)target;
			break;
		case 13:
			lbl_Record = (Label)target;
			break;
		case 14:
			ComboBox_Record = (ComboBox)target;
			ComboBox_Record.SelectionChanged += ComboBox_Record_SelectionChanged;
			break;
		case 15:
			lbl_Record1 = (Label)target;
			break;
		case 16:
			lbl_Switch = (Label)target;
			break;
		case 17:
			ComboBox_Switch = (ComboBox)target;
			ComboBox_Switch.SelectionChanged += ComboBox_Switch_SelectionChanged;
			break;
		case 18:
			lbl_Switch1 = (Label)target;
			break;
		case 19:
			lbl_Open = (Label)target;
			break;
		case 20:
			ComboBox_Open = (ComboBox)target;
			ComboBox_Open.SelectionChanged += ComboBox_Open_SelectionChanged;
			break;
		case 21:
			lbl_Open1 = (Label)target;
			break;
		case 22:
			lbl_Banben = (Label)target;
			break;
		case 23:
			ComboBox_Banben = (ComboBox)target;
			ComboBox_Banben.SelectionChanged += ComboBox_Banben_SelectionChanged;
			break;
		case 24:
			Qita = (Grid)target;
			break;
		case 25:
			btn_repair = (CustomImageButton)target;
			break;
		case 26:
			btn_videoplayer = (CustomImageButton)target;
			break;
		case 27:
			btn_diagnostics = (CustomImageButton)target;
			break;
		case 28:
			btn_dongtaizhuomian = (CustomImageButton)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
