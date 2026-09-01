namespace AutoDarkModeApp.ViewModels;

public partial class AutoSwitchViewModel : ObservableRecipient
{
    public enum SwitchTriggerMode
    {
        CustomTimes,
        LocationTimes,
        CoordinateTimes,
        WindowsNightLight,
        AmbientLight,
    }

    [ObservableProperty]
    public partial bool AutoThemeSwitchingEnabled { get; set; }

    [ObservableProperty]
    public partial SwitchTriggerMode SelectedTriggerMode { get; set; }

    [ObservableProperty]
    public partial Visibility OffsetTimeSettingsCardVisibility { get; set; }

    [ObservableProperty]
    public partial int OffsetTimesMinimum { get; set; }

    [ObservableProperty]
    public partial int OffsetLight { get; set; }

    [ObservableProperty]
    public partial int OffsetDark { get; set; }

    [RelayCommand]
    private void SetTriggerMode(string mode)
    {
        if (Enum.TryParse<SwitchTriggerMode>(mode, out var result))
        {
            SelectedTriggerMode = result;
        }
    }

    private void HandleAutoTheme(bool value)
    {
        AutoThemeSwitchingEnabled = value;

        if (_builder.Config.Governor == Governor.NightLight)
        {
            SelectedTriggerMode = SwitchTriggerMode.WindowsNightLight;
            LocationSettingsCardVisibility = Visibility.Collapsed;
            CustomTimeSettingsCardVisibility = Visibility.Collapsed;
            OffsetTimeSettingsCardVisibility = Visibility.Visible;
            PostponeOptionsSkipOnceVisibility = Visibility.Visible;
            OffsetTimesMinimum = 0;
            return;
        }

        if (_builder.Config.Governor == Governor.AmbientLight)
        {
            SelectedTriggerMode = SwitchTriggerMode.AmbientLight;
            LocationSettingsCardVisibility = Visibility.Collapsed;
            CustomTimeSettingsCardVisibility = Visibility.Collapsed;
            OffsetTimeSettingsCardVisibility = Visibility.Collapsed;
            PostponeOptionsSkipOnceVisibility = Visibility.Collapsed;
            return;
        }

        if (!_builder.Config.Location.Enabled)
        {
            SelectedTriggerMode = SwitchTriggerMode.CustomTimes;
            LocationSettingsCardVisibility = Visibility.Collapsed;
            CustomTimeSettingsCardVisibility = Visibility.Visible;
            return;
        }

        if (_builder.Config.Location.UseGeolocatorService)
        {
            SelectedTriggerMode = SwitchTriggerMode.LocationTimes;
        }
        else
        {
            SelectedTriggerMode = SwitchTriggerMode.CoordinateTimes;
        }

        LocationSettingsCardVisibility = Visibility.Visible;
        OffsetTimesMinimum = -720;
        CustomTimeSettingsCardVisibility = Visibility.Visible;
        OffsetTimeSettingsCardVisibility = Visibility.Visible;
        PostponeOptionsSkipOnceVisibility = Visibility.Visible;
    }

    partial void OnAutoThemeSwitchingEnabledChanged(bool value)
    {
        if (_isInitializing)
            return;

        HandleAutoTheme(value);

        _builder.Config.AutoThemeSwitchingEnabled = value;
        try
        {
            _builder.Save();
        }
        catch (Exception ex)
        {
            _errorService.ShowErrorMessage(ex, App.MainWindow.Content.XamlRoot, "AutoSwitchViewModel");
        }
    }

    partial void OnSelectedTriggerModeChanged(SwitchTriggerMode value)
    {
        if (_isInitializing)
            return;

        // Each case fully controls all visibility states to prevent flickering
        switch (value)
        {
            case SwitchTriggerMode.CustomTimes:
                _builder.Config.Governor = Governor.Default;
                _builder.Config.Location.Enabled = false;
                _builder.Config.Location.UseGeolocatorService = false;
                CustomTimeSettingsCardVisibility = Visibility.Visible;
                OffsetTimeSettingsCardVisibility = Visibility.Collapsed;
                LocationSettingsCardVisibility = Visibility.Collapsed;
                break;

            case SwitchTriggerMode.LocationTimes:
                _builder.Config.Governor = Governor.Default;
                _builder.Config.Location.Enabled = true;
                _builder.Config.Location.UseGeolocatorService = true;
                CustomTimeSettingsCardVisibility = Visibility.Visible;
                OffsetTimeSettingsCardVisibility = Visibility.Visible;
                OffsetTimesMinimum = -720;
                LocationSettingsCardVisibility = Visibility.Visible;
                break;

            case SwitchTriggerMode.CoordinateTimes:
                _builder.Config.Governor = Governor.Default;
                _builder.Config.Location.Enabled = true;
                _builder.Config.Location.UseGeolocatorService = false;
                CustomTimeSettingsCardVisibility = Visibility.Visible;
                OffsetTimeSettingsCardVisibility = Visibility.Visible;
                OffsetTimesMinimum = -720;
                LocationSettingsCardVisibility = Visibility.Visible;
                break;

            case SwitchTriggerMode.WindowsNightLight:
                _builder.Config.Governor = Governor.NightLight;
                _builder.Config.AutoThemeSwitchingEnabled = true;
                _builder.Config.Location.Enabled = false;
                _builder.Config.Location.UseGeolocatorService = false;
                CustomTimeSettingsCardVisibility = Visibility.Collapsed;
                OffsetTimeSettingsCardVisibility = Visibility.Visible;
                OffsetTimesMinimum = 0;
                LocationSettingsCardVisibility = Visibility.Collapsed;
                break;

            case SwitchTriggerMode.AmbientLight:
                // Run auto-configure only if we are switching to Ambient Light and values are still defaults
                // This prevents overwriting user's custom settings when switching modes
                if (_builder.Config.AmbientLight.DarkThreshold == 40 && _builder.Config.AmbientLight.LightThreshold == 80)
                {
                    AutoConfigure();
                }
                _builder.Config.Governor = Governor.AmbientLight;
                _builder.Config.AutoThemeSwitchingEnabled = true;
                _builder.Config.Location.Enabled = false;
                _builder.Config.Location.UseGeolocatorService = false;
                CustomTimeSettingsCardVisibility = Visibility.Collapsed;
                OffsetTimeSettingsCardVisibility = Visibility.Collapsed;
                LocationSettingsCardVisibility = Visibility.Collapsed;
                break;
        }

        try
        {
            _builder.Save();
        }
        catch (Exception ex)
        {
            _errorService.ShowErrorMessage(ex, App.MainWindow.Content.XamlRoot, "AutoSwitchViewModel");
        }

        SafeApplyTheme();
    }

    partial void OnOffsetLightChanged(int value)
    {
        if (_isInitializing)
            return;

        if (_debounceTimer != null)
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }
    }

    partial void OnOffsetDarkChanged(int value)
    {
        if (_isInitializing)
            return;

        if (_debounceTimer != null)
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }
    }
}
