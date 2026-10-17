using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CompanyTrack.Views;

public partial class ShellView : UserControl
{
    public ShellView() => AvaloniaXamlLoader.Load(this);
}
