using System;
using System.IO;
using System.Net;
using System.Text;

namespace my_new.net;

internal class HttpPoster
{
	public static string SendGet(string url, string param, string encoding, string header = "")
	{
		string text = "";
		try
		{
			string requestUriString = ((!url.EndsWith("?")) ? (url + "?" + param) : (url + param));
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
			httpWebRequest.KeepAlive = false;
			if (header.Length > 0)
			{
				httpWebRequest.Headers.Add(header);
			}
			httpWebRequest.Method = "GET";
			httpWebRequest.ContentType = "text/html;charset=" + encoding;
			httpWebRequest.Accept = "*/*";
			HttpWebResponse obj = (HttpWebResponse)httpWebRequest.GetResponse();
			Stream responseStream = obj.GetResponseStream();
			StreamReader streamReader = new StreamReader(responseStream, Encoding.GetEncoding(encoding));
			text = streamReader.ReadToEnd();
			streamReader.Close();
			responseStream.Close();
			obj.Close();
		}
		catch (Exception)
		{
			text = "";
		}
		return text;
	}

	public static byte[] SendDownload(string url, string param, string header = "")
	{
		Stream stream = null;
		byte[] result = null;
		try
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url.EndsWith("?") ? (url + param) : (url + "?" + param));
			httpWebRequest.KeepAlive = false;
			httpWebRequest.Method = "GET";
			httpWebRequest.Accept = "*/*";
			if (!string.IsNullOrEmpty(header))
			{
				httpWebRequest.Headers.Add(header);
			}
			stream = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream();
			using MemoryStream memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			result = memoryStream.ToArray();
		}
		catch (Exception)
		{
			result = null;
		}
		finally
		{
			stream?.Close();
		}
		return result;
	}
}
