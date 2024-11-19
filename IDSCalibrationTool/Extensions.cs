using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using Emgu.CV;

namespace IDSCalibrationTool;
public static class Extensions
{
    [DllImport("gdi32")]
    private static extern int DeleteObject(IntPtr o);

    /// <summary>
    /// Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.Source
    /// </summary>
    /// <param name="image">The Emgu CV Image</param>
    /// <returns>The equivalent BitmapSource</returns>
    public static BitmapSource ToBitmapSource(this Emgu.CV.Mat mat)
    {
        using (System.Drawing.Bitmap source = mat.ToBitmap())
        {
            IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

            BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                ptr,
                IntPtr.Zero,
                Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(ptr); //release the HBitmap
            return bs;
        }
    }

    public static T[][] ToArray2D<T>(this T[] array, int rows, int cols)
    {
        if (array.Length != rows * cols)
        {
            throw new ArgumentException("Array length must be equal to rows * cols");
        }

        T[][] result = new T[rows][];
        for (int i = 0; i < rows; i++)
        {
            result[i] = new T[cols];
            Array.Copy(array, i * cols, result[i], 0, cols);
        }

        return result;
    }

    public static T[] Flatten<T>(this T[][] array)
    {
        List<T> list = new List<T>();
        foreach (var row in array)
        {
            list.AddRange(row);
        }
        return list.ToArray();
    }
}
