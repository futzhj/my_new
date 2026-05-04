namespace my_new.net;

internal class PostPrams
{
	private string m_SendData;

	public PostPrams()
	{
		m_SendData = "";
	}

	public string getParam()
	{
		return m_SendData;
	}

	public PostPrams AddSection(string key, string val)
	{
		if (m_SendData.Length != 0)
		{
			m_SendData += "&";
		}
		m_SendData += key;
		m_SendData += "=";
		m_SendData += val;
		return this;
	}
}
