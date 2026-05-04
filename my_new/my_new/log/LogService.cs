using System.Collections.Generic;
using System.IO;
using my_new.utils;

namespace my_new.log;

internal class LogService
{
	private static LogService instance;

	private static object _object = new object();

	private Dictionary<int, Log> m_logs;

	private Dictionary<string, LogProxy> m_proxy;

	private Dictionary<int, List<InvokeWriteFunc>> m_invoke;

	private Dictionary<int, List<InvokeWriteFunc>> m_errorInvoke;

	public static LogService Instance()
	{
		if (instance == null)
		{
			instance = new LogService();
		}
		return instance;
	}

	~LogService()
	{
		m_logs.Clear();
		foreach (KeyValuePair<string, LogProxy> item in m_proxy)
		{
			item.Value.Release();
		}
		m_proxy.Clear();
		m_invoke.Clear();
		m_errorInvoke.Clear();
	}

	private LogService()
	{
		Directory.CreateDirectory(GlobalVariable.RootPath + "/Log");
		m_logs = new Dictionary<int, Log>();
		m_proxy = new Dictionary<string, LogProxy>();
		m_invoke = new Dictionary<int, List<InvokeWriteFunc>>();
		m_errorInvoke = new Dictionary<int, List<InvokeWriteFunc>>();
	}

	public int RegistChannel(int channel, string filename, string channelname, bool add = false)
	{
		lock (_object)
		{
			if (channel == -1)
			{
				for (int i = 2; i < 256; i++)
				{
					if (!m_logs.ContainsKey(i))
					{
						channel = i;
						break;
					}
				}
			}
			if (m_logs.ContainsKey(channel))
			{
				return -1;
			}
			LogProxy logProxy = null;
			if (m_proxy.ContainsKey(filename))
			{
				logProxy = m_proxy[filename];
			}
			else
			{
				logProxy = new LogProxy(filename, add);
				m_proxy[filename] = logProxy;
			}
			Log log = new Log(ref logProxy, channelname);
			if (!log.IsVaild())
			{
				return -1;
			}
			m_logs[channel] = log;
			return channel;
		}
	}

	public void UnregistChannel(int channel)
	{
		lock (_object)
		{
			if (m_logs.ContainsKey(channel))
			{
				m_logs[channel] = null;
			}
		}
	}

	public void RegistInvokeFunc(int channel, InvokeWriteFunc func)
	{
		lock (_object)
		{
			if (!m_invoke.ContainsKey(channel))
			{
				m_invoke[channel] = new List<InvokeWriteFunc>();
			}
			if (!m_invoke[channel].Contains(func))
			{
				m_invoke[channel].Add(func);
			}
		}
	}

	public void RegistErrorInvokeFunc(int channel, InvokeWriteFunc func)
	{
		lock (_object)
		{
			if (!m_errorInvoke.ContainsKey(channel))
			{
				m_errorInvoke[channel] = new List<InvokeWriteFunc>();
			}
			if (!m_errorInvoke[channel].Contains(func))
			{
				m_errorInvoke[channel].Add(func);
			}
		}
	}

	public void UnregistInvokeFunc(int channel, InvokeWriteFunc func)
	{
		lock (_object)
		{
			if (m_invoke.ContainsKey(channel))
			{
				m_invoke[channel].Remove(func);
			}
			if (m_errorInvoke.ContainsKey(channel))
			{
				m_errorInvoke[channel].Remove(func);
			}
		}
	}

	public void WriteString(int channel, string fmt, params string[] args)
	{
		lock (_object)
		{
			if (m_logs.ContainsKey(channel))
			{
				m_logs[channel].Write(fmt, args);
			}
			if (!m_invoke.ContainsKey(channel))
			{
				return;
			}
			foreach (InvokeWriteFunc item in m_invoke[channel])
			{
				item(fmt, args);
			}
		}
	}

	public void WriteError(int channel, string fmt, params string[] args)
	{
		lock (_object)
		{
			if (m_logs.ContainsKey(channel))
			{
				m_logs[channel].WriteError(fmt, args);
			}
			if (!m_errorInvoke.ContainsKey(channel))
			{
				return;
			}
			foreach (InvokeWriteFunc item in m_errorInvoke[channel])
			{
				item(fmt, args);
			}
		}
	}
}
