using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace ZuneModdingHelper.Messages;

public record ShowDialogMessage(DialogViewModel ViewModel);
public record CloseDialogMessage();

public partial class DialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private bool _showAffirmativeButton = true;

    [ObservableProperty]
    private bool _showNegativeButton = false;

    [ObservableProperty]
    private string _affirmativeText = "OK";

    [ObservableProperty]
    private string _negativeText = "CANCEL";

    [ObservableProperty]
    private int? _height;

    [ObservableProperty]
    private IAsyncRelayCommand<bool> _onAction = DefaultCloseCommand;

    protected static IAsyncRelayCommand<bool> DefaultCloseCommand { get; } = new AsyncRelayCommand<bool>(_ =>
    {
        WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
        return System.Threading.Tasks.Task.CompletedTask;
    });
}

public partial class ProgressDialogViewModel : DialogViewModel
{
    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private double _maximum = 1.0;

    [ObservableProperty]
    private bool _isIndeterminate = true;
}
