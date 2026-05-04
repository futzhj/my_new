using my_new.utils;

namespace my_new.log;

internal class LogAPI
{
	public static int LogRegistChannel(int channel, string filename, string channelname)
	{
		return LogService.Instance().RegistChannel(channel, filename, channelname);
	}

	public static int LogRegistChannelAdd(int channel, string filename, string channelname)
	{
		return LogService.Instance().RegistChannel(channel, filename, channelname, add: true);
	}

	public static void LogUnregistChannel(int channel)
	{
		LogService.Instance().UnregistChannel(channel);
	}

	public static void LogRegistInvoke(int channel, InvokeWriteFunc func)
	{
		LogService.Instance().RegistInvokeFunc(channel, func);
	}

	public static void LogRegistErrorInvoke(int channel, InvokeWriteFunc func)
	{
		LogService.Instance().RegistErrorInvokeFunc(channel, func);
	}

	public static void LogUnregistInvoke(int channel, InvokeWriteFunc func)
	{
		LogService.Instance().UnregistInvokeFunc(channel, func);
	}

	public static void SetLogRootPath(string rootpath)
	{
		GlobalVariable.SetLogRootPath(rootpath);
	}

	public static void LogWrite(string fmt, params string[] args)
	{
		LogService.Instance().WriteString(0, fmt, args);
	}

	public static void ClickLogWrite(string fmt, params string[] args)
	{
		LogService.Instance().WriteString(1, fmt, args);
	}

	public static void LogError(string fmt, params string[] args)
	{
		LogService.Instance().WriteError(0, fmt, args);
	}

	public static void LogWriteEx(int channel, string fmt, params string[] args)
	{
		LogService.Instance().WriteString(channel, fmt, args);
	}

	public static void LogErrorEx(int channel, string fmt, string args)
	{
		LogService.Instance().WriteError(channel, fmt, args);
	}
}
