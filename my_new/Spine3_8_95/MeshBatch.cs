using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Spine3_8_95;

internal class MeshBatch
{
	private readonly List<MeshItem> items;

	private readonly Queue<MeshItem> freeItems;

	public Point3D[] positions = new Point3D[0];

	public Point[] texcoords = new Point[0];

	public int[] triangles = new int[0];

	private object mutex = new object();

	public MeshBatch()
	{
		items = new List<MeshItem>(256);
		freeItems = new Queue<MeshItem>(256);
	}

	public MeshItem NextItem(int vertexCount, int triangleCount)
	{
		lock (mutex)
		{
			MeshItem meshItem = ((freeItems.Count > 0) ? freeItems.Dequeue() : new MeshItem());
			meshItem.Ensure(vertexCount, triangleCount);
			meshItem.vertexCount = vertexCount;
			meshItem.triangleCount = triangleCount;
			items.Add(meshItem);
			return meshItem;
		}
	}

	public int VertexCount()
	{
		if (items.Count == 0)
		{
			return 0;
		}
		int count = items.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			MeshItem meshItem = items[i];
			num += meshItem.vertexCount;
		}
		return num;
	}

	public int TriangleCount()
	{
		if (items.Count == 0)
		{
			return 0;
		}
		int count = items.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			MeshItem meshItem = items[i];
			num += meshItem.triangleCount / 3;
		}
		return num;
	}

	public void Draw(ViewObject view)
	{
		if (items.Count == 0)
		{
			return;
		}
		int count = items.Count;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			MeshItem meshItem = items[i];
			num += meshItem.vertexCount;
			num2 += meshItem.triangleCount;
		}
		EnsureCapacity(num, num2);
		Rect rect = Rect.Empty;
		num = 0;
		num2 = 0;
		ImageBrush imageBrush = null;
		for (int j = 0; j < count; j++)
		{
			MeshItem meshItem2 = items[j];
			int vertexCount = meshItem2.vertexCount;
			if (meshItem2.brush != imageBrush || num + vertexCount > 32767)
			{
				DrawBatch(view, imageBrush);
				num = 0;
				num2 = 0;
				imageBrush = meshItem2.brush;
			}
			int[] array = meshItem2.triangles;
			int triangleCount = meshItem2.triangleCount;
			int num3 = 0;
			int num4 = num2;
			while (num3 < triangleCount)
			{
				triangles[num4] = (short)(array[num3] + num);
				num3++;
				num4++;
			}
			num2 += triangleCount;
			Array.Copy(meshItem2.positions, 0, positions, num, vertexCount);
			Array.Copy(meshItem2.texcoords, 0, texcoords, num, vertexCount);
			num += vertexCount;
			rect = Rect.Union(meshItem2.getRect(), rect);
		}
		DrawBatch(view, imageBrush);
		Point.Add(rect.TopLeft, new Vector(rect.Width / 2.0, rect.Height / 2.0));
	}

	private void EnsureCapacity(int vertexCount, int triangleCount)
	{
		if (positions.Length < vertexCount)
		{
			positions = new Point3D[vertexCount];
		}
		if (texcoords.Length < vertexCount)
		{
			texcoords = new Point[vertexCount];
		}
		if (triangles.Length < triangleCount)
		{
			triangles = new int[triangleCount];
		}
	}

	public void AfterLastDrawPass()
	{
		int count = items.Count;
		for (int i = 0; i < count; i++)
		{
			MeshItem meshItem = items[i];
			meshItem.brush = null;
			freeItems.Enqueue(meshItem);
		}
		items.Clear();
	}

	public void DrawBatch(ViewObject view, ImageBrush lastBrush)
	{
		view.AddMesh(positions, texcoords, triangles, lastBrush);
		positions = new Point3D[positions.Length];
		texcoords = new Point[texcoords.Length];
		triangles = new int[triangles.Length];
	}
}
