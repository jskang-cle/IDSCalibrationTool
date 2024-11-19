using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace IDSCalibrationTool.Converters;
internal class TwoDimArrayConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double[,] arr)
        {
            return GetBindable2DArray(arr);
        }

        if (value is float[,] arr2)
        {
            return GetBindable2DArray(arr2);
        }

        return null;
    }

    public static DataView GetBindable2DArray<T>(T[,] array)
    {
        DataTable dataTable = new DataTable();
        for (int i = 0; i < array.GetLength(1); i++)
        {
            dataTable.Columns.Add(i.ToString(), typeof(T));
        }
        for (int i = 0; i < array.GetLength(0); i++)
        {
            DataRow dataRow = dataTable.NewRow();
            dataTable.Rows.Add(dataRow);
        }
        DataView dataView = new DataView(dataTable);
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                int a = i;
                int b = j;
                dataView[i][j] = array[a, b];
            }
        }
        return dataView;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
