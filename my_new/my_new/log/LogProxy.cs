using System;
using System.IO;
using my_new.utils;

namespace my_new.log;

internal class LogProxy
{
	private FileStream m_file;

	private StreamWriter m_writer;

	private string m_filename;

	private int m_ref;

	public LogProxy(string filename, bool add = false)
	{
		m_filename = filename;
		string path = Path.Combine(GlobalVariable.RootPath, "Log", filename);
		m_file = new FileStream(path, add ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		if (m_file != null)
		{
			m_writer = new StreamWriter(m_file);
			DateTime now = DateTime.Now;
			if (!add)
			{
				m_writer.WriteLine($"[{now:yyyy-MM-dd HH:mm:ss}] Filename: {filename}");
			}
		}
	}

	~LogProxy()
	{
		if (m_file != null)
		{
			m_file.Close();
		}
	}

	public void Release()
	{
		m_ref--;
		if (m_ref == 0 && m_file != null)
		{
			m_file.Close();
		}
	}

	public void IncRef()
	{
		m_ref++;
	}

	public StreamWriter GetFile()
	{
		return m_writer;
	}

	public string GetFilename()
	{
		return m_filename;
	}

	public bool IsValid()
	{
		if (m_file != null)
		{
			return m_writer != null;
		}
		return false;
	}
}
