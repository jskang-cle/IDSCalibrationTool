using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace IDSCalibrationTool.Converters;

[ValueConversion(typeof(Enum), typeof(IEnumerable<object>))]
public class EnumToCollectionConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Enum.GetValues(value.GetType());
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}