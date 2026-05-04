using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using my_new.log;

namespace my_new.utils;

internal class UtilsMethod
{
	[DllImport("kernel32.dll")]
	private static extern IntPtr LoadLibrary(string path);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetModuleHandle(string lpLibFileName);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

	[DllImport("kernel32.dll")]
	private static extern int GetPrivateProfileString(string lpSection, string lpKey, string lpDefault, byte[] retVal, int nSize, string filePath);

	[DllImport("kernel32.dll")]
	private static extern long WritePrivateProfileString(string lpSection, string lpKey, string lpVal, string filePath);

	[DllImport("kernel32.dll")]
	public static extern long GetLastError();

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool CloseHandle(IntPtr hObject);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("Gdi32.dll")]
	public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);

	[DllImport("Gdi32.dll")]
	public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

	[DllImport("Gdi32.dll")]
	public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

	[DllImport("Gdi32.dll")]
	public static extern bool DeleteDC(IntPtr hDC);

	[DllImport("Gdi32.dll")]
	public static extern bool DeleteObject(IntPtr hObject);

	[DllImport("Gdi32.dll")]
	public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

	[DllImport("user32.dll")]
	public static extern IntPtr GetDesktopWindow();

	[DllImport("user32.dll")]
	public static extern IntPtr GetWindowDC(IntPtr hWnd);

	[DllImport("user32.dll")]
	public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

	[DllImport("user32.dll")]
	public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

	public static bool IsInsideWine()
	{
		IntPtr moduleHandle = GetModuleHandle("ntdll.dll");
		bool result = false;
		if (moduleHandle != IntPtr.Zero && GetProcAddress(moduleHandle, "wine_get_version") != IntPtr.Zero)
		{
			result = true;
		}
		return result;
	}

	public static string NormalizePath(string path, bool isDir = false)
	{
		if (path == null)
		{
			return "";
		}
		string text = "";
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		while (!flag2 && num < path.Length)
		{
			char c = path[num];
			switch (c)
			{
			case '\0':
				if (flag || isDir)
				{
					text += "\\";
				}
				flag2 = true;
				break;
			case '/':
			case '\\':
				flag = true;
				break;
			default:
				if (flag)
				{
					text += "\\";
					flag = false;
				}
				text += c;
				break;
			}
			num++;
		}
		return text;
	}

	public static string GetSetting(string file, string strName, string strDefault)
	{
		byte[] array = new byte[1024];
		string xY_PATH = GlobalVariable.XY_PATH;
		xY_PATH = Path.Combine(GlobalVariable.XY_PATH, file);
		int privateProfileString = GetPrivateProfileString("Setting", strName, strDefault, array, 1024, xY_PATH);
		return Encoding.GetEncoding(Encoding.ASCII.CodePage).GetString(array, 0, privateProfileString);
	}

	public static string GetSettingEx(string file, string section, string strName, string strDefault)
	{
		if (file == "")
		{
			file = GlobalVariable.UPDATE_FILE_NAME;
		}
		string filePath = Path.Combine(GlobalVariable.g_dst_path, file);
		byte[] array = new byte[1024];
		int privateProfileString = GetPrivateProfileString(section, strName, strDefault, array, 1024, filePath);
		return Encoding.GetEncoding(Encoding.ASCII.CodePage).GetString(array, 0, privateProfileString);
	}

	public static void WriteSettingEx(string file, string strSession, string strName, string strValue)
	{
		string g_dst_path = GlobalVariable.g_dst_path;
		g_dst_path = Path.Combine(GlobalVariable.g_dst_path, file);
		WritePrivateProfileString(strSession, strName, strValue, g_dst_path);
	}

	public static bool IsValidLaunchPath()
	{
		string currentDirectory = Directory.GetCurrentDirectory();
		GlobalVariable.g_dst_path = $"{currentDirectory}\\";
		if (GetSettingEx("update.ini", "Setting", "Version", "").Length != 0)
		{
			return !IsSubClient();
		}
		return false;
	}

	public static bool IsSubClient()
	{
		return GetSettingEx("subclient.ini", "Setting", "Subversion", "").Length != 0;
	}

	public static bool CheckMaxInstance()
	{
		bool flag = false;
		Mutex mutex = null;
		try
		{
			for (int i = 0; i < GlobalVariable.MAX_INSTANCE_NUM; i++)
			{
				string name = $"mhxy_launch{i}";
				bool createdNew = false;
				mutex = new Mutex(initiallyOwned: false, name, out createdNew);
				if (createdNew)
				{
					if (GetLastError() != 183)
					{
						mutex.WaitOne();
						mutex.Close();
						flag = true;
						break;
					}
					mutex.ReleaseMutex();
					mutex.Close();
				}
			}
			if (!flag)
			{
				MessageBox.Show("您好！开启的梦幻西游客户端达到上限！", "温馨提示", MessageBoxButton.OK);
				return false;
			}
			return true;
		}
		finally
		{
			mutex?.Dispose();
		}
	}

	public static string GetMD5WithFilePath(string filePath)
	{
		FileStream inputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		return BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(inputStream));
	}

	public static string ReadConfig(string key, string defaultKey)
	{
		return GetSetting("xy1.ini", key, defaultKey);
	}

	public static string ReadConfigEx(string key, string defaultKey, string basePath)
	{
		string path = "xy1.ini";
		string filePath = Path.Combine(basePath, path);
		byte[] array = new byte[1024];
		int privateProfileString = GetPrivateProfileString("Setting", key, defaultKey, array, 1024, filePath);
		return Encoding.GetEncoding(Encoding.ASCII.CodePage).GetString(array, 0, privateProfileString);
	}

	public static void WriteConfig(string key, string val)
	{
		WritePrivateProfileString("Setting", key, val, GlobalVariable.g_dst_path + "\\xy1.ini");
	}

	public static void WriteConfigEx(string basePath, string fileName, string session, string key, string val)
	{
		WritePrivateProfileString(session, key, val, basePath + "\\" + fileName);
	}

	public static bool GetMacAddr(ref string mac_str)
	{
		try
		{
			List<string> list = new List<string>();
			foreach (ManagementObject instance in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
			{
				if ((bool)instance["IPEnabled"])
				{
					string item = instance["MacAddress"].ToString();
					list.Add(item);
				}
			}
			if (list.Count >= 1)
			{
				mac_str = list[0];
				return true;
			}
			mac_str = "";
		}
		catch (Exception)
		{
			mac_str = "";
		}
		return false;
	}

	public static void FixMicroVer()
	{
		if (!(GetSetting("update.ini", "CloudMode", "1") == "1"))
		{
			return;
		}
		if (GetSettingEx("xy3.ini", "Net", "micro_ver", "255") == "255")
		{
			WriteSettingEx("client.ini", "Setting", "DefaultPreload", "-1");
			WriteSettingEx("client.ini", "Setting", "OptLv", "65535");
			WriteSettingEx("client.ini", "Setting", "OptLvDone", "65535");
			return;
		}
		WriteSettingEx("client.ini", "Setting", "DefaultPreload", "0");
		if (GetSettingEx("xy3.ini", "Net", "micro_ext_installed", "") != "1")
		{
			WriteSettingEx("client.ini", "Setting", "OptLv", "0");
			WriteSettingEx("client.ini", "Setting", "OptLvDone", "0");
		}
		else
		{
			WriteSettingEx("client.ini", "Setting", "OptLv", "1");
			WriteSettingEx("client.ini", "Setting", "OptLvDone", "1");
		}
	}

	public static void FixMicroVerDone()
	{
		GetSetting("client.ini", "OptLvDone", "");
		WriteSettingEx("client.ini", "Setting", "OptLvDone", GetSetting("client.ini", "OptLvDone", "0"));
	}

	public static void ReportNGPError()
	{
	}

	public static int countBits(int n)
	{
		int num = 0;
		while (n != 0)
		{
			n &= n - 1;
			num++;
		}
		return num;
	}

	public static bool CheckClientProcess(string basepath, List<Process> processList, string excludeFile)
	{
		processList.Clear();
		Process[] processes = Process.GetProcesses();
		foreach (Process process in processes)
		{
			if (process.Id == Process.GetCurrentProcess().Id)
			{
				continue;
			}
			try
			{
				string fileName = process.MainModule.FileName;
				string text = fileName.ToLower();
				string text2 = excludeFile.ToLower();
				if (Path.GetDirectoryName(fileName) == basepath && text != text2)
				{
					processList.Add(process);
				}
			}
			catch (Exception)
			{
			}
		}
		return processList.Count > 0;
	}

	public static void KillClientProcess(List<Process> proceeeList)
	{
		foreach (Process proceee in proceeeList)
		{
			try
			{
				proceee.Kill();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to kill process: " + ex.Message);
			}
		}
	}

	public static bool CopyDirectoryRecursively(string source, string dest)
	{
		if (!Directory.Exists(source) || !Directory.Exists(dest))
		{
			return false;
		}
		string[] directories = Directory.GetDirectories(source, "*", SearchOption.AllDirectories);
		for (int i = 0; i < directories.Length; i++)
		{
			Directory.CreateDirectory(directories[i].Replace(source, dest));
		}
		directories = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);
		foreach (string obj in directories)
		{
			File.Copy(obj, obj.Replace(source, dest), overwrite: true);
		}
		return true;
	}

	public static bool NeedOpenWallpaper()
	{
		byte[] array = new byte[1024];
		int privateProfileString = GetPrivateProfileString("wallpaper", "startup", "0", array, 1024, GlobalVariable.g_dst_path + "\\wallpaper\\wp.ini");
		return Convert.ToInt32(Encoding.GetEncoding(Encoding.ASCII.CodePage).GetString(array, 0, privateProfileString)) == 1;
	}

	public static bool SaveToInt(string str, out int res)
	{
		if (int.TryParse(str, out var result))
		{
			res = result;
			return true;
		}
		res = 0;
		return false;
	}

	public static MessageBoxResult ShowNGPMessageBox(string str, string title, MessageBoxButton bts)
	{
		return launch.ShowMyMessageBox(str, title, bts);
	}

	public static void OpenPage(string url)
	{
		Process.Start(new ProcessStartInfo
		{
			FileName = url,
			UseShellExecute = true
		});
	}

	public static int GetBrowserVersion()
	{
		int result = 0;
		using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Internet Explorer", RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.QueryValues))
		{
			object value = registryKey.GetValue("svcVersion");
			if (value == null)
			{
				value = registryKey.GetValue("Version");
				if (value == null)
				{
					throw new ApplicationException("Microsoft Internet Explorer is required!");
				}
			}
			int.TryParse(value.ToString().Split('.')[0], out result);
		}
		if (result < 7)
		{
			throw new ApplicationException("不支持的浏览器版本！");
		}
		return result;
	}

	public static void SetWebBrowserFeatures(int iVersion)
	{
		string fileName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
		uint num = (uint)(iVersion * 1000);
		Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\" + "FEATURE_BROWSER_EMULATION", fileName, num, RegistryValueKind.DWord);
		Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\" + "FEATURE_ENABEL_CLIPCHILDREN_OPTIMIZATION", fileName, 1, RegistryValueKind.DWord);
	}

	public static string OutputToString(char[] buffer)
	{
		int num = Array.IndexOf(buffer, '\0');
		if (num >= 0)
		{
			return new string(buffer, 0, num);
		}
		return new string(buffer);
	}

	public static BitmapSource ToBitmapSource(Bitmap bmp)
	{
		IntPtr hbitmap = bmp.GetHbitmap();
		try
		{
			return Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
		}
		finally
		{
			DeleteObject(hbitmap);
		}
	}

	public static Bitmap CaptureWindow(IntPtr handle)
	{
		IntPtr windowDC = GetWindowDC(handle);
		RECT rect = default(RECT);
		GetWindowRect(handle, ref rect);
		int nWidth = rect.right - rect.left;
		int nHeight = rect.bottom - rect.top;
		IntPtr intPtr = CreateCompatibleDC(windowDC);
		IntPtr intPtr2 = CreateCompatibleBitmap(windowDC, nWidth, nHeight);
		IntPtr hObject = SelectObject(intPtr, intPtr2);
		BitBlt(intPtr, 0, 0, nWidth, nHeight, windowDC, 0, 0, 13369376);
		SelectObject(intPtr, hObject);
		DeleteDC(intPtr);
		ReleaseDC(handle, windowDC);
		return Image.FromHbitmap(intPtr2);
	}

	public static void SyncXyConfig(int subclient)
	{
		string[] array = new string[2] { "xy1.ini", "xy3.ini" };
		foreach (string path in array)
		{
			string text = Path.Combine(GlobalVariable.g_dst_path, path);
			if (File.Exists(text))
			{
				char[] array2 = new char[256];
				Updater.get_subclient_path(GlobalVariable.g_dst_path, GlobalVariable.g_subver, array2);
				string path2 = OutputToString(array2);
				path2 = Path.Combine(path2, path);
				try
				{
					Console.WriteLine($"Copy From {text} To {path2}");
					LogAPI.LogWrite("Copy From {0} To {1}", text, path2);
					File.Copy(text, path2, overwrite: true);
				}
				catch
				{
				}
			}
		}
	}

	public static bool IsMini()
	{
		return GetSettingEx("xy3.ini", "Net", "micro_ver", "255") != "255";
	}

	public static string GetMyName()
	{
		return "my_new.exe";
	}

	public static bool my_exchanged()
	{
		string g_dst_path = GlobalVariable.g_dst_path;
		string sourceFileName = Path.Combine(g_dst_path + "/_test", GetMyName());
		string destFileName = Path.Combine(g_dst_path, GetMyName());
		try
		{
			File.Copy(sourceFileName, destFileName, overwrite: true);
		}
		catch
		{
			return false;
		}
		return true;
	}

	public static void onStartSuccess()
	{
		if (GlobalVariable.gPartentWnd != IntPtr.Zero)
		{
			SendMessage(GlobalVariable.gPartentWnd, GlobalVariable.WM_START_NEW_MY_END, IntPtr.Zero, IntPtr.Zero);
		}
	}
}
