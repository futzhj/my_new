using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Spine3_8_95;

namespace my_new;

internal class SpineController
{
	private static float F = 30f;

	private Viewport3D viewport3D;

	private SkeletonRenderer skeletonRenderer;

	private ViewObject viewObject;

	private List<SpineObj> m_spines;

	private DispatcherTimer timer;

	private Canvas m_p;

	private float m_frame_time;

	public float frame_time => 0f;

	public float fps
	{
		get
		{
			if (m_frame_time == 0f)
			{
				return 0f;
			}
			return Math.Min(1000f / frame_time, F);
		}
	}

	public SpineController(Canvas canvas, Point view_position, Size viewSize)
	{
		m_p = canvas;
		if (m_p != null)
		{
			initView(view_position, viewSize);
		}
		m_spines = new List<SpineObj>();
		timer = new DispatcherTimer();
		timer.Interval = TimeSpan.FromSeconds(1.0 / 30.0);
		timer.Tick += Draw;
		timer.Start();
	}

	private void initView(Point view_position, Size viewSize)
	{
		viewObject = new ViewObject((int)viewSize.Width, (int)viewSize.Height);
		viewport3D = viewObject.view();
		viewport3D.SetValue(Panel.ZIndexProperty, -1);
		skeletonRenderer = new SkeletonRenderer(viewObject);
		viewObject.SetCameraCenter(new Point(0.0, 0.0));
		viewObject.ClearModel();
		m_p.Children.Add(viewport3D);
		Canvas.SetLeft(viewport3D, view_position.X);
		Canvas.SetLeft(viewport3D, view_position.Y);
	}

	private void Draw(object sender, EventArgs e)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		skeletonRenderer.Begin();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		lock (viewport3D)
		{
			foreach (SpineObj spine in m_spines)
			{
				spine.onUpdate(30f);
				spine.onDraw(skeletonRenderer);
				num += spine.BonesCount();
				num2 += spine.VertexCount();
				num3 += spine.TriangleCount();
			}
		}
		viewObject.ClearModel();
		skeletonRenderer.End();
		stopwatch.Stop();
		m_frame_time = stopwatch.ElapsedMilliseconds;
		Console.WriteLine($"frame_time {m_frame_time}ms");
	}

	public void AddSpine(string atlas, string skel, int x = 0, int y = 0)
	{
		SpineObj spineObj = new SpineObjOther(atlas, skel, package: true);
		spineObj.MoveTo(x, y);
		m_spines.Add(spineObj);
	}

	public void Pause()
	{
		timer.Stop();
	}

	public void Resume()
	{
		timer.Start();
	}

	public void Release()
	{
		m_spines.Clear();
		timer.Stop();
	}
}
