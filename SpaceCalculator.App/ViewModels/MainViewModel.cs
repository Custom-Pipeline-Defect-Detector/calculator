using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using SpaceCalculator.Models;

namespace SpaceCalculator.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private string _display = string.Empty;
    private string _status = "Ready.";
    private string _lastAnswer = "0";

    private string _deltaVResult = "–";
    private string _orbitalVelocityResult = "–";
    private string _escapeVelocityResult = "–";
    private string _transferDeltaVResult = "–";
    private string _surfaceGravityResult = "–";
    private string _transferTimeResult = "–";

    private string _rocketInitialMass = "550000"; // kg default Saturn V staging sample
    private string _rocketFinalMass = "130000";
    private string _rocketExhaustVelocity = "3050"; // m/s typical RP-1/LOX

    private string _muValue = "398600.4418"; // km^3/s^2 Earth
    private string _radius1 = "6678"; // km (LEO altitude ~300 km above Earth)
    private string _radius2 = "42164"; // km (GEO)

    private bool _useDegrees = true;
    private CelestialPreset? _selectedBody;

    public string Display
    {
        get => _display;
        set => SetField(ref _display, value);
    }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public string DeltaVResult
    {
        get => _deltaVResult;
        set => SetField(ref _deltaVResult, value);
    }

    public string OrbitalVelocityResult
    {
        get => _orbitalVelocityResult;
        set => SetField(ref _orbitalVelocityResult, value);
    }

    public string EscapeVelocityResult
    {
        get => _escapeVelocityResult;
        set => SetField(ref _escapeVelocityResult, value);
    }

    public string TransferDeltaVResult
    {
        get => _transferDeltaVResult;
        set => SetField(ref _transferDeltaVResult, value);
    }

    public string SurfaceGravityResult
    {
        get => _surfaceGravityResult;
        set => SetField(ref _surfaceGravityResult, value);
    }

    public string TransferTimeResult
    {
        get => _transferTimeResult;
        set => SetField(ref _transferTimeResult, value);
    }

    public string RocketInitialMass
    {
        get => _rocketInitialMass;
        set => SetField(ref _rocketInitialMass, value);
    }

    public string RocketFinalMass
    {
        get => _rocketFinalMass;
        set => SetField(ref _rocketFinalMass, value);
    }

    public string RocketExhaustVelocity
    {
        get => _rocketExhaustVelocity;
        set => SetField(ref _rocketExhaustVelocity, value);
    }

    public string MuValue
    {
        get => _muValue;
        set => SetField(ref _muValue, value);
    }

    public string Radius1
    {
        get => _radius1;
        set => SetField(ref _radius1, value);
    }

    public string Radius2
    {
        get => _radius2;
        set => SetField(ref _radius2, value);
    }

    public bool UseDegrees
    {
        get => _useDegrees;
        set
        {
            if (SetField(ref _useDegrees, value))
            {
                OnPropertyChanged(nameof(AngleModeLabel));
            }
        }
    }

    public string AngleModeLabel => UseDegrees ? "DEG" : "RAD";

    public IReadOnlyList<CelestialPreset> BodyPresets { get; } = new List<CelestialPreset>
    {
        new("Earth", 398600.4418, 6378.137),
        new("Moon", 4902.800066, 1737.4),
        new("Mars", 42828.375214, 3396.2),
        new("Jupiter", 126686511, 71492),
        new("Sun", 132712440018, 696340)
    };

    public CelestialPreset? SelectedBody
    {
        get => _selectedBody;
        set
        {
            if (SetField(ref _selectedBody, value) && value is not null)
            {
                ApplyPreset(value);
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel()
    {
        SelectedBody = BodyPresets.FirstOrDefault();
    }

    public void AppendSymbol(string symbol)
    {
        Display += symbol;
        Status = "Building expression";
    }

    public void AppendOperator(string op)
    {
        if (string.IsNullOrWhiteSpace(Display))
        {
            Display = _lastAnswer + op;
        }
        else
        {
            Display += op;
        }

        Status = "Added operator";
    }

    public void InsertFunction(string tag)
    {
        switch (tag)
        {
            case "pi":
            case "tau":
            case "e":
            case "g0":
            case "c":
            case "au":
                AppendSymbol(tag);
                break;
            case "fact":
                AppendSymbol("!");
                break;
            default:
                AppendSymbol(tag + "(");
                break;
        }
    }

    public void Evaluate()
    {
        if (string.IsNullOrWhiteSpace(Display))
        {
            Status = "Nothing to evaluate.";
            return;
        }

        try
        {
            var result = CalculatorEngine.Evaluate(Display, UseDegrees);
            _lastAnswer = result.ToString("G15", CultureInfo.InvariantCulture);
            Display = _lastAnswer;
            Status = "Expression solved.";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    public void Backspace()
    {
        if (string.IsNullOrEmpty(Display))
        {
            return;
        }

        Display = Display[..^1];
    }

    public void Clear()
    {
        Display = string.Empty;
        Status = "Cleared.";
    }

    public void ComputeRocketEquation()
    {
        if (!TryParse(RocketInitialMass, out var m0) || !TryParse(RocketFinalMass, out var mf) || !TryParse(RocketExhaustVelocity, out var ve))
        {
            Status = "Check rocket equation inputs.";
            return;
        }

        try
        {
            var deltaV = SpaceCalculations.ComputeDeltaV(m0, mf, ve);
            DeltaVResult = $"{deltaV:F2} m/s";
            Status = "Rocket equation ready.";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    public void ComputeOrbitalVelocity()
    {
        if (!TryParse(MuValue, out var mu) || !TryParse(Radius1, out var radiusKm))
        {
            Status = "Invalid μ or radius.";
            return;
        }

        var velocity = SpaceCalculations.OrbitalVelocity(mu, radiusKm);
        OrbitalVelocityResult = $"{velocity:F2} km/s";
        Status = "Circular orbital velocity computed.";
    }

    public void ComputeEscapeVelocity()
    {
        if (!TryParse(MuValue, out var mu) || !TryParse(Radius1, out var radiusKm))
        {
            Status = "Invalid μ or radius.";
            return;
        }

        var velocity = SpaceCalculations.EscapeVelocity(mu, radiusKm);
        EscapeVelocityResult = $"{velocity:F2} km/s";
        Status = "Escape velocity computed.";
    }

    public void ComputeHohmannTransfer()
    {
        if (!TryParse(MuValue, out var mu) || !TryParse(Radius1, out var r1) || !TryParse(Radius2, out var r2))
        {
            Status = "Invalid μ or radii.";
            return;
        }

        var (dv1, dv2, total) = SpaceCalculations.HohmannTransferDeltaV(mu, r1, r2);
        TransferDeltaVResult = $"Δv1: {dv1:F2} km/s | Δv2: {dv2:F2} km/s | Total: {total:F2} km/s";
        Status = "Hohmann transfer solved.";
    }

    public void ComputeHohmannTransferTime()
    {
        if (!TryParse(MuValue, out var mu) || !TryParse(Radius1, out var r1) || !TryParse(Radius2, out var r2))
        {
            Status = "Invalid μ or radii.";
            return;
        }

        var transferTime = SpaceCalculations.HohmannTransferTime(mu, r1, r2);
        TransferTimeResult = $"{transferTime / 60:F1} minutes";
        Status = "Transfer time computed.";
    }

    public void ComputeSurfaceGravity()
    {
        if (!TryParse(MuValue, out var mu) || !TryParse(Radius1, out var radiusKm))
        {
            Status = "Invalid μ or radius.";
            return;
        }

        var g = SpaceCalculations.SurfaceGravity(mu, radiusKm);
        SurfaceGravityResult = $"{g:F3} m/s²";
        Status = "Surface gravity computed.";
    }

    public void UseLastAnswer() => AppendSymbol(_lastAnswer);

    public void ToggleAngleMode()
    {
        UseDegrees = !UseDegrees;
        Status = $"Angle mode: {AngleModeLabel}";
    }

    private void ApplyPreset(CelestialPreset preset)
    {
        MuValue = preset.Mu.ToString("G", CultureInfo.InvariantCulture);
        Radius1 = preset.ReferenceRadiusKm.ToString("G", CultureInfo.InvariantCulture);
        Status = $"{preset.Name} constants loaded.";
    }

    private bool TryParse(string? text, out double value)
    {
        var normalized = text?.Trim() ?? string.Empty;
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
               double.TryParse(normalized, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged(string? propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public record CelestialPreset(string Name, double Mu, double ReferenceRadiusKm);
