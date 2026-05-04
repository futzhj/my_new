using System;

namespace Spine3_8_95;

public class RegionAttachment : Attachment, IHasRendererObject
{
	public const int BLX = 0;

	public const int BLY = 1;

	public const int ULX = 2;

	public const int ULY = 3;

	public const int URX = 4;

	public const int URY = 5;

	public const int BRX = 6;

	public const int BRY = 7;

	internal float x;

	internal float y;

	internal float rotation;

	internal float scaleX = 1f;

	internal float scaleY = 1f;

	internal float width;

	internal float height;

	internal float regionOffsetX;

	internal float regionOffsetY;

	internal float regionWidth;

	internal float regionHeight;

	internal float regionOriginalWidth;

	internal float regionOriginalHeight;

	internal float[] offset = new float[8];

	internal float[] uvs = new float[8];

	internal float r = 1f;

	internal float g = 1f;

	internal float b = 1f;

	internal float a = 1f;

	public float X
	{
		get
		{
			return x;
		}
		set
		{
			x = value;
		}
	}

	public float Y
	{
		get
		{
			return y;
		}
		set
		{
			y = value;
		}
	}

	public float Rotation
	{
		get
		{
			return rotation;
		}
		set
		{
			rotation = value;
		}
	}

	public float ScaleX
	{
		get
		{
			return scaleX;
		}
		set
		{
			scaleX = value;
		}
	}

	public float ScaleY
	{
		get
		{
			return scaleY;
		}
		set
		{
			scaleY = value;
		}
	}

	public float Width
	{
		get
		{
			return width;
		}
		set
		{
			width = value;
		}
	}

	public float Height
	{
		get
		{
			return height;
		}
		set
		{
			height = value;
		}
	}

	public float R
	{
		get
		{
			return r;
		}
		set
		{
			r = value;
		}
	}

	public float G
	{
		get
		{
			return g;
		}
		set
		{
			g = value;
		}
	}

	public float B
	{
		get
		{
			return b;
		}
		set
		{
			b = value;
		}
	}

	public float A
	{
		get
		{
			return a;
		}
		set
		{
			a = value;
		}
	}

	public string Path { get; set; }

	public object RendererObject { get; set; }

	public float RegionOffsetX
	{
		get
		{
			return regionOffsetX;
		}
		set
		{
			regionOffsetX = value;
		}
	}

	public float RegionOffsetY
	{
		get
		{
			return regionOffsetY;
		}
		set
		{
			regionOffsetY = value;
		}
	}

	public float RegionWidth
	{
		get
		{
			return regionWidth;
		}
		set
		{
			regionWidth = value;
		}
	}

	public float RegionHeight
	{
		get
		{
			return regionHeight;
		}
		set
		{
			regionHeight = value;
		}
	}

	public float RegionOriginalWidth
	{
		get
		{
			return regionOriginalWidth;
		}
		set
		{
			regionOriginalWidth = value;
		}
	}

	public float RegionOriginalHeight
	{
		get
		{
			return regionOriginalHeight;
		}
		set
		{
			regionOriginalHeight = value;
		}
	}

	public float[] Offset => offset;

	public float[] UVs => uvs;

	public RegionAttachment(string name)
		: base(name)
	{
	}

	public void UpdateOffset()
	{
		float num = width;
		float num2 = height;
		float num3 = num * 0.5f;
		float num4 = num2 * 0.5f;
		float num5 = 0f - num3;
		float num6 = 0f - num4;
		if (regionOriginalWidth != 0f)
		{
			num5 += regionOffsetX / regionOriginalWidth * num;
			num6 += regionOffsetY / regionOriginalHeight * num2;
			num3 -= (regionOriginalWidth - regionOffsetX - regionWidth) / regionOriginalWidth * num;
			num4 -= (regionOriginalHeight - regionOffsetY - regionHeight) / regionOriginalHeight * num2;
		}
		float num7 = scaleX;
		float num8 = scaleY;
		num5 *= num7;
		num6 *= num8;
		num3 *= num7;
		num4 *= num8;
		float degrees = rotation;
		float num9 = MathUtils.CosDeg(degrees);
		float num10 = MathUtils.SinDeg(degrees);
		float num11 = x;
		float num12 = y;
		float num13 = num5 * num9 + num11;
		float num14 = num5 * num10;
		float num15 = num6 * num9 + num12;
		float num16 = num6 * num10;
		float num17 = num3 * num9 + num11;
		float num18 = num3 * num10;
		float num19 = num4 * num9 + num12;
		float num20 = num4 * num10;
		float[] array = offset;
		array[0] = num13 - num16;
		array[1] = num15 + num14;
		array[2] = num13 - num20;
		array[3] = num19 + num14;
		array[4] = num17 - num20;
		array[5] = num19 + num18;
		array[6] = num17 - num16;
		array[7] = num15 + num18;
	}

	public void SetUVs(float u, float v, float u2, float v2, bool rotate)
	{
		float[] array = uvs;
		if (rotate)
		{
			array[4] = u;
			array[5] = v2;
			array[6] = u;
			array[7] = v;
			array[0] = u2;
			array[1] = v;
			array[2] = u2;
			array[3] = v2;
		}
		else
		{
			array[2] = u;
			array[3] = v2;
			array[4] = u;
			array[5] = v;
			array[6] = u2;
			array[7] = v;
			array[0] = u2;
			array[1] = v2;
		}
	}

	public void ComputeWorldVertices(Bone bone, float[] worldVertices, int offset, int stride = 2)
	{
		float[] array = this.offset;
		float worldX = bone.worldX;
		float worldY = bone.worldY;
		float num = bone.a;
		float num2 = bone.b;
		float c = bone.c;
		float d = bone.d;
		float num3 = array[6];
		float num4 = array[7];
		worldVertices[offset] = num3 * num + num4 * num2 + worldX;
		worldVertices[offset + 1] = num3 * c + num4 * d + worldY;
		offset += stride;
		num3 = array[0];
		num4 = array[1];
		worldVertices[offset] = num3 * num + num4 * num2 + worldX;
		worldVertices[offset + 1] = num3 * c + num4 * d + worldY;
		offset += stride;
		num3 = array[2];
		num4 = array[3];
		worldVertices[offset] = num3 * num + num4 * num2 + worldX;
		worldVertices[offset + 1] = num3 * c + num4 * d + worldY;
		offset += stride;
		num3 = array[4];
		num4 = array[5];
		worldVertices[offset] = num3 * num + num4 * num2 + worldX;
		worldVertices[offset + 1] = num3 * c + num4 * d + worldY;
	}

	public override Attachment Copy()
	{
		RegionAttachment regionAttachment = new RegionAttachment(base.Name);
		regionAttachment.RendererObject = RendererObject;
		regionAttachment.regionOffsetX = regionOffsetX;
		regionAttachment.regionOffsetY = regionOffsetY;
		regionAttachment.regionWidth = regionWidth;
		regionAttachment.regionHeight = regionHeight;
		regionAttachment.regionOriginalWidth = regionOriginalWidth;
		regionAttachment.regionOriginalHeight = regionOriginalHeight;
		regionAttachment.Path = Path;
		regionAttachment.x = x;
		regionAttachment.y = y;
		regionAttachment.scaleX = scaleX;
		regionAttachment.scaleY = scaleY;
		regionAttachment.rotation = rotation;
		regionAttachment.width = width;
		regionAttachment.height = height;
		Array.Copy(uvs, 0, regionAttachment.uvs, 0, 8);
		Array.Copy(offset, 0, regionAttachment.offset, 0, 8);
		regionAttachment.r = r;
		regionAttachment.g = g;
		regionAttachment.b = b;
		regionAttachment.a = a;
		return regionAttachment;
	}
}
