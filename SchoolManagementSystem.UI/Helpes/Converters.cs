// ============================================================
// Converters.cs
// ============================================================

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SchoolManagementSystem.Converters
{
    public class HexToBrushConverter : IValueConverter
    {
        public static readonly HexToBrushConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var hex = value?.ToString()?.TrimStart('#') ?? "";

                if (hex.Length == 6)
                {
                    return new SolidColorBrush(Color.FromRgb(
                        System.Convert.ToByte(hex[..2], 16),
                        System.Convert.ToByte(hex[2..4], 16),
                        System.Convert.ToByte(hex[4..6], 16)));
                }
            }
            catch { }

            return Brushes.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class NullToVisConverter : IValueConverter
    {
        public static readonly NullToVisConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value == null ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class BoolToVisConverter : IValueConverter
    {
        public static readonly BoolToVisConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}