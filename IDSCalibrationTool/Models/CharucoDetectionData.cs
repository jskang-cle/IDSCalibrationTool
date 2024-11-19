using System.Drawing;

namespace IDSCalibrationTool.Models;

public class CharucoDetectionData
{
    public bool Success { get; set; }

    public Size ImageSize { get; set; }

    public int ArucoMarkersFound { get; set; }
    public int ArucoMarkersExpected { get; set; }

    public PointF[][]? ArucoMarkerCorners { get; set; }
    public int[]? ArucoMarkerIds { get; set; }

    public PointF[]? CharucoCorners { get; set; }
    public int[]? CharucoIds { get; set; }
}
