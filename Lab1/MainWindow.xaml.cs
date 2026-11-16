using System.Windows;
using Lab1.ViewModels;

namespace Lab1;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        if(DataContext is MainViewModel viewModel)
        {
            // Release the COM port when the window closes.
            viewModel.Shutdown();
        }
        base.OnClosed(e);
    }
}
