using System;
using JinoOrder.Application.Common;
using JinoOrder.Presentation.Common;
using Microsoft.Extensions.DependencyInjection;

namespace JinoOrder.Infrastructure.Services;

/// <summary>
/// IServiceProvider를 캡슐화하여 ViewModel을 생성하는 팩토리
/// </summary>
public class ViewModelFactory : IViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TViewModel Create<TViewModel>() where TViewModel : ViewModelBase
    {
        return _serviceProvider.GetRequiredService<TViewModel>();
    }
}
