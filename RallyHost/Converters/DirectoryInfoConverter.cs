using System.Globalization;
using System.IO;
using System;
using Avalonia.Data.Converters;

namespace RallyHost.Converters;

public class DirectoryInfoConverter : IValueConverter
{
    // 将 DirectoryInfo 转换为路径字符串
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value as DirectoryInfo)?.FullName ?? string.Empty;
    }

    // 将路径字符串转换为 DirectoryInfo
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var path = value?.ToString();
        return !string.IsNullOrWhiteSpace(path) ? new DirectoryInfo(path) : null!;
    }
}