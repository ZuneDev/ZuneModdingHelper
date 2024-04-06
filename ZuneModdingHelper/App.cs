using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using IrisShell;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using ZuneModdingHelper.Services;

namespace ZuneModdingHelper;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public class App : IrisAppBase
{
    public static readonly ReleaseVersion Version = new(2021, 12, 30, 0, Phase.Alpha);
    public static readonly string VersionStr = Version.ToString();

    public const string DonateLink = "http://josh.askharoun.com/donate";

    public override string Title => "Zune Modding Helper";

    public override string WindowSource => "clr-res://ZuneModdingHelper!Home.uix#Frame";

    public override bool EnableDebugging => false;

    public App()
    {
        ServiceProviderReady += OnServiceProviderReady;
    }

    [STAThread]
    public static int Main(string[] args)
    {
        ConfigureAppCenter();

        App app = new();
        return app.Run(args);
    }

    public static void OpenInBrowser(string url)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url)
        {
            UseShellExecute = true
        });
    }

    private static void ConfigureAppCenter()
    {
        // Set up App Center analytics
        AppCenter.SetCountryCode(System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName);
        AppCenter.Start("24903c19-b3d9-4ab5-b445-b981ca647125", typeof(Analytics), typeof(Crashes));

#if DEBUG
        // Disable crash and event analytics when in debug
        AppCenter.SetEnabledAsync(false);
#endif
    }

    protected override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        return base.ConfigureServices(services)
            .AddSingleton<IStringLocalizer>(new LocalizationService());
    }

    private void OnServiceProviderReady(object sender, IServiceProvider serviceProvider)
    {
        ServiceProviderReady -= OnServiceProviderReady;
        Ioc.Default.ConfigureServices(serviceProvider);
    }
}
