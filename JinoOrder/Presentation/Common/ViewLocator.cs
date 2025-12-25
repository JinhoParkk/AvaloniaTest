using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using JinoOrder.Presentation.Auth;
using JinoOrder.Presentation.Customers;
using JinoOrder.Presentation.Main;
using JinoOrder.Presentation.Menu;
using JinoOrder.Presentation.Orders;
using JinoOrder.Presentation.Settings;
using JinoOrder.Presentation.Shell;
using JinoOrder.Presentation.Statistics;

namespace JinoOrder.Presentation.Common;

/// <summary>
/// 명시적 Dictionary 기반 ViewLocator (AOT/Trimming 안전)
/// </summary>
public class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Func<Control>> ViewRegistry = new()
    {
        { typeof(JinoOrderMainViewModel), () => new JinoOrderMainView() },
        { typeof(LoginViewModel), () => new LoginView() },
        { typeof(MainWindowViewModel), () => new MainWindow() },
        // 분리된 자식 ViewModel들은 JinoOrderMainView 내에서 직접 바인딩되므로
        // ViewLocator에서 따로 매핑하지 않아도 됩니다.
    };

    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        if (ViewRegistry.TryGetValue(param.GetType(), out var factory))
        {
            return factory();
        }

        return new TextBlock { Text = $"View not found for: {param.GetType().Name}" };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
