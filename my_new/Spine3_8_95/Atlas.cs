using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Spine3_8_95;

public class Atlas : IEnumerable<AtlasRegion>, IEnumerable
{
	private readonly List<AtlasPage> pages = new List<AtlasPage>();

	private List<AtlasRegion> regions = new List<AtlasRegion>();

	private TextureLoader textureLoader;

	public IEnumerator<AtlasRegion> GetEnumerator()
	{
		return regions.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return regions.GetEnumerator();
	}

	public Atlas(string path, TextureLoader textureLoader)
	{
		using StreamReader reader = new StreamReader(path);
		try
		{
			Load(reader, Path.GetDirectoryName(path), textureLoader);
		}
		catch (Exception innerException)
		{
			throw new Exception("Error reading atlas file: " + path, innerException);
		}
	}

	public Atlas(TextReader reader, string dir, TextureLoader textureLoader)
	{
		Load(reader, dir, textureLoader);
	}

	public Atlas(List<AtlasPage> pages, List<AtlasRegion> regions)
	{
		this.pages = pages;
		this.regions = regions;
		textureLoader = null;
	}

	private void Load(TextReader reader, string imagesDir, TextureLoader textureLoader)
	{
		if (textureLoader == null)
		{
			throw new ArgumentNullException("textureLoader", "textureLoader cannot be null.");
		}
		this.textureLoader = textureLoader;
		string[] array = new string[4];
		AtlasPage atlasPage = null;
		while (true)
		{
			string text = reader.ReadLine();
			if (text == null)
			{
				break;
			}
			if (text.Trim().Length == 0)
			{
				atlasPage = null;
				continue;
			}
			if (atlasPage == null)
			{
				atlasPage = new AtlasPage();
				atlasPage.name = text;
				if (ReadTuple(reader, array) == 2)
				{
					atlasPage.width = int.Parse(array[0], CultureInfo.InvariantCulture);
					atlasPage.height = int.Parse(array[1], CultureInfo.InvariantCulture);
					ReadTuple(reader, array);
				}
				atlasPage.format = (Format)Enum.Parse(typeof(Format), array[0], ignoreCase: false);
				ReadTuple(reader, array);
				atlasPage.minFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), array[0], ignoreCase: false);
				atlasPage.magFilter = (TextureFilter)Enum.Parse(typeof(TextureFilter), array[1], ignoreCase: false);
				string text2 = ReadValue(reader);
				atlasPage.uWrap = TextureWrap.ClampToEdge;
				atlasPage.vWrap = TextureWrap.ClampToEdge;
				switch (text2)
				{
				case "x":
					atlasPage.uWrap = TextureWrap.Repeat;
					break;
				case "y":
					atlasPage.vWrap = TextureWrap.Repeat;
					break;
				case "xy":
					atlasPage.uWrap = (atlasPage.vWrap = TextureWrap.Repeat);
					break;
				}
				textureLoader.Load(atlasPage, Path.Combine(imagesDir, text));
				pages.Add(atlasPage);
				continue;
			}
			AtlasRegion atlasRegion = new AtlasRegion();
			atlasRegion.name = text;
			atlasRegion.page = atlasPage;
			string text3 = ReadValue(reader);
			if (text3 == "true")
			{
				atlasRegion.degrees = 90;
			}
			else if (text3 == "false")
			{
				atlasRegion.degrees = 0;
			}
			else
			{
				atlasRegion.degrees = int.Parse(text3);
			}
			atlasRegion.rotate = atlasRegion.degrees == 90;
			ReadTuple(reader, array);
			int num = int.Parse(array[0], CultureInfo.InvariantCulture);
			int num2 = int.Parse(array[1], CultureInfo.InvariantCulture);
			ReadTuple(reader, array);
			int num3 = int.Parse(array[0], CultureInfo.InvariantCulture);
			int num4 = int.Parse(array[1], CultureInfo.InvariantCulture);
			atlasRegion.u = (float)num / (float)atlasPage.width;
			atlasRegion.v = (float)num2 / (float)atlasPage.height;
			if (atlasRegion.rotate)
			{
				atlasRegion.u2 = (float)(num + num4) / (float)atlasPage.width;
				atlasRegion.v2 = (float)(num2 + num3) / (float)atlasPage.height;
			}
			else
			{
				atlasRegion.u2 = (float)(num + num3) / (float)atlasPage.width;
				atlasRegion.v2 = (float)(num2 + num4) / (float)atlasPage.height;
			}
			atlasRegion.x = num;
			atlasRegion.y = num2;
			atlasRegion.width = Math.Abs(num3);
			atlasRegion.height = Math.Abs(num4);
			if (ReadTuple(reader, array) == 4)
			{
				atlasRegion.splits = new int[4]
				{
					int.Parse(array[0], CultureInfo.InvariantCulture),
					int.Parse(array[1], CultureInfo.InvariantCulture),
					int.Parse(array[2], CultureInfo.InvariantCulture),
					int.Parse(array[3], CultureInfo.InvariantCulture)
				};
				if (ReadTuple(reader, array) == 4)
				{
					atlasRegion.pads = new int[4]
					{
						int.Parse(array[0], CultureInfo.InvariantCulture),
						int.Parse(array[1], CultureInfo.InvariantCulture),
						int.Parse(array[2], CultureInfo.InvariantCulture),
						int.Parse(array[3], CultureInfo.InvariantCulture)
					};
					ReadTuple(reader, array);
				}
			}
			atlasRegion.originalWidth = int.Parse(array[0], CultureInfo.InvariantCulture);
			atlasRegion.originalHeight = int.Parse(array[1], CultureInfo.InvariantCulture);
			ReadTuple(reader, array);
			atlasRegion.offsetX = int.Parse(array[0], CultureInfo.InvariantCulture);
			atlasRegion.offsetY = int.Parse(array[1], CultureInfo.InvariantCulture);
			atlasRegion.index = int.Parse(ReadValue(reader), CultureInfo.InvariantCulture);
			regions.Add(atlasRegion);
		}
	}

	private static string ReadValue(TextReader reader)
	{
		string text = reader.ReadLine();
		int num = text.IndexOf(':');
		if (num == -1)
		{
			throw new Exception("Invalid line: " + text);
		}
		return text.Substring(num + 1).Trim();
	}

	private static int ReadTuple(TextReader reader, string[] tuple)
	{
		string text = reader.ReadLine();
		int num = text.IndexOf(':');
		if (num == -1)
		{
			throw new Exception("Invalid line: " + text);
		}
		int i = 0;
		int num2 = num + 1;
		for (; i < 3; i++)
		{
			int num3 = text.IndexOf(',', num2);
			if (num3 == -1)
			{
				break;
			}
			tuple[i] = text.Substring(num2, num3 - num2).Trim();
			num2 = num3 + 1;
		}
		tuple[i] = text.Substring(num2).Trim();
		return i + 1;
	}

	public void FlipV()
	{
		int i = 0;
		for (int count = regions.Count; i < count; i++)
		{
			AtlasRegion atlasRegion = regions[i];
			atlasRegion.v = 1f - atlasRegion.v;
			atlasRegion.v2 = 1f - atlasRegion.v2;
		}
	}

	public AtlasRegion FindRegion(string name)
	{
		int i = 0;
		for (int count = regions.Count; i < count; i++)
		{
			if (regions[i].name == name)
			{
				return regions[i];
			}
		}
		return null;
	}

	public void Dispose()
	{
		if (textureLoader != null)
		{
			int i = 0;
			for (int count = pages.Count; i < count; i++)
			{
				textureLoader.Unload(pages[i].rendererObject);
			}
		}
	}
}
