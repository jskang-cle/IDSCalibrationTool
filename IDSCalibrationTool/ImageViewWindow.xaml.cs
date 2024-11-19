using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IDSCalibrationTool;
/// <summary>
/// Interaction logic for ImageViewWindow.xaml
/// </summary>
public partial class ImageViewWindow
{
    public ImageViewWindow()
    {
        InitializeComponent();

        this.PreviewKeyUp += ImageViewWindow_PreviewKeyUp;
    }

    private void ImageViewWindow_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        // R key
        if (e.Key == Key.R)
        {
            ImageZoomBorder.Reset();
        }
        else if (e.Key == Key.Q)
        {
            this.Close();
        }
    }
}
