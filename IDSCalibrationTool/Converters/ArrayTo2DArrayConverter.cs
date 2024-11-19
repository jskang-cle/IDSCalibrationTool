﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace IDSCalibrationTool.Converters;
internal class ArrayTo2DArrayConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not double[] array)
        {
            return DependencyProperty.UnsetValue;
        }

        return new double[][] { array };
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