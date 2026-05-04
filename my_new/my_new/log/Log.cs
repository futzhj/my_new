using System;

namespace my_new.log;

internal class Log
{
	private LogProxy m_proxy;

	private string m_channelname;

	public Log(ref LogProxy proxy, string channelname)
	{
		m_proxy = proxy;
		m_channelname = channelname;
		if (m_proxy != null)
		{
			m_proxy.IncRef();
		}
	}

	~Log()
	{
		if (m_proxy != null)
		{
			m_proxy.Release();
		}
	}

	public bool IsVaild()
	{
		if (m_proxy != null)
		{
			return m_proxy.IsValid();
		}
		return false;
	}

	public void Write(string fmt, params string[] args)
	{
		string arg = DateTime.Now.ToString("HH:mm:ss.fff");
		m_proxy.GetFile().Write($"[{arg}] [{m_channelname}] ");
		m_proxy.GetFile().Write(string.Format(fmt, args));
		m_proxy.GetFile().Write("\n");
		m_proxy.GetFile().Flush();
	}

	public void WriteError(string fmt, params string[] args)
	{
		string arg = DateTime.Now.ToString("HH:mm:ss");
		m_proxy.GetFile().Write($"[{arg}] [{m_channelname}] [ERROR]\n***********************************************\n");
		m_proxy.GetFile().Write(string.Format(fmt, args));
		m_proxy.GetFile().Write("\n***********************************************\n");
		m_proxy.GetFile().Flush();
	}
}
