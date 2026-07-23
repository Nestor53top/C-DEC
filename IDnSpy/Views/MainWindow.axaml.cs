using Avalonia.Controls;
using Avalonia.Interactivity;
using IDnSpy.ViewModels;

namespace IDnSpy.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var vm = new MainWindowViewModel();
        vm.SetWindow(this);
        DataContext = vm;
    }

    private void TypeList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && sender is ListBox listBox)
        {
            var selected = listBox.SelectedItem as Services.TypeEntry;
            vm.SelectType(selected);
        }
    }
}
