using System;
using System.Linq;
using JinoOrder.Application.Common;
using JinoOrder.Domain.Common;
using JinoOrder.Presentation.Shell;
using JinoOrder.Presentation.Orders;
using JinoOrder.Presentation.Menu;
using JinoOrder.Presentation.Customers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JinoOrder.Infrastructure.Services;

/// <summary>
/// 딥링크 처리 서비스 구현
/// </summary>
public class DeepLinkService : IDeepLinkService
{
    private readonly NavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeepLinkService> _logger;

    public string Scheme => AppConstants.DeepLinkScheme;

    public DeepLinkService(NavigationService navigationService, IServiceProvider serviceProvider, ILogger<DeepLinkService> logger)
    {
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
        _logger = logger;

        _logger.LogDebug("DeepLinkService 초기화됨");
    }

    public DeepLinkResult Parse(string uriString)
    {
        if (string.IsNullOrWhiteSpace(uriString))
        {
            _logger.LogWarning("딥링크 파싱 실패: URI가 비어있음");
            return new DeepLinkResult(false, ErrorMessage: "URI가 비어있습니다.");
        }

        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
        {
            _logger.LogWarning("딥링크 파싱 실패: 잘못된 URI 형식 - {Uri}", uriString);
            return new DeepLinkResult(false, ErrorMessage: "잘못된 URI 형식입니다.");
        }

        return Parse(uri);
    }

    public DeepLinkResult Parse(Uri uri)
    {
        if (uri == null)
        {
            _logger.LogWarning("딥링크 파싱 실패: URI가 null");
            return new DeepLinkResult(false, ErrorMessage: "URI가 null입니다.");
        }

        if (!string.Equals(uri.Scheme, AppConstants.DeepLinkScheme, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("딥링크 파싱 실패: 지원하지 않는 스키마 - {Scheme}", uri.Scheme);
            return new DeepLinkResult(false, ErrorMessage: $"지원하지 않는 스키마입니다: {uri.Scheme}");
        }

        // URI 구조: jinoorder://route/entityId
        // Host가 route가 됨 (jinoorder://orders -> Host = "orders")
        var route = uri.Host?.ToLowerInvariant();
        if (string.IsNullOrEmpty(route))
        {
            _logger.LogWarning("딥링크 파싱 실패: 경로가 지정되지 않음");
            return new DeepLinkResult(false, ErrorMessage: "경로가 지정되지 않았습니다.");
        }

        // 유효한 라우트 검증
        if (!Routes.IsValidRoute(route))
        {
            _logger.LogWarning("딥링크 파싱 실패: 지원하지 않는 경로 - {Route}", route);
            return new DeepLinkResult(false, ErrorMessage: $"지원하지 않는 경로입니다: {route}");
        }

        // EntityId 추출 (경로의 첫 번째 세그먼트)
        string? entityId = null;
        var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (pathSegments.Length > 0)
        {
            entityId = pathSegments[0];
        }

        _logger.LogDebug("딥링크 파싱 성공: Route={Route}, EntityId={EntityId}", route, entityId);
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

        return string.Equals(uri.Scheme, AppConstants.DeepLinkScheme, StringComparison.OrdinalIgnoreCase);
    }

    public bool Handle(string uriString)
    {
        _logger.LogInformation("딥링크 처리 시도: {Uri}", uriString);
        var result = Parse(uriString);
        if (!result.Success)
            return false;

        return HandleInternal(result);
    }

    public bool Handle(Uri uri)
    {
        _logger.LogInformation("딥링크 처리 시도: {Uri}", uri);
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
            mainViewModel.SelectMenuCommand.Execute(result.Route);
            _logger.LogDebug("딥링크 메뉴 선택: {Route}", result.Route);

            // EntityId가 있으면 해당 엔티티 선택 (향후 확장)
            if (!string.IsNullOrEmpty(result.EntityId))
            {
                SelectEntity(mainViewModel, result.Route, result.EntityId);
            }

            // 메인 뷰로 네비게이션
            _navigationService.NavigateTo(mainViewModel);

            _logger.LogInformation("딥링크 처리 완료: Route={Route}, EntityId={EntityId}", result.Route, result.EntityId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "딥링크 처리 중 오류 발생");
            return false;
        }
    }

    private static void SelectEntity(JinoOrderMainViewModel viewModel, string route, string entityId)
    {
        // 라우트별 엔티티 선택 로직
        // 현재는 기본 구현만 제공, 향후 확장 가능
        switch (route)
        {
            case Routes.Orders:
            case Routes.History:
                // entityId로 주문 찾아서 선택 (자식 ViewModel에서 접근)
                if (int.TryParse(entityId, out var orderId))
                {
                    var order = viewModel.Orders.ActiveOrders.FirstOrDefault(o => o.Id == orderId)
                        ?? viewModel.Orders.PendingOrders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        viewModel.Orders.SelectedOrder = order;
                    }
                }
                break;

            case Routes.Menu:
                // entityId로 메뉴 아이템 찾아서 선택
                if (int.TryParse(entityId, out var menuItemId))
                {
                    var menuItem = viewModel.Menu.MenuItems.FirstOrDefault(m => m.Id == menuItemId);
                    if (menuItem != null)
                    {
                        viewModel.Menu.SelectedMenuItem = menuItem;
                    }
                }
                break;

            case Routes.Customers:
                // entityId로 고객 찾아서 선택
                if (int.TryParse(entityId, out var customerId))
                {
                    var customer = viewModel.Customers.Customers.FirstOrDefault(c => c.Id == customerId);
                    if (customer != null)
                    {
                        viewModel.Customers.SelectedCustomer = customer;
                    }
                }
                break;

            // stats, settings는 엔티티 선택 없음
        }
    }
}
