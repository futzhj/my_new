using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Spine3_8_95;

internal class SkeletonRenderer
{
	private const int TL = 0;

	private const int TR = 1;

	private const int BL = 2;

	private const int BR = 3;

	private SkeletonClipping clipper = new SkeletonClipping();

	private ViewObject device;

	private MeshBatch batcher;

	private float[] vertices = new float[8];

	private int[] quadTriangles = new int[6] { 0, 1, 2, 2, 3, 0 };

	private bool premultipliedAlpha;

	private float zSpacing;

	public MeshBatch Batcher => batcher;

	public bool PremultipliedAlpha
	{
		get
		{
			return premultipliedAlpha;
		}
		set
		{
			premultipliedAlpha = value;
		}
	}

	public float ZSpacing
	{
		get
		{
			return zSpacing;
		}
		set
		{
			zSpacing = value;
		}
	}

	public SkeletonRenderer(ViewObject device)
	{
		this.device = device;
		batcher = new MeshBatch();
		Bone.yDown = true;
	}

	public void Begin()
	{
	}

	public void End()
	{
		batcher.Draw(device);
		batcher.AfterLastDrawPass();
	}

	public int VertexCount()
	{
		return batcher.VertexCount();
	}

	public int TriangleCount()
	{
		return batcher.TriangleCount();
	}

	public void Draw(Skeleton skeleton)
	{
		ExposedList<Slot> drawOrder = skeleton.DrawOrder;
		Slot[] items = skeleton.DrawOrder.Items;
		float r = skeleton.R;
		float g = skeleton.G;
		float b = skeleton.B;
		float a = skeleton.A;
		int i = 0;
		for (int count = drawOrder.Count; i < count; i++)
		{
			Slot slot = items[i];
			Attachment attachment = slot.Attachment;
			float num = zSpacing * (float)i;
			object obj = null;
			int num2 = 0;
			float[] array = vertices;
			int num3 = 0;
			int[] array2 = null;
			float[] array3 = null;
			float r2;
			float g2;
			float b2;
			float a2;
			if (attachment is RegionAttachment)
			{
				RegionAttachment obj2 = (RegionAttachment)attachment;
				r2 = obj2.R;
				g2 = obj2.G;
				b2 = obj2.B;
				a2 = obj2.A;
				obj = ((AtlasRegion)obj2.RendererObject).page.rendererObject;
				num2 = 4;
				obj2.ComputeWorldVertices(slot.Bone, array, 0);
				num3 = 6;
				array2 = quadTriangles;
				array3 = obj2.UVs;
			}
			else
			{
				if (!(attachment is MeshAttachment))
				{
					if (attachment is ClippingAttachment)
					{
						ClippingAttachment clip = (ClippingAttachment)attachment;
						clipper.ClipStart(slot, clip);
					}
					continue;
				}
				MeshAttachment obj3 = (MeshAttachment)attachment;
				r2 = obj3.R;
				g2 = obj3.G;
				b2 = obj3.B;
				a2 = obj3.A;
				obj = ((AtlasRegion)obj3.RendererObject).page.rendererObject;
				int worldVerticesLength = obj3.WorldVerticesLength;
				if (array.Length < worldVerticesLength)
				{
					array = new float[worldVerticesLength];
				}
				num2 = worldVerticesLength >> 1;
				obj3.ComputeWorldVertices(slot, array);
				num3 = obj3.Triangles.Length;
				array2 = obj3.Triangles;
				array3 = obj3.UVs;
			}
			float num4 = a * slot.A * a2;
			if (premultipliedAlpha)
			{
				Color.FromScRgb(num4, r * slot.R * r2 * num4, g * slot.G * g2 * num4, b * slot.B * b2 * num4);
			}
			else
			{
				Color.FromScRgb(num4, r * slot.R * r2, g * slot.G * g2, b * slot.B * b2);
			}
			Color color = default(Color);
			if (slot.HasSecondColor)
			{
				color = ((!premultipliedAlpha) ? Color.FromScRgb(1f, slot.R2 * num4, slot.G2 * num4, slot.B2 * num4) : Color.FromScRgb(1f, slot.R2 * num4, slot.G2 * num4, slot.B2 * num4));
			}
			color.A = (byte)(premultipliedAlpha ? byte.MaxValue : 0);
			if (clipper.IsClipping)
			{
				clipper.ClipTriangles(array, num2 << 1, array2, num3, array3);
				array = clipper.ClippedVertices.Items;
				num2 = clipper.ClippedVertices.Count >> 1;
				array2 = clipper.ClippedTriangles.Items;
				num3 = clipper.ClippedTriangles.Count;
				array3 = clipper.ClippedUVs.Items;
			}
			if (num2 != 0 && num3 != 0)
			{
				MeshItem meshItem = batcher.NextItem(num2, num3);
				if (obj is ImageBrush)
				{
					meshItem.brush = (ImageBrush)obj;
				}
				int j = 0;
				for (int num5 = num3; j < num5; j++)
				{
					meshItem.triangles[j] = array2[j];
				}
				Point3D[] positions = meshItem.positions;
				Point[] texcoords = meshItem.texcoords;
				int num6 = 0;
				int k = 0;
				for (int num7 = num2 << 1; k < num7; k += 2)
				{
					positions[num6] = new Point3D(array[k], array[k + 1], num);
					texcoords[num6] = new Point(array3[k], array3[k + 1]);
					num6++;
				}
				clipper.ClipEnd(slot);
			}
		}
		clipper.ClipEnd();
	}
}
