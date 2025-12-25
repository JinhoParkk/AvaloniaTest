using Avalonia.Controls;
using FluentAvalonia.UI.Navigation;

namespace JinoOrder.Presentation.Orders;

public partial class OrdersPage : UserControl
{
    public OrdersPage()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);
    }
}
