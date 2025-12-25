using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JinoOrder.Application.Customers;
using JinoOrder.Domain.Customers;
using JinoOrder.Presentation.Common;

namespace JinoOrder.Presentation.Customers;

public partial class CustomersViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;

    [ObservableProperty] private ObservableCollection<Customer> _customers = new();
    [ObservableProperty] private Customer? _selectedCustomer;
    [ObservableProperty] private bool _isLoading;

    public CustomersViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public override void OnActivated()
    {
        base.OnActivated();
        _ = LoadCustomersAsync();
    }

    public async Task LoadCustomersAsync()
    {
        IsLoading = true;
        try
        {
            var customers = await _customerService.GetCustomersAsync();
            Customers = new ObservableCollection<Customer>(customers);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
