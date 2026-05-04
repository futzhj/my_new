using System.IO;

namespace Spine3_8_95;

public class Utils
{
	public static bool IsBinaryData(string path)
	{
		if (File.Exists(path.Replace(".atlas", ".skel")) && path.IndexOf(".skel") > -1)
		{
			return true;
		}
		return false;
	}
}
