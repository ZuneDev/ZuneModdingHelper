using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZuneModdingHelper.Messages;

namespace ZuneModdingHelper.Services;

public class UpdateHelper
{
    public const string UPDATES_DIALOG_TITLE = "UPDATES";

    private readonly IUpdateService? _updateService = Ioc.Default.GetService<IUpdateService>();
    private bool _isRunning = false;
    private UpdateAvailableInfo _updateInfo;
    private string _downloadedUpdatePath;

    public static UpdateHelper Instance { get; } = new();

    public bool Enabled => _updateService is not null;

    public async Task CheckForUpdatesAsync(bool isUserTriggered)
    {
        if (_isRunning)
            return;

        _isRunning = true;

        if (isUserTriggered)
        {
            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new ProgressDialogViewModel
            {
                Title = UPDATES_DIALOG_TITLE,
                Description = "Checking for updates, please wait...",
                IsIndeterminate = true,
                ShowAffirmativeButton = false,
            }));
        }

        try
        {
            await Task.Delay(5000);
            _updateInfo = await _updateService.FetchAvailableUpdate();

            if (isUserTriggered)
                WeakReferenceMessenger.Default.Send<CloseDialogMessage>();

            if (_updateInfo is null)
            {
                if (isUserTriggered)
                {
                    WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new DialogViewModel
                    {
                        Title = UPDATES_DIALOG_TITLE,
                        Description = "No updates available.\r\n\r\nYou're already using the latest version.",
                        ShowAffirmativeButton = true,
                    }));
                }

                _isRunning = false;
                return;
            }

            // Newer version available, prompt user to download

            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new DialogViewModel
            {
                Title = UPDATES_DIALOG_TITLE,
                Description = $"Release {_updateInfo.Name} is available. Would you like to download it now?",
                AffirmativeText = "YES",
                NegativeText = "NO",
                ShowAffirmativeButton = true,
                ShowNegativeButton = true,
                OnAction = new AsyncRelayCommand<bool>(OnUpdateDialogResult)
            }));
        }
        catch
        {
            if (isUserTriggered)
            {
                App.OpenInBrowser($"{App.RepoUri}/releases");
                WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
            }

            _isRunning = false;
        }
    }

    private async Task OnUpdateDialogResult(bool accepted)
    {
        WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
        if (!accepted)
            return;

        ProgressDialogViewModel prog = new()
        {
            Title = UPDATES_DIALOG_TITLE,
            Description = "Downloading update...\r\nThis may take a few minutes.",
            IsIndeterminate = true,
            ShowAffirmativeButton = false,
            ShowNegativeButton = false,
            Maximum = 1.0,
        };
        WeakReferenceMessenger.Default.Send(new ShowDialogMessage(prog));

        // Ask user to save file
        Microsoft.Win32.SaveFileDialog saveFileDialog = new()
        {
            FileName = _updateInfo.Name,
            InitialDirectory = new KnownFolder(KnownFolderType.Downloads).Path
        };
        bool dialogResult = saveFileDialog.ShowDialog() ?? false;
        WeakReferenceMessenger.Default.Send<CloseDialogMessage>();

        if (dialogResult)
        {
            _downloadedUpdatePath = saveFileDialog.FileName;

            // Download new version to requested folder with progress updates
            Progress<double> progress = new(p => prog.Progress = p);
            prog.Progress = 0;
            prog.IsIndeterminate = false;
            await _updateService.DownloadUpdate(_updateInfo, _downloadedUpdatePath, progress);

            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new DialogViewModel
            {
                Title = UPDATES_DIALOG_TITLE,
                Description = "Update downloaded. Would you like to launch the new version?",
                AffirmativeText = "YES",
                NegativeText = "NO",
                ShowAffirmativeButton = true,
                ShowNegativeButton = true,
                OnAction = new AsyncRelayCommand<bool>(OnLaunchUpdateDialogResult)
            }));
        }

        _isRunning = false;
    }

    private async Task OnLaunchUpdateDialogResult(bool accepted)
    {
        WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
        if (!accepted)
            return;

        // In the future, this should launch an installer
        System.Diagnostics.Process.Start("explorer", _downloadedUpdatePath);
    }
}
