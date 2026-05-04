using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace my_new;

public class CustomImageButton : Button
{
	public static readonly DependencyProperty NormalImageProperty;

	public static readonly DependencyProperty HoverImageProperty;

	public static readonly DependencyProperty DisabledImageProperty;

	public static readonly DependencyProperty PressedImageProperty;

	public Brush NormalImage
	{
		get
		{
			return (Brush)GetValue(NormalImageProperty);
		}
		set
		{
			SetValue(NormalImageProperty, value);
		}
	}

	public Brush HoverImage
	{
		get
		{
			return (Brush)GetValue(HoverImageProperty);
		}
		set
		{
			SetValue(HoverImageProperty, value);
		}
	}

	public Brush PressedImage
	{
		get
		{
			return (Brush)GetValue(PressedImageProperty);
		}
		set
		{
			SetValue(PressedImageProperty, value);
		}
	}

	public Brush DisabledImage
	{
		get
		{
			return (Brush)GetValue(DisabledImageProperty);
		}
		set
		{
			SetValue(DisabledImageProperty, value);
		}
	}

	static CustomImageButton()
	{
		NormalImageProperty = DependencyProperty.Register("NormalImage", typeof(Brush), typeof(CustomImageButton));
		HoverImageProperty = DependencyProperty.Register("HoverImage", typeof(Brush), typeof(CustomImageButton));
		DisabledImageProperty = DependencyProperty.Register("DisabledImage", typeof(Brush), typeof(CustomImageButton));
		PressedImageProperty = DependencyProperty.Register("PressedImage", typeof(Brush), typeof(CustomImageButton));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomImageButton), new FrameworkPropertyMetadata(typeof(CustomImageButton)));
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		if (HoverImage == null)
		{
			HoverImage = base.Background;
		}
		if (PressedImage == null)
		{
			if (HoverImage == null)
			{
				PressedImage = base.Background;
			}
			else
			{
				PressedImage = HoverImage;
			}
		}
	}
}
