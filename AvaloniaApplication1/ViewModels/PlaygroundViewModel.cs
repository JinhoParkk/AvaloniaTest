using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaApplication1.Services;

namespace AvaloniaApplication1.ViewModels;

public partial class PlaygroundViewModel : ViewModelBase
{
    private readonly IDialogService? _dialogService;
    private readonly IToastService? _toastService;

    [ObservableProperty]
    private string _selectedSection = "Buttons";

    [ObservableProperty]
    private string _appTitle = "UI Playground";

    // Sections for navigation
    public ObservableCollection<PlaygroundSection> Sections { get; } = new()
    {
        new PlaygroundSection("Buttons", "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8z"),
        new PlaygroundSection("Dialogs", "M20 2H4c-1.1 0-2 .9-2 2v18l4-4h14c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2z"),
        new PlaygroundSection("Toasts", "M20 4H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 14H4V6h16v12z"),
        new PlaygroundSection("Forms", "M14 2H6c-1.1 0-2 .9-2 2v16c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V8l-6-6zm4 18H6V4h7v5h5v11z"),
        new PlaygroundSection("Navigation", "M12 2l-5.5 9h11L12 2zm0 3.84L13.93 9h-3.87L12 5.84zM17.5 13c-2.49 0-4.5 2.01-4.5 4.5s2.01 4.5 4.5 4.5 4.5-2.01 4.5-4.5-2.01-4.5-4.5-4.5z"),
        new PlaygroundSection("Cards", "M20 4H4c-1.11 0-2 .89-2 2v12c0 1.11.89 2 2 2h16c1.11 0 2-.89 2-2V6c0-1.11-.89-2-2-2zm0 14H4V6h16v12z"),
        new PlaygroundSection("Animations", "M12 4V1L8 5l4 4V6c3.31 0 6 2.69 6 6 0 1.01-.25 1.97-.7 2.8l1.46 1.46C19.54 15.03 20 13.57 20 12c0-4.42-3.58-8-8-8zm0 14c-3.31 0-6-2.69-6-6 0-1.01.25-1.97.7-2.8L5.24 7.74C4.46 8.97 4 10.43 4 12c0 4.42 3.58 8 8 8v3l4-4-4-4v3z"),
        new PlaygroundSection("Progress", "M12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm0-18C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2z"),
        new PlaygroundSection("Lists", "M4 10.5c-.83 0-1.5.67-1.5 1.5s.67 1.5 1.5 1.5 1.5-.67 1.5-1.5-.67-1.5-1.5-1.5zm0-6c-.83 0-1.5.67-1.5 1.5S3.17 7.5 4 7.5 5.5 6.83 5.5 6 4.83 4.5 4 4.5zm0 12c-.83 0-1.5.68-1.5 1.5s.68 1.5 1.5 1.5 1.5-.68 1.5-1.5-.67-1.5-1.5-1.5zM7 19h14v-2H7v2zm0-6h14v-2H7v2zm0-8v2h14V5H7z"),
        new PlaygroundSection("Theme", "M12 3c-4.97 0-9 4.03-9 9s4.03 9 9 9c.83 0 1.5-.67 1.5-1.5 0-.39-.15-.74-.39-1.01-.23-.26-.38-.61-.38-.99 0-.83.67-1.5 1.5-1.5H16c2.76 0 5-2.24 5-5 0-4.42-4.03-8-9-8z")
    };

    // Button section properties
    [ObservableProperty]
    private bool _isButtonLoading;

    [ObservableProperty]
    private bool _isButtonEnabled = true;

    // Forms section properties
    [ObservableProperty]
    private string _textInput = "";

    [ObservableProperty]
    private string _passwordInput = "";

    [ObservableProperty]
    private bool _checkboxValue;

    [ObservableProperty]
    private bool _switchValue;

    [ObservableProperty]
    private double _sliderValue = 50;

    [ObservableProperty]
    private int _selectedComboIndex;

    [ObservableProperty]
    private DateTime? _selectedDate = DateTime.Today;

    // Progress section properties
    [ObservableProperty]
    private double _progressValue = 65;

    [ObservableProperty]
    private bool _isIndeterminate;

    // Animation section properties
    [ObservableProperty]
    private bool _isAnimating;

    [ObservableProperty]
    private double _animationScale = 1.0;

    [ObservableProperty]
    private double _animationRotation = 0;

    [ObservableProperty]
    private double _animationOpacity = 1.0;

    // Theme section properties
    [ObservableProperty]
    private bool _isDarkMode;

    [ObservableProperty]
    private string _accentColor = "#0078D4";

    public PlaygroundViewModel(IDialogService? dialogService = null, IToastService? toastService = null)
    {
        _dialogService = dialogService;
        _toastService = toastService;
    }

    [RelayCommand]
    private void SelectSection(string sectionName)
    {
        SelectedSection = sectionName;
    }

    // Button Commands
    [RelayCommand]
    private void PrimaryButtonClick()
    {
        _toastService?.ShowAsync("Button Clicked", "You clicked the Primary button!", ToastType.Information);
    }

    [RelayCommand]
    private void SecondaryButtonClick()
    {
        _toastService?.ShowAsync("Button Clicked", "You clicked the Secondary button!", ToastType.Success);
    }

    [RelayCommand]
    private async Task LoadingButtonClickAsync()
    {
        IsButtonLoading = true;
        await Task.Delay(2000);
        IsButtonLoading = false;
        _toastService?.ShowAsync("Complete", "Loading finished!", ToastType.Success);
    }

    [RelayCommand]
    private void DangerButtonClick()
    {
        _toastService?.ShowAsync("Danger!", "This is a danger action!", ToastType.Warning);
    }

    // Dialog Commands
    [RelayCommand]
    private async Task ShowConfirmDialogAsync()
    {
        if (_dialogService == null) return;
        var result = await _dialogService.ShowConfirmationAsync(
            "Confirm Action",
            "Are you sure you want to proceed with this action?");
        await _toastService?.ShowAsync("Result", result ? "Confirmed!" : "Cancelled", ToastType.Information)!;
    }

    [RelayCommand]
    private async Task ShowInfoDialogAsync()
    {
        if (_dialogService == null) return;
        await _dialogService.ShowInformationAsync(
            "Information",
            "This is an informational dialog with some helpful content.");
    }

    [RelayCommand]
    private async Task ShowErrorDialogAsync()
    {
        if (_dialogService == null) return;
        await _dialogService.ShowErrorAsync(
            "Error Occurred",
            "Something went wrong! Please try again later.");
    }

    [RelayCommand]
    private async Task ShowInputDialogAsync()
    {
        if (_dialogService == null) return;
        var result = await _dialogService.ShowInputAsync(
            "Enter Your Name",
            "Please enter your name:",
            "John Doe");
        if (!string.IsNullOrEmpty(result))
        {
            await _toastService?.ShowAsync("Hello!", $"Nice to meet you, {result}!", ToastType.Success)!;
        }
    }

    // Toast Commands
    [RelayCommand]
    private async Task ShowInfoToastAsync()
    {
        if (_toastService == null) return;
        await _toastService.ShowAsync("Information", "This is an info notification", ToastType.Information);
    }

    [RelayCommand]
    private async Task ShowSuccessToastAsync()
    {
        if (_toastService == null) return;
        await _toastService.ShowAsync("Success!", "Operation completed successfully", ToastType.Success);
    }

    [RelayCommand]
    private async Task ShowWarningToastAsync()
    {
        if (_toastService == null) return;
        await _toastService.ShowAsync("Warning", "Please be careful with this action", ToastType.Warning);
    }

    [RelayCommand]
    private async Task ShowErrorToastAsync()
    {
        if (_toastService == null) return;
        await _toastService.ShowAsync("Error", "Something went wrong!", ToastType.Error);
    }

    // Animation Commands
    [RelayCommand]
    private async Task StartAnimationAsync()
    {
        IsAnimating = true;

        // Scale animation
        for (int i = 0; i <= 20; i++)
        {
            AnimationScale = 1.0 + Math.Sin(i * 0.314) * 0.3;
            await Task.Delay(50);
        }
        AnimationScale = 1.0;

        // Rotation animation
        for (int i = 0; i <= 36; i++)
        {
            AnimationRotation = i * 10;
            await Task.Delay(20);
        }
        AnimationRotation = 0;

        // Opacity animation
        for (int i = 0; i <= 20; i++)
        {
            AnimationOpacity = 0.3 + Math.Abs(Math.Sin(i * 0.314)) * 0.7;
            await Task.Delay(50);
        }
        AnimationOpacity = 1.0;

        IsAnimating = false;
    }

    [RelayCommand]
    private void ResetAnimation()
    {
        AnimationScale = 1.0;
        AnimationRotation = 0;
        AnimationOpacity = 1.0;
        IsAnimating = false;
    }

    // Progress Commands
    [RelayCommand]
    private async Task StartProgressAsync()
    {
        ProgressValue = 0;
        for (int i = 0; i <= 100; i++)
        {
            ProgressValue = i;
            await Task.Delay(30);
        }
        await _toastService?.ShowAsync("Complete", "Progress finished!", ToastType.Success)!;
    }

    [RelayCommand]
    private void ToggleIndeterminate()
    {
        IsIndeterminate = !IsIndeterminate;
    }

    // Theme Commands
    [RelayCommand]
    private void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        _toastService?.ShowAsync("Theme Changed", IsDarkMode ? "Dark mode enabled" : "Light mode enabled", ToastType.Information);
    }

    [RelayCommand]
    private void SetAccentColor(string color)
    {
        AccentColor = color;
        _toastService?.ShowAsync("Accent Changed", $"Accent color set to {color}", ToastType.Information);
    }
}

public record PlaygroundSection(string Name, string IconPath);
