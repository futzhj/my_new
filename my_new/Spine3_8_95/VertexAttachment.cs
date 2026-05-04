using System;
using System.Threading.Tasks;

namespace Spine3_8_95;

public abstract class VertexAttachment : Attachment
{
	private static int nextID = 0;

	private static readonly object nextIdLock = new object();

	internal readonly int id;

	internal int[] bones;

	internal float[] vertices;

	internal int worldVerticesLength;

	internal VertexAttachment deformAttachment;

	public int Id => id;

	public int[] Bones
	{
		get
		{
			return bones;
		}
		set
		{
			bones = value;
		}
	}

	public float[] Vertices
	{
		get
		{
			return vertices;
		}
		set
		{
			vertices = value;
		}
	}

	public int WorldVerticesLength
	{
		get
		{
			return worldVerticesLength;
		}
		set
		{
			worldVerticesLength = value;
		}
	}

	public VertexAttachment DeformAttachment
	{
		get
		{
			return deformAttachment;
		}
		set
		{
			deformAttachment = value;
		}
	}

	public VertexAttachment(string name)
		: base(name)
	{
		deformAttachment = this;
		lock (nextIdLock)
		{
			id = (nextID++ & 0xFFFF) << 11;
		}
	}

	public void ComputeWorldVertices(Slot slot, float[] worldVertices)
	{
		ComputeWorldVertices(slot, 0, worldVerticesLength, worldVertices, 0);
	}

	public void ComputeWorldVertices(Slot slot, int start, int count, float[] worldVertices, int offset, int stride = 2)
	{
		count = offset + (count >> 1) * stride;
		Skeleton skeleton = slot.bone.skeleton;
		ExposedList<float> deform = slot.deform;
		float[] vertices = this.vertices;
		int[] array = bones;
		if (array == null)
		{
			if (deform.Count > 0)
			{
				vertices = deform.Items;
			}
			Bone bone = slot.bone;
			float x = bone.worldX;
			float y = bone.worldY;
			float a = bone.a;
			float b = bone.b;
			float c = bone.c;
			float d = bone.d;
			int toExclusive = (count - offset) / stride;
			Parallel.For(0, toExclusive, delegate(int item)
			{
				int num22 = start + 2 * item;
				int num23 = offset + stride * item;
				float num24 = vertices[num22];
				float num25 = vertices[num22 + 1];
				worldVertices[num23] = num24 * a + num25 * b + x;
				worldVertices[num23 + 1] = num24 * c + num25 * d + y;
			});
			return;
		}
		int num = 0;
		int num2 = 0;
		for (int num3 = 0; num3 < start; num3 += 2)
		{
			int num4 = array[num];
			num += num4 + 1;
			num2 += num4;
		}
		Bone[] items = skeleton.bones.Items;
		if (deform.Count == 0)
		{
			_ = (count - offset) / stride;
			int num5 = offset;
			int num6 = num2 * 3;
			for (; num5 < count; num5 += stride)
			{
				float num7 = 0f;
				float num8 = 0f;
				int num9 = array[num++];
				num9 += num;
				while (num < num9)
				{
					Bone bone2 = items[array[num]];
					float num10 = vertices[num6];
					float num11 = vertices[num6 + 1];
					float num12 = vertices[num6 + 2];
					num7 += (num10 * bone2.a + num11 * bone2.b + bone2.worldX) * num12;
					num8 += (num10 * bone2.c + num11 * bone2.d + bone2.worldY) * num12;
					num++;
					num6 += 3;
				}
				worldVertices[num5] = num7;
				worldVertices[num5 + 1] = num8;
			}
			return;
		}
		float[] items2 = deform.Items;
		_ = (count - offset) / stride;
		int num13 = offset;
		int num14 = num2 * 3;
		int num15 = num2 << 1;
		for (; num13 < count; num13 += stride)
		{
			float num16 = 0f;
			float num17 = 0f;
			int num18 = array[num++];
			num18 += num;
			while (num < num18)
			{
				Bone bone3 = items[array[num]];
				float num19 = vertices[num14] + items2[num15];
				float num20 = vertices[num14 + 1] + items2[num15 + 1];
				float num21 = vertices[num14 + 2];
				num16 += (num19 * bone3.a + num20 * bone3.b + bone3.worldX) * num21;
				num17 += (num19 * bone3.c + num20 * bone3.d + bone3.worldY) * num21;
				num++;
				num14 += 3;
				num15 += 2;
			}
			worldVertices[num13] = num16;
			worldVertices[num13 + 1] = num17;
		}
	}

	internal void CopyTo(VertexAttachment attachment)
	{
		if (bones != null)
		{
			attachment.bones = new int[bones.Length];
			Array.Copy(bones, 0, attachment.bones, 0, bones.Length);
		}
		else
		{
			attachment.bones = null;
		}
		if (vertices != null)
		{
			attachment.vertices = new float[vertices.Length];
			Array.Copy(vertices, 0, attachment.vertices, 0, vertices.Length);
		}
		else
		{
			attachment.vertices = null;
		}
		attachment.worldVerticesLength = worldVerticesLength;
		attachment.deformAttachment = deformAttachment;
	}
}
