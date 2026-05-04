using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using my_new.log;
using my_new.utils;

namespace my_new.net;

internal class NetServer
{
	private static IntPtr hRemoteCheck;

	private static RemoteChecker wnd;

	private static Task<uint> copyTask;

	private static CancellationTokenSource cancelToken;

	public static void WriteErrorMessage(string msg)
	{
		LogAPI.LogWrite(msg);
	}

	public static int GetClientNeedUpdateSize()
	{
		Updater.fetch_server_list();
		Updater.fetch_remote_note();
		return 1024;
	}

	public static void SaveAutoSwitchLog(bool bAutoSwitchOn)
	{
		string text = "{\"new_log\": {\"LOG_CONTENT\": 1}}";
		string url = "https://clientlog.xyq.netease.com/query";
		string val = "";
		int num = text.IndexOf("LOG_CONTENT");
		if (num != -1)
		{
			val = text.Remove(num, "LOG_CONTENT".Length).Insert(num, bAutoSwitchOn ? "AutoSwitchOn" : "AutoSwitchOff");
		}
		PostPrams postPrams = new PostPrams();
		postPrams.AddSection("Content", val).AddSection("logfile", "VersionAutoSwitch").AddSection("Tag", "VersionAutoSwitch");
		HttpPoster.SendGet(url, postPrams.getParam(), "gbk", "Host: clientlog.xyq.netease.com");
	}

	public static uint GetServerList()
	{
		new MessagePoster(hRemoteCheck, 1025);
		string serverListUrl = GetServerListUrl();
		int millisecond = DateTime.Now.Millisecond;
		string text = HttpPoster.SendGet(serverListUrl, "", "gbk");
		_ = DateTime.Now.Millisecond;
		StreamWriter streamWriter = new StreamWriter(GlobalVariable.XY_PATH + "\\server.ini");
		if (text.Length == 0)
		{
			WriteErrorMessage("无法打开服务器列表文件");
			return 0u;
		}
		streamWriter.Write(text);
		streamWriter.Close();
		streamWriter = null;
		return 1u;
	}

	public static uint GetGameNote()
	{
		new MessagePoster(hRemoteCheck, 1027);
		string setting = UtilsMethod.GetSetting("update.ini", "NoteFile", ConstDef.NOTE_INI);
		_ = DateTime.Now.Millisecond;
		string text = HttpPoster.SendGet(setting, "", "gbk");
		StreamWriter streamWriter = new StreamWriter(GlobalVariable.XY_PATH + "\\note.txt");
		if (text.Length == 0)
		{
			WriteErrorMessage("无法打开公告文件");
			return 0u;
		}
		streamWriter.Write(text);
		streamWriter.Close();
		streamWriter = null;
		return 1u;
	}

	public static string GetServerListUrl()
	{
		return UtilsMethod.GetSetting("update.ini", "ServerList", ConstDef.SERVER_INI);
	}

	public static string GetPatchListUrl()
	{
		return UtilsMethod.GetSetting("update.ini", "UpdateURL", ConstDef.PATCH_INI);
	}

	public static bool CopyExeFile(string src, string dest)
	{
		FileInfo fileInfo = new FileInfo(src);
		if (fileInfo.Exists)
		{
			try
			{
				fileInfo.CopyTo(dest, overwrite: true);
			}
			catch
			{
				return false;
			}
			return true;
		}
		fileInfo = null;
		Thread.Sleep(500);
		fileInfo = new FileInfo(src);
		if (fileInfo.Exists)
		{
			fileInfo.CopyTo(dest, overwrite: true);
			return true;
		}
		return false;
	}

	public static uint ExecuteCopyProcess()
	{
		new MessagePoster(hRemoteCheck, 1028);
		string[] obj = new string[4]
		{
			"myRes.dll",
			"updater.dll",
			"httpdns.dll",
			UtilsMethod.GetMyName()
		};
		string currentDirectory = Directory.GetCurrentDirectory();
		string currentDirectory2 = Directory.GetCurrentDirectory();
		string text = "";
		string[] array = obj;
		foreach (string text2 in array)
		{
			text = currentDirectory2 + "\\bin\\" + text2;
			Directory.CreateDirectory(currentDirectory2 + "\\bin\\");
			if (CopyExeFile(text2, text))
			{
				continue;
			}
			string mD5WithFilePath = UtilsMethod.GetMD5WithFilePath(text2);
			if (UtilsMethod.GetMD5WithFilePath(text) != mD5WithFilePath)
			{
				currentDirectory = Path.GetTempPath();
				text = currentDirectory + text2;
				if (!CopyExeFile(text2, text))
				{
					WriteErrorMessage("启动《梦幻西游 ONLINE》失败!");
					return 1u;
				}
			}
		}
		currentDirectory = $"{GlobalVariable.LAUNCH_ARG} \"{currentDirectory2}\" {GlobalVariable.g_subver} {GlobalVariable.g_switch_game_style} {GlobalVariable.g_autorun_arg} {GlobalVariable.g_ngp_port}";
		Process process = new Process();
		process.StartInfo.FileName = text;
		process.StartInfo.Arguments = currentDirectory;
		process.StartInfo.WorkingDirectory = currentDirectory2;
		bool flag = false;
		try
		{
			flag = process.Start();
		}
		catch
		{
		}
		if (flag)
		{
			return 0u;
		}
		return 1u;
	}

	private static void RemoteCheckFunc(ref Message m)
	{
		IntPtr hWnd = m.HWnd;
		IntPtr wParam = m.WParam;
		switch (m.Msg)
		{
		case 272:
			hRemoteCheck = hWnd;
			wnd.Center();
			LogAPI.LogWrite("正在启动程序");
			wnd.SetMessage("正在启动...");
			cancelToken = new CancellationTokenSource();
			copyTask = new Task<uint>(() => ExecuteCopyProcess(), cancelToken.Token);
			copyTask.Start();
			m.Result = (IntPtr)1;
			return;
		case 1028:
		{
			Task.WaitAny(new Task[1] { copyTask }, 700);
			uint result = copyTask.Result;
			copyTask = null;
			cancelToken = null;
			if (result == 0)
			{
				int num = (int)UtilsMethod.GetLastError();
				LogAPI.LogWrite("启动程序失败，error code:{0}", num.ToString());
				MainWindow.ShowMessageBox("启动《梦幻西游 ONLINE》失败!", "温馨提醒", MessageBoxButton.OK);
				wnd.Close();
				hRemoteCheck = (IntPtr)0;
			}
			else
			{
				wnd.Close();
				hRemoteCheck = (IntPtr)0;
			}
			m.Result = (IntPtr)1;
			return;
		}
		case 273:
			if (wParam == (IntPtr)2)
			{
				wnd.SetMessage("正在取消...");
				if (copyTask != null && !copyTask.IsCompleted)
				{
					Task.WaitAny(new Task[1] { copyTask }, 700);
				}
				if (copyTask != null)
				{
					cancelToken.Cancel();
					copyTask = null;
					cancelToken = null;
				}
				wnd.Close();
				m.Result = (IntPtr)0;
				return;
			}
			break;
		}
		m.Result = (IntPtr)0;
	}

	public static void RemoteChecker()
	{
	}
}
