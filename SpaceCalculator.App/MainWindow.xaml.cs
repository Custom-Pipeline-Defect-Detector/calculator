using System.Windows;
using System.Windows.Controls;
using SpaceCalculator.ViewModels;

namespace SpaceCalculator;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void OnNumberClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            _viewModel.AppendSymbol(button.Tag?.ToString() ?? button.Content.ToString() ?? string.Empty);
        }
    }

    private void OnOperatorClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            _viewModel.AppendOperator(button.Tag?.ToString() ?? button.Content.ToString() ?? string.Empty);
        }
    }

    private void OnFunctionInsert(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var tag = button.Tag?.ToString() ?? string.Empty;
            _viewModel.InsertFunction(tag);
        }
    }

    private void OnEquals(object sender, RoutedEventArgs e) => _viewModel.Evaluate();

    private void OnBackspace(object sender, RoutedEventArgs e) => _viewModel.Backspace();

    private void OnClearEntry(object sender, RoutedEventArgs e) => _viewModel.Clear();

    private void OnComputeDeltaV(object sender, RoutedEventArgs e) => _viewModel.ComputeRocketEquation();

    private void OnComputeOrbitalVelocity(object sender, RoutedEventArgs e) => _viewModel.ComputeOrbitalVelocity();

    private void OnComputeEscapeVelocity(object sender, RoutedEventArgs e) => _viewModel.ComputeEscapeVelocity();

    private void OnComputeHohmann(object sender, RoutedEventArgs e) => _viewModel.ComputeHohmannTransfer();

    private void OnComputeSurfaceGravity(object sender, RoutedEventArgs e) => _viewModel.ComputeSurfaceGravity();

    private void OnUseAnswer(object sender, RoutedEventArgs e) => _viewModel.UseLastAnswer();
}
