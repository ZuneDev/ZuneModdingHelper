using CommunityToolkit.Mvvm.ComponentModel;

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
    private bool _showPrimaryButton;

    [ObservableProperty]
    private int? _height;
}

public partial class ProgressDialogViewModel : DialogViewModel
{
    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private bool _isIndeterminate;
}
