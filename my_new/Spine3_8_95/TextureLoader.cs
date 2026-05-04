namespace Spine3_8_95;

public interface TextureLoader
{
	void Load(AtlasPage page, string path);

	void Unload(object texture);
}
