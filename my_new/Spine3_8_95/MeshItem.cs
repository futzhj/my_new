using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Spine3_8_95;

public class MeshItem
{
	public Point3D[] positions = new Point3D[0];

	public Point[] texcoords = new Point[0];

	public int[] triangles = new int[0];

	public int vertexCount;

	public int triangleCount;

	public ImageBrush brush;

	public Rect getRect()
	{
		if (positions.Length == 0)
		{
			return Rect.Empty;
		}
		Rect rect = new Rect(new Point(positions[0].X, positions[0].Y), new Size(0.0, 0.0));
		Point3D[] array = positions;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3D vector3D = (Vector3D)array[i];
			rect = Rect.Union(rect, new Point(vector3D.X, vector3D.Y));
		}
		return rect;
	}

	public void Ensure(int vertexCount, int triangleCount)
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
}
