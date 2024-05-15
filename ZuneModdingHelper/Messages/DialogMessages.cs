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
}

public partial class ProgressDialogViewModel : DialogViewModel
{
    [ObservableProperty]
    private double _progress;
}
