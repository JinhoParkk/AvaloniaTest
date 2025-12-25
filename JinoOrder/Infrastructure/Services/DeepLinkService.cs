using System;
using JinoOrder.Application.Common;
using JinoOrder.Presentation.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace JinoOrder.Infrastructure.Services;

/// <summary>
/// 딥링크 처리 서비스 구현
/// </summary>
public class DeepLinkService : IDeepLinkService
{
    public const string UriScheme = "jinoorder";

    private readonly NavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;

    public string Scheme => UriScheme;

    public DeepLinkService(NavigationService navigationService, IServiceProvider serviceProvider)
    {
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
    }

    public DeepLinkResult Parse(string uriString)
    {
        if (string.IsNullOrWhiteSpace(uriString))
            return new DeepLinkResult(false, ErrorMessage: "URI가 비어있습니다.");

        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            return new DeepLinkResult(false, ErrorMessage: "잘못된 URI 형식입니다.");

        return Parse(uri);
    }

    public DeepLinkResult Parse(Uri uri)
    {
        if (uri == null)
            return new DeepLinkResult(false, ErrorMessage: "URI가 null입니다.");

        if (!string.Equals(uri.Scheme, UriScheme, StringComparison.OrdinalIgnoreCase))
            return new DeepLinkResult(false, ErrorMessage: $"지원하지 않는 스키마입니다: {uri.Scheme}");

        // URI 구조: jinoorder://route/entityId
        // Host가 route가 됨 (jinoorder://orders -> Host = "orders")
        var route = uri.Host?.ToLowerInvariant();
        if (string.IsNullOrEmpty(route))
            return new DeepLinkResult(false, ErrorMessage: "경로가 지정되지 않았습니다.");

        // 유효한 라우트 검증
        if (!IsValidRoute(route))
            return new DeepLinkResult(false, ErrorMessage: $"지원하지 않는 경로입니다: {route}");

        // EntityId 추출 (경로의 첫 번째 세그먼트)
        string? entityId = null;
        var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (pathSegments.Length > 0)
        {
            entityId = pathSegments[0];
        }

        return new DeepLinkResult(true, route, entityId);
    }

    public bool CanHandle(string uriString)
    {
        if (string.IsNullOrWhiteSpace(uriString))
            return false;

        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            return false;

        return CanHandle(uri);
    }

    public bool CanHandle(Uri uri)
    {
        if (uri == null)
            return false;

        return string.Equals(uri.Scheme, UriScheme, StringComparison.OrdinalIgnoreCase);
    }

    public bool Handle(string uriString)
    {
        var result = Parse(uriString);
        if (!result.Success)
            return false;

        return HandleInternal(result);
    }

    public bool Handle(Uri uri)
    {
        var result = Parse(uri);
        if (!result.Success)
            return false;

        return HandleInternal(result);
    }

    private bool HandleInternal(DeepLinkResult result)
    {
        if (!result.Success || string.IsNullOrEmpty(result.Route))
            return false;

        try
        {
            // JinoOrderMainViewModel으로 네비게이션하고 해당 메뉴 선택
            var mainViewModel = _serviceProvider.GetRequiredService<JinoOrderMainViewModel>();

            // 라우트에 따라 메뉴 선택
            var menuName = MapRouteToMenu(result.Route);
            mainViewModel.SelectMenuCommand.Execute(menuName);

            // EntityId가 있으면 해당 엔티티 선택 (향후 확장)
            if (!string.IsNullOrEmpty(result.EntityId))
            {
                SelectEntity(mainViewModel, result.Route, result.EntityId);
            }

            // 메인 뷰로 네비게이션
            _navigationService.NavigateTo(mainViewModel);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsValidRoute(string route)
    {
        return route switch
        {
            "orders" or "history" or "menu" or "customers" or "stats" or "settings" => true,
            _ => false
        };
    }

    private static string MapRouteToMenu(string route)
    {
        // 라우트와 메뉴 이름이 동일함
        return route;
    }

    private static void SelectEntity(JinoOrderMainViewModel viewModel, string route, string entityId)
    {
        // 라우트별 엔티티 선택 로직
        // 현재는 기본 구현만 제공, 향후 확장 가능
        switch (route)
        {
            case "orders":
            case "history":
                // entityId로 주문 찾아서 선택
                if (int.TryParse(entityId, out var orderId))
                {
                    var order = viewModel.ActiveOrders.FirstOrDefault(o => o.Id == orderId)
                        ?? viewModel.PendingOrders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        viewModel.SelectedOrder = order;
                    }
                }
                break;

            case "menu":
                // entityId로 메뉴 아이템 찾아서 선택
                if (int.TryParse(entityId, out var menuItemId))
                {
                    var menuItem = viewModel.MenuItems.FirstOrDefault(m => m.Id == menuItemId);
                    if (menuItem != null)
                    {
                        viewModel.SelectedMenuItem = menuItem;
                    }
                }
                break;

            case "customers":
                // entityId로 고객 찾아서 선택
                if (int.TryParse(entityId, out var customerId))
                {
                    var customer = viewModel.Customers.FirstOrDefault(c => c.Id == customerId);
                    if (customer != null)
                    {
                        viewModel.SelectedCustomer = customer;
                    }
                }
                break;

            // stats, settings는 엔티티 선택 없음
        }
    }
}
