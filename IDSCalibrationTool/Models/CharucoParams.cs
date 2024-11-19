
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using CommunityToolkit.Mvvm.ComponentModel;

using static Emgu.CV.Aruco.Dictionary;

namespace IDSCalibrationTool.Models;
public partial class CharucoParams : ObservableValidator
{
    private int _squaresX;

    [Range(3, 100)]
    public int SquaresX
    {
        get => _squaresX;
        set
        {
            SetProperty(ref _squaresX, value, validate: true);
            OnPropertyChanged(nameof(MarkerCount));
        }
    }

    private int _squaresY;

    [Range(3, 100)]
    public int SquaresY
    {
        get => _squaresY;
        set
        {
            SetProperty(ref _squaresY, value, validate: true);
            OnPropertyChanged(nameof(MarkerCount));
        }
    }

    private float _squareLength;

    [Range(0.001f, 1.0f)]
    public float SquareLength
    {
        get => _squareLength;
        set => SetProperty(ref _squareLength, value, validate: true);
    }

    private float _markerLength;

    [Range(0.001f, 1.0f)]
    public float MarkerLength
    {
        get => _markerLength;
        set => SetProperty(ref _markerLength, value, validate: true);
    }

    [JsonIgnore]
    public int MarkerCount
    {
        get
        {
            int result = 0;
            for (int i = 0; i < SquaresX; i++)
            {
                for (int j = 0; j < SquaresY; j++)
                {
                    result += (i + j + 1) % 2;
                }
            }
            return result;
        }
    }

    [ObservableProperty]
    private PredefinedDictionaryName _dictionaryName;

    public static CharucoParams Load(string jsonFilePath)
    {
        string jsonText = File.ReadAllText(jsonFilePath);

        var options = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<CharucoParams>(jsonText, options) ?? throw new JsonException("Failed to deserialize CharucoParams");
    }

    public void Save(string jsonFilePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        string jsonText = JsonSerializer.Serialize(this, options);
        File.WriteAllText(jsonFilePath, jsonText);
    }
}
