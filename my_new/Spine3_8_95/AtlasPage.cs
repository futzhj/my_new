namespace Spine3_8_95;

public class AtlasPage
{
	public string name;

	public Format format;

	public TextureFilter minFilter;

	public TextureFilter magFilter;

	public TextureWrap uWrap;

	public TextureWrap vWrap;

	public object rendererObject;

	public int width;

	public int height;

	public AtlasPage Clone()
	{
		return MemberwiseClone() as AtlasPage;
	}
}
