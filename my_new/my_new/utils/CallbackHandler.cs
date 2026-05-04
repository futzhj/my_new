using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using my_new.log;

namespace my_new.utils;

public class CallbackHandler
{
	private static CheckCallback keptDelegate;

	private static launch m_launch;

	public static void SetupCallback(launch launch)
	{
		keptDelegate = CheckFunc;
		Updater.set_check_func(keptDelegate);
		m_launch = launch;
	}

	public static void Release()
	{
		keptDelegate = null;
		m_launch = null;
	}

	private static bool CheckFunc(string basepath, bool check_only)
	{
		LogAPI.LogWrite("CheckClient begin");
		if (basepath == null || basepath.Length == 0)
		{
			basepath = GlobalVariable.g_dst_path;
		}
		string excludeFile = UtilsMethod.NormalizePath(Path.Combine(basepath, "bin", UtilsMethod.GetMyName()));
		List<Process> list = new List<Process>();
		List<Process> list2 = new List<Process>();
		bool flag = UtilsMethod.CheckClientProcess(basepath, list, excludeFile);
		if (check_only)
		{
			if (flag)
			{
				foreach (Process item in list)
				{
					if (item.ProcessName != "XYQWallPaper.exe" && item.ProcessName != "mhRCPlayer.exe")
					{
						LogAPI.LogError("CheckClient(check_only) conflict");
						return false;
					}
				}
			}
			LogAPI.LogWrite("CheckClient(check_only) ok");
			return true;
		}
		LogAPI.LogWrite("CheckClient msg begin");
		int num = 5;
		while (flag && num > 0)
		{
			list2.Clear();
			foreach (Process item2 in list)
			{
				if (item2.ProcessName == "XYQWallPaper" || item2.ProcessName == "mhRCPlayer")
				{
					list2.Add(item2);
				}
			}
			if (list2.Count > 0)
			{
				UtilsMethod.KillClientProcess(list2);
			}
			if (m_launch != null)
			{
				m_launch.SetDlgPatchStatus("等待客户端程序退出......", thread: true);
			}
			Thread.Sleep(1000);
			flag = UtilsMethod.CheckClientProcess(basepath, list, excludeFile);
			num--;
		}
		while (flag)
		{
			string strError = "发现运行的客户端有程序正在运行，请关闭以下程序后再点击“重试”按钮完成更新操作：\n";
			foreach (Process item3 in list)
			{
				strError = strError + "\n" + item3.ProcessName;
			}
			MessageBoxResult ret = MessageBoxResult.None;
			ManualResetEvent waitHandle = new ManualResetEvent(initialState: false);
			Application.Current.Dispatcher.Invoke((Action)delegate
			{
				try
				{
					ret = launch.ShowMyMessageBox(strError, "Error", MessageBoxButton.OKCancel);
				}
				finally
				{
					waitHandle.Set();
				}
			}, new object[0]);
			waitHandle.WaitOne();
			if (ret == MessageBoxResult.Cancel)
			{
				LogAPI.LogError("客户端有程序正在运行");
				return false;
			}
			List<Process> list3 = new List<Process>();
			foreach (Process item4 in list)
			{
				if (item4.ProcessName == "xyqsvc")
				{
					list3.Add(item4);
				}
			}
			if (list3.Count > 0)
			{
				UtilsMethod.KillClientProcess(list3);
			}
			list3 = new List<Process>();
			foreach (Process item5 in list)
			{
				if (item5.ProcessName == "mhrender")
				{
					list3.Add(item5);
				}
			}
			if (list3.Count > 0)
			{
				UtilsMethod.KillClientProcess(list3);
			}
			list2.Clear();
			foreach (Process item6 in list)
			{
				if (item6.ProcessName == "XYQWallPaper" || item6.ProcessName == "mhRCPlayer")
				{
					list2.Add(item6);
				}
			}
			if (list2.Count > 0)
			{
				UtilsMethod.KillClientProcess(list2);
			}
			flag = UtilsMethod.CheckClientProcess(basepath, list, excludeFile);
		}
		return true;
	}
}
