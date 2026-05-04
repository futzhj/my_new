using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace my_new;

public class FrameSequence
{
	private float frame_rate = 30f;

	private Rectangle m_rectangle;

	private string m_aniBaseName = "";

	private Storyboard m_storyboard;

	public float FPS()
	{
		return frame_rate;
	}

	public FrameSequence(Rectangle rectangle, string aniName, float f = 30f)
	{
		m_rectangle = rectangle;
		m_aniBaseName = aniName;
		frame_rate = f;
		initStroyBorad();
	}

	private string _getBasePath()
	{
		if (m_aniBaseName.Length != 0)
		{
			return $"pack://application:,,,/Resources/{m_aniBaseName}";
		}
		return "";
	}

	private void initStroyBorad()
	{
		m_storyboard = new Storyboard();
		string text = _getBasePath();
		if (text.Length == 0)
		{
			return;
		}
		for (int i = 0; i < 100; i++)
		{
			Uri uri = new Uri(text + $"{i:D4}.png");
			Stream stream = null;
			try
			{
				stream = Application.GetResourceStream(uri)?.Stream;
			}
			catch
			{
				stream = null;
			}
			if (stream == null)
			{
				break;
			}
			ObjectAnimationUsingKeyFrames objectAnimationUsingKeyFrames = new ObjectAnimationUsingKeyFrames();
			ImageBrush imageBrush = new ImageBrush();
			BitmapImage imageSource = new BitmapImage(uri);
			imageBrush.ImageSource = imageSource;
			imageBrush.Stretch = Stretch.None;
			DiscreteObjectKeyFrame discreteObjectKeyFrame = new DiscreteObjectKeyFrame();
			discreteObjectKeyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds((float)(1000 * i) / frame_rate));
			discreteObjectKeyFrame.Value = imageBrush;
			objectAnimationUsingKeyFrames.KeyFrames.Add(discreteObjectKeyFrame);
			Storyboard.SetTarget(objectAnimationUsingKeyFrames, m_rectangle);
			Storyboard.SetTargetProperty(objectAnimationUsingKeyFrames, new PropertyPath("(Rectangle.Fill)"));
			m_storyboard.Children.Add(objectAnimationUsingKeyFrames);
		}
		m_storyboard.RepeatBehavior = RepeatBehavior.Forever;
		m_storyboard.Begin();
	}
}
