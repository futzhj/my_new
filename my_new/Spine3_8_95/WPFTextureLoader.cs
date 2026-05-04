using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Spine3_8_95;

public class WPFTextureLoader : TextureLoader
{
	private string[] textureLayerSuffixes;

	public WPFTextureLoader(bool loadMultipleTextureLayers = false, string[] textureSuffixes = null)
	{
		if (loadMultipleTextureLayers)
		{
			textureLayerSuffixes = textureSuffixes;
		}
	}

	public void Load(AtlasPage page, string path)
	{
		ImageBrush imageBrush = new ImageBrush();
		BitmapImage bitmapImage = (BitmapImage)(imageBrush.ImageSource = new BitmapImage(new Uri(path)));
		if (page.width == 0 || page.height == 0)
		{
			page.width = bitmapImage.PixelWidth;
			page.height = bitmapImage.PixelHeight;
		}
		if (textureLayerSuffixes == null)
		{
			page.rendererObject = imageBrush;
		}
	}

	public void Unload(object texture)
	{
		((ImageBrush)texture).ImageSource = null;
	}

	private string GetLayerName(string firstLayerPath, string firstLayerSuffix, string replacementSuffix)
	{
		int num = firstLayerPath.LastIndexOf(firstLayerSuffix + ".");
		if (num == -1)
		{
			throw new Exception("Error composing texture layer name: first texture layer name '" + firstLayerPath + "' does not contain suffix to be replaced: '" + firstLayerSuffix + "'");
		}
		return firstLayerPath.Remove(num, firstLayerSuffix.Length).Insert(num, replacementSuffix);
	}
}
