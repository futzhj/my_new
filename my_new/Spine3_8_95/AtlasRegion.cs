namespace Spine3_8_95;

public class AtlasRegion
{
	public AtlasPage page;

	public string name;

	public int x;

	public int y;

	public int width;

	public int height;

	public float u;

	public float v;

	public float u2;

	public float v2;

	public float offsetX;

	public float offsetY;

	public int originalWidth;

	public int originalHeight;

	public int index;

	public bool rotate;

	public int degrees;

	public int[] splits;

	public int[] pads;

	public AtlasRegion Clone()
	{
		return MemberwiseClone() as AtlasRegion;
	}
}
