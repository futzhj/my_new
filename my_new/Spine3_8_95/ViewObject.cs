using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Spine3_8_95;

public class ViewObject
{
	private Viewport3D viewport;

	private AmbientLight ambientLight;

	private OrthographicCamera camera;

	private Model3DGroup group;

	private double camera_x;

	private double camera_y;

	public Viewport3D view()
	{
		return viewport;
	}

	public ViewObject(int w, int h)
	{
		viewport = new Viewport3D();
		viewport.Width = w;
		viewport.Height = h;
		viewport.RenderSize = new Size(w, h);
		camera = new OrthographicCamera(new Point3D(0.0, 0.0, 1.0), new Vector3D(0.0, 0.0, -1.0), new Vector3D(0.0, -1.0, 0.0), w);
		viewport.Camera = camera;
		group = new Model3DGroup();
		ambientLight = new AmbientLight(Colors.White);
		viewport.Children.Add(new ModelVisual3D
		{
			Content = ambientLight
		});
		viewport.Children.Add(new ModelVisual3D
		{
			Content = group
		});
	}

	private void testViewPort3d()
	{
		int num = 32;
		int num2 = (num + 1) * (num + 1);
		Point3D[] array = new Point3D[num2];
		Vector3D[] array2 = new Vector3D[num2];
		Point[] array3 = new Point[num2];
		for (int i = 0; i <= num; i++)
		{
			double num3 = Math.PI / (double)num * (double)i;
			double y = Math.Cos(num3);
			double num4 = Math.Sin(num3);
			for (int j = 0; j <= num; j++)
			{
				double num5 = Math.PI * 2.0 / (double)num * (double)j;
				double x = num4 * Math.Sin(num5);
				double z = num4 * Math.Cos(num5);
				int num6 = i * (num + 1) + j;
				array[num6] = new Point3D(x, y, z);
				array2[num6] = new Vector3D(x, y, z);
				array3[num6] = new Point((double)j / (double)num, (double)i / (double)num);
			}
		}
		int[] array4 = new int[num * num * 2 * 3];
		for (int k = 0; k < num; k++)
		{
			for (int l = 0; l < num; l++)
			{
				int num7 = (k * num + l) * 6;
				int num8 = k * (num + 1) + l;
				int num9 = num8 + 1;
				int num10 = (k + 1) * (num + 1) + l;
				int num11 = num10 + 1;
				array4[num7] = num8;
				array4[num7 + 1] = num10;
				array4[num7 + 2] = num9;
				array4[num7 + 3] = num9;
				array4[num7 + 4] = num10;
				array4[num7 + 5] = num11;
			}
		}
		MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
		meshGeometry3D.Positions = new Point3DCollection(array);
		meshGeometry3D.Normals = new Vector3DCollection(array2);
		meshGeometry3D.TextureCoordinates = new PointCollection(array3);
		meshGeometry3D.TriangleIndices = new Int32Collection(array4);
		DiffuseMaterial diffuseMaterial = new DiffuseMaterial();
		ImageBrush imageBrush = new ImageBrush();
		imageBrush.ImageSource = new BitmapImage(new Uri("F:\\workspace2022\\trunk\\bywd.jpg", UriKind.Relative));
		diffuseMaterial.Brush = imageBrush;
		GeometryModel3D geometryModel3D = new GeometryModel3D();
		geometryModel3D.Geometry = meshGeometry3D;
		geometryModel3D.Material = diffuseMaterial;
		group.Children.Add(geometryModel3D);
	}

	public void ClearModel()
	{
		group.Children.Clear();
	}

	public void AddMesh(MeshItem meshItem)
	{
		GeometryModel3D geometryModel3D = new GeometryModel3D();
		MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
		meshGeometry3D.Positions = new Point3DCollection(meshItem.positions);
		meshGeometry3D.TextureCoordinates = new PointCollection(meshItem.texcoords);
		Vector3D[] array = new Vector3D[meshItem.positions.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Vector3D(0.0, 0.0, 1.0);
		}
		meshGeometry3D.Normals = new Vector3DCollection(array);
		meshGeometry3D.TriangleIndices = new Int32Collection(meshItem.triangles);
		DiffuseMaterial diffuseMaterial = new DiffuseMaterial();
		diffuseMaterial.Brush = meshItem.brush;
		geometryModel3D.Geometry = meshGeometry3D;
		geometryModel3D.Material = diffuseMaterial;
		group.Children.Add(geometryModel3D);
	}

	public void AddMesh(Point3D[] positions, Point[] texcoords, int[] triangles, ImageBrush brush)
	{
		GeometryModel3D geometryModel3D = new GeometryModel3D();
		MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
		meshGeometry3D.Positions = new Point3DCollection(positions);
		meshGeometry3D.TextureCoordinates = new PointCollection(texcoords);
		meshGeometry3D.TriangleIndices = new Int32Collection(triangles);
		DiffuseMaterial diffuseMaterial = new DiffuseMaterial();
		diffuseMaterial.Brush = brush;
		geometryModel3D.Geometry = meshGeometry3D;
		geometryModel3D.Material = diffuseMaterial;
		group.Children.Add(geometryModel3D);
	}

	public Point GetCameraCenter()
	{
		return new Point(camera_x, camera_y);
	}

	public void SetCameraCenter(Point p)
	{
		camera_x = p.X;
		camera_y = p.Y;
		camera.Position = new Point3D(camera_x, camera_y, 5.0);
	}
}
