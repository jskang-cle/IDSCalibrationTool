using System.IO;
using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;

namespace IDSCalibrationTool.Models;

public partial class CalibrationImage : ObservableObject
{
    public string Path { get; }
    public string FileName { get; }

    [ObservableProperty]
    private bool _selected;

    [ObservableProperty]
    private Uri _imageSource;

    [ObservableProperty]
    private CharucoDetectionData? _detectionData;

    public CalibrationImage(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("File not found", path);
        }

        Path = path;
        FileName = System.IO.Path.GetFileName(path);
        ImageSource = new Uri(Path);
    }

    public void Reset()
    {
        ImageSource = new Uri(Path);
        DetectionData = null;
        Selected = false;
    }
}

public enum DetectionResult
{
    NotCalibrated,
    Success,
    Failure
}