using System.ComponentModel;
using System.Globalization;
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

    public string RocketInitialMass { get; set; } = "550000"; // kg default Saturn V staging sample
    public string RocketFinalMass { get; set; } = "130000";
    public string RocketExhaustVelocity { get; set; } = "3050"; // m/s typical RP-1/LOX

    public string MuValue { get; set; } = "398600.4418"; // km^3/s^2 Earth
    public string Radius1 { get; set; } = "6678"; // km (LEO altitude ~300 km above Earth)
    public string Radius2 { get; set; } = "42164"; // km (GEO)

    public event PropertyChangedEventHandler? PropertyChanged;

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
            var result = CalculatorEngine.Evaluate(Display);
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

    private bool TryParse(string? text, out double value)
    {
        var normalized = text?.Trim() ?? string.Empty;
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
               double.TryParse(normalized, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
    }

    protected void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
