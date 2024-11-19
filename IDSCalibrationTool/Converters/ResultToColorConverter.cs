using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

using IDSCalibrationTool.Models;

namespace IDSCalibrationTool.Converters;

[ValueConversion(typeof(DetectionResult), typeof(Brush))]
internal class ResultToColorConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DetectionResult result)
        {
            return Brushes.Transparent;
        }

        return result switch
        {
            DetectionResult.Success => Brushes.Green,
            DetectionResult.Failure => Brushes.Red,
            _ => Brushes.Transparent
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
