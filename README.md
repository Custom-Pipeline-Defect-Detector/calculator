# Astro Calculator (C# / WPF)

A Windows-focused scientific calculator with spaceflight utilities. It mixes a traditional keypad with mission-planning helpers such as the Tsiolkovsky rocket equation, orbital/escape velocity, Hohmann transfer, and surface gravity calculations.

## Project layout
- `SpaceCalculator.App/` – WPF application with the UI (XAML), view model, and calculation engines.
  - `Models/CalculatorEngine.cs` parses and evaluates scientific expressions with functions, constants, powers, and factorial support.
  - `Models/SpaceCalculations.cs` implements mission-specific helpers (delta-v, orbital velocity, Hohmann transfers, and surface gravity).
  - `ViewModels/MainViewModel.cs` wires the UI to the engines and exposes bindings for the Astro utilities.

## Building
The project targets `net8.0-windows` with WPF (`UseWPF` enabled). To build on Windows with the .NET 8 SDK installed:

```bash
dotnet build SpaceCalculator.App/SpaceCalculator.App.csproj
```

Running on non-Windows hosts will require a Windows build agent because WPF tooling is Windows-only.
