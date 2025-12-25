using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JinoOrder.Application.Customers;
using JinoOrder.Domain.Customers;
using JinoOrder.Presentation.Common;
using Microsoft.Extensions.Logging;

namespace JinoOrder.Presentation.Customers;

public partial class CustomersViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;

    [ObservableProperty] private ObservableCollection<Customer> _customers = new();
    [ObservableProperty] private Customer? _selectedCustomer;

    public CustomersViewModel(ICustomerService customerService, ILogger<CustomersViewModel> logger)
    {
        _customerService = customerService;
        Logger = logger;

        Logger.LogDebug("CustomersViewModel 초기화됨");
    }

    public override void OnActivated()
    {
        base.OnActivated();
        Logger.LogDebug("CustomersViewModel 활성화됨");
        ExecuteAndForget(async _ => await LoadCustomersAsync(), "고객 목록 로드");
    }

    public async Task LoadCustomersAsync()
    {
        Logger.LogDebug("고객 목록 로드 시작");

        await ExecuteAsync(async _ =>
        {
            var customers = await _customerService.GetCustomersAsync();
            Customers = new ObservableCollection<Customer>(customers);
            Logger.LogDebug("고객 {Count}명 로드됨", customers.Count);
        }, "고객 목록 로드");
    }
}
