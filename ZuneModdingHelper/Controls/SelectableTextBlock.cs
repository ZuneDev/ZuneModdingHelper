using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ZuneModdingHelper.Controls;

//  https://stackoverflow.com/a/32870521
public class SelectableTextBlock : TextBlock
{
    private TextPointer _startSelectPosition;
    private TextPointer _endSelectPosition;

    public string SelectedText { get; protected set; } = "";

    public delegate void TextSelectedHandler(string SelectedText);
    public event TextSelectedHandler TextSelected;

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        Point mouseDownPoint = e.GetPosition(this);
        _startSelectPosition = GetPositionFromPoint(mouseDownPoint, true);

        // TODO: Is there any better way to allow text to be selected?
        Clipboard.SetText(Text);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        Point mouseUpPoint = e.GetPosition(this);
        _endSelectPosition = GetPositionFromPoint(mouseUpPoint, true);

        TextRange otr = new(ContentStart, ContentEnd);
        otr.ApplyPropertyValue(TextElement.ForegroundProperty, Foreground);
        otr.ApplyPropertyValue(TextElement.BackgroundProperty, Background);

        TextRange ntr = new(_startSelectPosition, _endSelectPosition);
        ntr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));
        ntr.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.HotPink));

        SelectedText = ntr.Text;
        TextSelected?.Invoke(SelectedText);
    }
}
