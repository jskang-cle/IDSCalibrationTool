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

using IDSCalibrationTool.Models;

using ReactiveUI;

namespace IDSCalibrationTool;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : IViewFor<MainWindowViewModel>
{
    public MainWindowViewModel? ViewModel { get => DataContext as MainWindowViewModel; set => DataContext = value; }
    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (MainWindowViewModel?)value; }

    private ImageViewWindow? _imageViewWindow;

    public MainWindow(MainWindowViewModel vm)
    {
        ViewModel = vm;
        InitializeComponent();

        Closed += MainWindow_Closed;
    }

    private void MainWindow_Closed(object sender, EventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void ZoomButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe)
        {
            return;
        }

        if (fe.DataContext is not CalibrationImage image)
        {
            return;
        }

        if (_imageViewWindow?.IsLoaded == false)
        {
            _imageViewWindow = null;
        }

        _imageViewWindow ??= new ImageViewWindow();
        _imageViewWindow.DataContext = image;

        if (_imageViewWindow.IsVisible)
        {
            _imageViewWindow.Activate();
            return;
        }

        _imageViewWindow.Show();
    }

    private void ItemCard_MouseLeftButton(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 1)
        {
            return;
        }

        if (sender is FrameworkElement element && element.DataContext is CalibrationImage image)
        {
            if (image.DetectionData?.Success == true)
            {
                image.Selected = !image.Selected;
            }
        }
    }
}
