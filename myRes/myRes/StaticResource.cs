using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace myRes;

public static class StaticResource
{
	public static ComponentResourceKey MainBackgroudBrushKey => new ComponentResourceKey(typeof(StaticResource), "MainBackgroudBrush");

	public static ComponentResourceKey SpineTextureBrushKey => new ComponentResourceKey(typeof(StaticResource), "SpineTextureBrush");

	public static ComponentResourceKey KefuNormalBrushKey => new ComponentResourceKey(typeof(StaticResource), "KefuNormalBrush");

	public static ComponentResourceKey KefuHoverBrushKey => new ComponentResourceKey(typeof(StaticResource), "KefuHoverBrush");

	public static ComponentResourceKey KefuPressedBrushKey => new ComponentResourceKey(typeof(StaticResource), "KefuPressedBrush");

	private static bool SaveToInt(string str, out int res)
	{
		if (int.TryParse(str, out var result))
		{
			res = result;
			return true;
		}
		res = 0;
		return false;
	}

	public static void SetRoleImage(Image img)
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		List<string> list = new List<string>();
		using (Stream stream = executingAssembly.GetManifestResourceStream("myRes.roles.roles_position.txt"))
		{
			using StreamReader streamReader = new StreamReader(stream);
			string text = streamReader.ReadLine();
			while (text != null && text.Length > 0)
			{
				list.Add(text);
				text = streamReader.ReadLine();
			}
		}
		if (list.Count > 0)
		{
			int index = new Random().Next(list.Count);
			string[] array = list[index].Split('\t');
			string arg = array[0];
			string uriString = $"pack://application:,,,/myRes;Component/roles/{arg}";
			string[] array2 = array[1].Split(',');
			int res = 0;
			int res2 = 0;
			if (SaveToInt(array2[0], out res) && SaveToInt(array2[1], out res2))
			{
				Thickness margin = img.Margin;
				double left = margin.Left;
				double top = margin.Top;
				left += (double)res;
				top += (double)res2;
				img.Margin = new Thickness(left, top, 0.0, 0.0);
				BitmapImage bitmapImage = new BitmapImage(new Uri(uriString));
				img.Width = bitmapImage.PixelWidth;
				img.Height = bitmapImage.PixelHeight;
				img.Source = bitmapImage;
				img.Stretch = Stretch.Fill;
			}
		}
	}
}
