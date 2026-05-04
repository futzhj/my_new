using System;
using System.Runtime.InteropServices;
using my_new.utils;

namespace my_new;

[ComVisible(true)]
public class CustomDocHandler
{
	public void OpenURL(string url)
	{
		Console.WriteLine(url);
		UtilsMethod.OpenPage(url);
	}
}
