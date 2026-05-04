using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace my_new;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class InverseBooleanConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (targetType != typeof(Visibility))
		{
			throw new InvalidOperationException("The Target must be a boolean");
		}
		return (!(bool)value) ? Visibility.Hidden : Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
