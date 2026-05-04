using System.Windows;

namespace my_new;

internal struct MARGINS(Thickness t)
{
	public int Left = (int)t.Left;

	public int Right = (int)t.Right;

	public int Top = (int)t.Top;

	public int Bottom = (int)t.Bottom;
}
