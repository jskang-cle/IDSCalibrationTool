using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

using DynamicData;
using DynamicData.Binding;

using Emgu.CV;
using Emgu.CV.Util;

using IDSCalibrationTool.Models;

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace IDSCalibrationTool;
public partial class MainWindowViewModel : ReactiveObject
{
    const string CHARUCO_CONFIG_FILE = "charuco_config.json";

    [Reactive]
    private string _title = "IDS Calibration Tool";

    [Reactive(SetModifier = AccessModifier.Private)]
    private string? _selectedFolder;

    [Reactive(SetModifier = AccessModifier.Private)]
    private CalibrationResult? _result;

    [Reactive(SetModifier = AccessModifier.Private)]
    private bool _running;

    public CharucoParams CharucoBoardConfig { get; }

    private CharucoCalibrator Calibrator { get; }

    private SourceList<CalibrationImage> ImagesSource { get; } = new();

    public ReadOnlyObservableCollection<CalibrationImage> Images { get; private set; }

    public MainWindowViewModel()
    {
        Calibrator = App.GetService<CharucoCalibrator>();

        if (File.Exists(CHARUCO_CONFIG_FILE))
        {
            CharucoBoardConfig = CharucoParams.Load(CHARUCO_CONFIG_FILE);
        }
        else
        {
            CharucoBoardConfig = new()
            {
                SquaresX = 8,
                SquaresY = 6,
                SquareLength = 0.015f,
                MarkerLength = 0.011f,
                DictionaryName = Emgu.CV.Aruco.Dictionary.PredefinedDictionaryName.Dict4X4_250
            };
            CharucoBoardConfig.Save(CHARUCO_CONFIG_FILE);
        }

        CharucoBoardConfig
            .WhenAnyPropertyChanged()
            .Subscribe(_ => CharucoBoardConfig.Save(CHARUCO_CONFIG_FILE));

        ImagesSource.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out var _images)
            .Subscribe();

        Observable.Merge(
            ImagesSource.Connect()
                .WhenValueChanged(x => x.Selected)
                .Select(x => Unit.Default),
            ImagesSource.Connect()
                .Select(x => Unit.Default)
            )
            .Throttle(TimeSpan.FromMilliseconds(100))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x => Calibrate());

        Images = _images;

        _saveParamsCanExecute = this.WhenAnyValue(x => x.Result).Select(x => x != null);

        this.WhenAnyValue(x => x.SelectedFolder)
            .WhereNotNull()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(LoadImages);

#if DEBUG
        // SelectedFolder = @"D:\underbody\cam_intrinsic_calib\charuco";
#endif
    }

    [ReactiveCommand]
    public void SelectFolder()
    {
        var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
        if (dialog.ShowDialog() == true)
        {
            SelectedFolder = dialog.SelectedPath;
        }
    }

    [ReactiveCommand]
    private void OpenSelectedFolder()
    {
        if (SelectedFolder is not null)
        {
            System.Diagnostics.Process.Start(SelectedFolder);
        }
    }

    private void LoadImages(string folder)
    {
        ImagesSource.Clear();

        var files = Directory.GetFiles(folder, "*.png");

        ImagesSource.Edit(innerList =>
        {
            innerList.AddRange(files.Select(x => new CalibrationImage(x)));
        });

        Result = null;

        DetectAll();
    }

    [ReactiveCommand]
    private async Task DetectAll()
    {
        if (Running)
        {
            return;
        }

        if (ImagesSource.Count == 0)
        {
            MessageBox.Show("No images to detect", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Running = true;

        try
        {
            await Calibrator.CharucoDetectAllAsync(CharucoBoardConfig, ImagesSource.Items.Where(x => x.DetectionData is null));
        }
        finally
        {
            Running = false;
        }
    }

    [ReactiveCommand]
    private void Calibrate()
    {
        if (ImagesSource.Count == 0)
        {
            MessageBox.Show("No images to calibrate", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Result = Calibrator.Calibrate(CharucoBoardConfig, ImagesSource.Items);

        SaveParams();
    }

    [ReactiveCommand]
    private void Reset()
    {
        foreach (var item in ImagesSource.Items)
        {
            item.Reset();
        }

        Result = null;
    }

    private IObservable<bool> _saveParamsCanExecute;

    [ReactiveCommand(CanExecute = nameof(_saveParamsCanExecute))]
    private void SaveParams(string? path = null)
    {
        if (Result is null)
        {
            // MessageBox.Show("No calibration result to save", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        path ??= Path.Combine(SelectedFolder, "calibration.yaml");

        using FileStorage storage = new FileStorage(path, FileStorage.Mode.Write, "utf-8");

        storage.Write(Result.ReprojectionError, "reprojectionError");

        // create mat from double[][]
        double[,] cameraMatrixArray = Result.CameraMatrix;
        using Matrix<double> cameraMatrix = new Matrix<double>(cameraMatrixArray);

        storage.Write(cameraMatrix.Mat, "cameraMatrix");

        using Mat distCoeffs = new Mat(1, 5, Emgu.CV.CvEnum.DepthType.Cv64F, 1);
        distCoeffs.SetTo(Result.DistCoeffs);

        storage.Write(distCoeffs, "distCoeffs");
    }

    [ReactiveCommand(CanExecute = nameof(_saveParamsCanExecute))]
    private void SaveParamsAs()
    {
        SaveFileDialog dialog = new()
        {
            Filter = "YAML files (*.yaml)|*.yaml|All files (*.*)|*.*",
            DefaultExt = "yaml"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            SaveParams(dialog.FileName);
        }
    }

    [ReactiveCommand]
    private void SelectAll()
    {
        foreach (var item in ImagesSource.Items)
        {
            if (item.DetectionData?.Success == true)
            {
                item.Selected = true;
            }
        }
    }

    [ReactiveCommand]
    private void DeselectAll()
    {
        foreach (var item in ImagesSource.Items)
        {
            item.Selected = false;
        }
    }

    [ReactiveCommand]
    private void RefreshImages()
    {
        if (Running || SelectedFolder is null)
        {
            return;
        }

        Running = true;

        try
        {
            var files = Directory.GetFiles(SelectedFolder, "*.png");
            var filesSet = files.ToHashSet();

            ImagesSource.Edit(innerList =>
            {
                var existingFiles = innerList.Select(x => x.Path);
                var filesToRemove = existingFiles.Except(filesSet).ToList();
                var filesToAdd = filesSet.Except(existingFiles).ToList();

                foreach (var file in filesToRemove)
                {
                    innerList.Remove(innerList.First(x => x.Path == file));
                }

                foreach (var file in filesToAdd)
                {
                    innerList.Add(new CalibrationImage(file));
                }
            });
        }
        finally
        {
            Running = false;
        }

        DetectAll();
    }

    [ReactiveCommand]
    private void DeleteImage(object commandParameter)
    {
        if (commandParameter is not CalibrationImage image)
        {
            return;
        }

        ImagesSource.Remove(image);

        Task.Delay(100)
            .ContinueWith(_ =>
            {
                string deletedDirectory = Path.Combine(Path.GetDirectoryName(image.Path) ?? string.Empty, "deleted");
                Directory.CreateDirectory(deletedDirectory);

                string deletedPath = Path.Combine(deletedDirectory, image.FileName);
                File.Move(image.Path, deletedPath);
            });
    }
}
