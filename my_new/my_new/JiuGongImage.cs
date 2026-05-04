using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace my_new;

internal class JiuGongImage : Image
{
	public static readonly DependencyProperty ClipPaddingProperty = DependencyProperty.Register("ClipPadding", typeof(Thickness), typeof(JiuGongImage), new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsRender));

	public static readonly DependencyProperty ClipRectProperty = DependencyProperty.Register("ClipRect", typeof(Int32Rect), typeof(JiuGongImage), new FrameworkPropertyMetadata(default(Int32Rect), FrameworkPropertyMetadataOptions.AffectsRender));

	public Thickness ClipPadding
	{
		get
		{
			return (Thickness)GetValue(ClipPaddingProperty);
		}
		set
		{
			SetValue(ClipPaddingProperty, value);
		}
	}

	public Int32Rect ClipRect
	{
		get
		{
			return (Int32Rect)GetValue(ClipRectProperty);
		}
		set
		{
			SetValue(ClipRectProperty, value);
		}
	}

	public static ImageSource SplitImage(BitmapSource source, Int32Rect clipRect)
	{
		int stride = clipRect.Width * ((source.Format.BitsPerPixel + 7) / 8);
		int num = clipRect.Width * clipRect.Height;
		Int32Rect sourceRect = new Int32Rect(0, 0, clipRect.Width, clipRect.Height);
		int[] pixels = new int[num];
		source.CopyPixels(clipRect, pixels, stride, 0);
		WriteableBitmap writeableBitmap = new WriteableBitmap(clipRect.Width, clipRect.Height, source.DpiX, source.DpiY, source.Format, source.Palette);
		writeableBitmap.Lock();
		writeableBitmap.WritePixels(sourceRect, pixels, stride, 0);
		writeableBitmap.Unlock();
		return writeableBitmap;
	}

	public static ImageSource[] Get9CellImageSource(BitmapSource source, Int32Rect clipRect)
	{
		ImageSource[] array = new ImageSource[9];
		Int32Rect empty = Int32Rect.Empty;
		int width = source.PixelWidth - clipRect.X - clipRect.Width;
		empty.Width = clipRect.X;
		empty.Height = clipRect.Y;
		array[0] = SplitImage(source, empty);
		empty.X += empty.Width;
		empty.Width = clipRect.Width;
		array[1] = SplitImage(source, empty);
		empty.X += empty.Width;
		empty.Width = width;
		array[2] = SplitImage(source, empty);
		empty = Int32Rect.Empty;
		empty.Y = clipRect.Y;
		empty.Width = clipRect.X;
		empty.Height = clipRect.Height;
		array[3] = SplitImage(source, empty);
		empty.X += empty.Width;
		empty.Width = clipRect.Width;
		array[4] = SplitImage(source, empty);
		empty.X += empty.Width;
		empty.Width = width;
		array[5] = SplitImage(source, empty);
		empty = Int32Rect.Empty;
		empty.Y = clipRect.Y + clipRect.Height;
		empty.Height = source.PixelHeight - clipRect.Height - clipRect.Y;
		empty.Width = clipRect.X;
		array[6] = SplitImage(source, empty);
		empty.X += empty.Width;
		empty.Width = clipRect.Width;
		array[7] = SplitImage(source, empty);
		empty.X += empty.Width;
		empty.Width = width;
		array[8] = SplitImage(source, empty);
		return array;
	}

	protected override void OnRender(DrawingContext dc)
	{
		if (ClipRect != Int32Rect.Empty)
		{
			DrawBitblt(dc);
		}
		else
		{
			RenderWith9Cells(dc);
		}
	}

	private void DrawBitblt(DrawingContext dc)
	{
		RenderWith9Cells(dc);
	}

	private void RenderWith9Cells(DrawingContext dc)
	{
		if (base.Source != null && ClipPadding.Right != 0.0 && ClipPadding.Bottom != 0.0)
		{
			BitmapSource bitmapSource = new BitmapImage(new Uri(base.Source.ToString()));
			if (ClipRect != Int32Rect.Empty)
			{
				bitmapSource = SplitImage(bitmapSource, ClipRect) as BitmapSource;
			}
			double num = (double)bitmapSource.PixelWidth - ClipPadding.Left - ClipPadding.Right;
			double num2 = (double)bitmapSource.PixelHeight - ClipPadding.Top - ClipPadding.Bottom;
			Int32Rect int32Rect = new Int32Rect((int)ClipPadding.Left, (int)ClipPadding.Top, (int)num, (int)num2);
			ImageSource[] array = Get9CellImageSource(bitmapSource, int32Rect);
			if (array == null || array.Length != 9)
			{
				base.OnRender(dc);
			}
			else
			{
				DrawFrame(dc, array, int32Rect);
			}
		}
	}

	private void DrawFrame(DrawingContext drawingContext, ImageSource[] images, Int32Rect contentRect)
	{
		Rect rectangle = new Rect(default(Point), new Size(contentRect.X, contentRect.Y));
		double width = base.ActualWidth - ClipPadding.Left - ClipPadding.Right;
		double height = base.ActualHeight - ClipPadding.Top - ClipPadding.Bottom;
		drawingContext.DrawImage(images[0], rectangle);
		rectangle.X += rectangle.Width;
		rectangle.Width = width;
		drawingContext.DrawImage(images[1], rectangle);
		rectangle.X += rectangle.Width;
		rectangle.Width = ClipPadding.Right;
		drawingContext.DrawImage(images[2], rectangle);
		rectangle.X = 0.0;
		rectangle.Y = contentRect.Y;
		rectangle.Width = contentRect.X;
		rectangle.Height = height;
		drawingContext.DrawImage(images[3], rectangle);
		rectangle.X += rectangle.Width;
		rectangle.Width = width;
		drawingContext.DrawImage(images[4], rectangle);
		rectangle.X += rectangle.Width;
		rectangle.Width = ClipPadding.Right;
		drawingContext.DrawImage(images[5], rectangle);
		rectangle.X = 0.0;
		rectangle.Y = base.ActualHeight - ClipPadding.Bottom;
		rectangle.Width = ClipPadding.Left;
		rectangle.Height = ClipPadding.Bottom;
		drawingContext.DrawImage(images[6], rectangle);
		rectangle.X += rectangle.Width;
		rectangle.Width = width;
		drawingContext.DrawImage(images[7], rectangle);
		rectangle.X += rectangle.Width;
		rectangle.Width = ClipPadding.Right;
		drawingContext.DrawImage(images[8], rectangle);
	}

	private void DrawClip(Rect clipRect, DrawingContext drawingContext, Rect targetRect)
	{
		RectangleGeometry rectangleGeometry = new RectangleGeometry(clipRect);
		rectangleGeometry.Freeze();
		drawingContext.PushClip(rectangleGeometry);
		drawingContext.DrawImage(base.Source, targetRect);
		drawingContext.Pop();
	}
}
