using System;
using System.Windows;
using System.Windows.Media;
using myRes;

namespace Spine3_8_95;

public class WPFTextureLoaderPackage : TextureLoader
{
	private string[] textureLayerSuffixes;

	public WPFTextureLoaderPackage(bool loadMultipleTextureLayers = false, string[] textureSuffixes = null)
	{
		if (loadMultipleTextureLayers)
		{
			textureLayerSuffixes = textureSuffixes;
		}
	}

	public void Load(AtlasPage page, string path)
	{
		ImageBrush rendererObject = (ImageBrush)new ResourceDictionary
		{
			Source = new Uri("/myRes;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute)
		}[StaticResource.SpineTextureBrushKey];
		if (textureLayerSuffixes == null)
		{
			page.rendererObject = rendererObject;
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
