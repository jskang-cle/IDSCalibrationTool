using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace IDSCalibrationTool.Converters;
internal class ZeroToVisibilityConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return value switch
        {
            int i => i == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,
            long l => l == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,
            double d => d == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,
            decimal dec => dec == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,
            _ => System.Windows.Visibility.Collapsed
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
