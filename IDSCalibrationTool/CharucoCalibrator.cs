using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Emgu.CV;
using Emgu.CV.Aruco;
using Emgu.CV.Ocl;
using Emgu.CV.Structure;
using Emgu.CV.Util;

using IDSCalibrationTool.Models;

using Microsoft.Extensions.Logging;

using Serilog;

namespace IDSCalibrationTool;
public class CharucoCalibrator
{
    private static ILogger<CharucoCalibrator> _logger;

    public CharucoCalibrator(ILogger<CharucoCalibrator> logger)
    {
        _logger = logger;
    }

    public async Task CharucoDetectAllAsync(CharucoParams config, IEnumerable<CalibrationImage> images)
    {
        Size imageSize = new(800, 600);

        foreach (var image in images)
        {
            CharucoDetectionData result = await Task.Run(() => CharucoDetect(image, config));
            image.DetectionData = result;
            image.Selected = result.Success;
        }
    }

    private CharucoDetectionData CharucoDetect(CalibrationImage image, CharucoParams config)
    {
        var result = new CharucoDetectionData();

        using Dictionary dict = new Dictionary(config.DictionaryName);
        using CharucoBoard board = new CharucoBoard(config.SquaresX, config.SquaresY, config.SquareLength, config.MarkerLength, dict);

        using Mat imageMat = CvInvoke.Imread(image.Path);
        using Mat gray = new Mat();

        result.ImageSize = imageMat.Size;

        CvInvoke.CvtColor(imageMat, gray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

        using VectorOfVectorOfPointF markerCorners = new();
        using VectorOfInt markerIds = new();

        DetectorParameters detectorParams = DetectorParameters.GetDefault();
        ArucoInvoke.DetectMarkers(gray, dict, markerCorners, markerIds, detectorParams);

        result.ArucoMarkersFound = markerCorners.Size;
        result.ArucoMarkersExpected = config.MarkerCount;

        ArucoInvoke.DrawDetectedMarkers(imageMat, markerCorners, markerIds, new MCvScalar(0, 0, 255));

        if (markerCorners.Size != config.MarkerCount)
        {
            _logger.LogWarning("Expected {Expected} markers, but found {Found} in {FileName}", config.MarkerCount, markerCorners.Size, image.FileName);
            WriteAndSetOutputImage(image, imageMat);
            return result;
        }

        _logger.LogInformation("Found {Found} markers in {FileName}", markerCorners.Size, image.FileName);

        using VectorOfPointF charucoCorners = new();
        using VectorOfInt charucoIds = new();

        ArucoInvoke.InterpolateCornersCharuco(markerCorners, markerIds, gray, board, charucoCorners, charucoIds);

        if (charucoIds.Size > 0)
        {
            ArucoInvoke.DrawDetectedCornersCharuco(imageMat, charucoCorners, charucoIds, new MCvScalar(0, 255, 0));
            WriteAndSetOutputImage(image, imageMat);

            result.CharucoCorners = charucoCorners.ToArray();
            result.CharucoIds = charucoIds.ToArray();
        }

        if (charucoIds.Size == (config.SquaresX-1) * (config.SquaresY - 1))
        {
            _logger.LogInformation("Found {Found} charuco corners in {FileName}", charucoIds.Size, image.FileName);

            result.CharucoCorners = charucoCorners.ToArray();
            result.CharucoIds = charucoIds.ToArray();
        }
        else
        {
            _logger.LogWarning("Expected {Expected} charuco corners, but found {Found} in {FileName}", config.MarkerCount, charucoIds.Size, image.FileName);
        }

        result.Success = true;
        return result;
    }

    private void WriteAndSetOutputImage(CalibrationImage image, Mat imageMat)
    {
        string imageOutputPath = Path.Combine(Path.GetDirectoryName(image.Path) ?? string.Empty, "output", image.FileName);
        Directory.CreateDirectory(Path.GetDirectoryName(imageOutputPath) ?? string.Empty);
        CvInvoke.Imwrite(imageOutputPath, imageMat);
        image.ImageSource = new Uri(imageOutputPath);
    }

    public CalibrationResult? Calibrate(CharucoParams config, IEnumerable<CalibrationImage> images)
    {
        List<PointF[]> allCharucoCorners = new();
        List<int[]> allCharucoIds = new();

        Size imageSize = new();

        foreach (var image in images)
        {
            if (!image.Selected)
            {
                continue;
            }

            if (image.DetectionData?.CharucoCorners is not null && image.DetectionData.CharucoIds is not null)
            {
                allCharucoCorners.Add(image.DetectionData.CharucoCorners);
                allCharucoIds.Add(image.DetectionData.CharucoIds);
                imageSize = image.DetectionData.ImageSize;
            }
        }

        if (allCharucoCorners.Count == 0)
        {
            return null;
        }

        using Dictionary dict = new Dictionary(config.DictionaryName);
        using CharucoBoard board = new CharucoBoard(config.SquaresX, config.SquaresY, config.SquareLength, config.MarkerLength, dict);

        using VectorOfVectorOfPointF corners = new VectorOfVectorOfPointF(allCharucoCorners.ToArray());
        using VectorOfVectorOfInt ids = new VectorOfVectorOfInt(allCharucoIds.ToArray());

        using Matrix<double> cameraMatrix = new Matrix<double>(3, 3);
        using Mat distCoeffs = new Mat();

        using VectorOfMat rvecs = new();
        using VectorOfMat tvecs = new();

        var critera = new MCvTermCriteria(30, 0.001);

        double repError = ArucoInvoke.CalibrateCameraCharuco(corners, ids, board, imageSize, cameraMatrix, distCoeffs, rvecs, tvecs, Emgu.CV.CvEnum.CalibType.Default, critera);

        double[] distCoeffsOutput = new double[5];
        distCoeffs.CopyTo(distCoeffsOutput);

        return new CalibrationResult()
        {
            ReprojectionError = repError,
            CameraMatrix = cameraMatrix.Data,
            DistCoeffs = distCoeffsOutput,
        };
    }
}

public sealed record class CalibrationResult
{
    public required double ReprojectionError { get; init; }
    public required double[,] CameraMatrix { get; init; }
    public required double[] DistCoeffs { get; init; }
}