using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.WindowManagement;

namespace AutoDarkModeApp;

public sealed partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hWnd);

    public MainWindow(INavigationService navigationService)
    {
        _navigationService = navigationService;
        InitializeComponent();

        Title = Debugger.IsAttached ? "Auto Dark Mode Debug" : "Auto Dark Mode";

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBar);
        TitleBar.Subtitle = Debugger.IsAttached ? "Debug" : "";

        // Listen for theme changes on the title bar and clean up when window closes
        TypedEventHandler<Microsoft.UI.Xaml.FrameworkElement, object> titleBarThemeChanged = (s, e) => ApplySystemThemeToCaptionButtons();
        TitleBar.ActualThemeChanged += titleBarThemeChanged;
        this.Closed += (s, e) => TitleBar.ActualThemeChanged -= titleBarThemeChanged;

        // Initial apply
        ApplySystemThemeToCaptionButtons();

        // Set app icons only if present to avoid throwing on missing files
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AutoDarkModeIcon.ico");
        if (File.Exists(iconPath))
        {
            AppWindow.SetIcon(iconPath);
            AppWindow.SetTaskbarIcon(iconPath);
            AppWindow.SetTitleBarIcon(iconPath);
        }

        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        uint dpi = GetDpiForWindow(hwnd);
        double scaleFactor = dpi / 96.0;

        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(860 * scaleFactor);
            presenter.PreferredMinimumHeight = (int)(600 * scaleFactor);
        }

        _navigationService.Frame = NavigationFrame;
        _navigationService.InitializeNavigationView(NavigationViewControl);
        _navigationService.InitializeBreadcrumbBar(BreadcrumBarControl);
    }

    private void NavViewTitleBar_BackRequested(Microsoft.UI.Xaml.FrameworkElement sender, object args)
    {
        if (NavigationFrame.CanGoBack)
        {
            NavigationFrame.GoBack();
        }
    }

    private void NavViewTitleBar_PaneToggleRequested(Microsoft.UI.Xaml.FrameworkElement sender, object args)
    {
        NavigationViewControl.IsPaneOpen = !NavigationViewControl.IsPaneOpen;
    }

    private void ApplySystemThemeToCaptionButtons()
    {
        // Align title bar and caption button colors with WinUI / Windows 11 semantics
        // Follow issue https://github.com/microsoft/microsoft-ui-xaml/issues/9722
        if (TitleBar.ActualTheme == ElementTheme.Dark)
        {
            // Title bar
            AppWindow.TitleBar.ForegroundColor = Colors.White;
            AppWindow.TitleBar.BackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.InactiveForegroundColor = Color.FromArgb(0xA0, 0xFF, 0xFF, 0xFF); // semi-transparent white
            AppWindow.TitleBar.InactiveBackgroundColor = Colors.Transparent;

            // Caption buttons
            AppWindow.TitleBar.ButtonForegroundColor = Colors.White;
            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.ButtonHoverForegroundColor = Colors.White;
            AppWindow.TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x14, 0xFF, 0xFF, 0xFF); // ~8% white
            AppWindow.TitleBar.ButtonPressedForegroundColor = Colors.White;
            AppWindow.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(0x28, 0xFF, 0xFF, 0xFF); // ~16% white
            AppWindow.TitleBar.ButtonInactiveForegroundColor = Color.FromArgb(0xA0, 0xFF, 0xFF, 0xFF);
            AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
        else
        {
            // Light theme
            // Title bar
            AppWindow.TitleBar.ForegroundColor = Colors.Black;
            AppWindow.TitleBar.BackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.InactiveForegroundColor = Color.FromArgb(0xA0, 0x00, 0x00, 0x00); // semi-transparent black
            AppWindow.TitleBar.InactiveBackgroundColor = Colors.Transparent;

            // Caption buttons
            AppWindow.TitleBar.ButtonForegroundColor = Colors.Black;
            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.ButtonHoverForegroundColor = Colors.Black;
            AppWindow.TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x14, 0x00, 0x00, 0x00); // ~8% black
            AppWindow.TitleBar.ButtonPressedForegroundColor = Colors.Black;
            AppWindow.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(0x28, 0x00, 0x00, 0x00); // ~16% black
            AppWindow.TitleBar.ButtonInactiveForegroundColor = Color.FromArgb(0xA0, 0x00, 0x00, 0x00);
            AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
    }

}
