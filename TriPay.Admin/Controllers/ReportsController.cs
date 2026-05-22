using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TriPay.Admin.Application.Dtos.Reports;
using TriPay.Admin.Application.Services;
using TriPay.Admin.Authorization;
using TriPay.Admin.Infrastructure;
using TriPay.Admin.Models.Reports;

namespace TriPay.Admin.Controllers;

/// <summary>Ödeme ve işlem raporları.</summary>
[Authorize(Policy = AdminPolicies.ReportsView)]
public sealed class ReportsController : Controller
{
    private readonly IAdminReportsService _reports;

    public ReportsController(IAdminReportsService reports) => _reports = reports;

    public async Task<IActionResult> Index(ReportsFilterModel filter, CancellationToken cancellationToken)
    {
        var dto = await _reports.GetIndexAsync(ToFilterDto(filter), cancellationToken);

        ViewBag.Filter = new ReportsFilterModel
        {
            FromUtc = dto.Filter.FromUtc,
            ToUtc = dto.Filter.ToUtc,
            MerchantId = dto.Filter.MerchantId,
            PaymentGatewayId = dto.Filter.PaymentGatewayId
        };
        ViewBag.Merchants = new SelectList(dto.Merchants, "Id", "Name", dto.Filter.MerchantId);
        ViewBag.Gateways = new SelectList(dto.Gateways, "Id", "Name", dto.Filter.PaymentGatewayId);

        ViewData["Title"] = "Raporlar";
        ViewData["AdminModule"] = "reports.index";

        return AdminMvcAjax.IsAjaxRequest(Request)
            ? PartialView("_IndexContent", dto)
            : View(dto);
    }

    private static ReportsFilterDto ToFilterDto(ReportsFilterModel filter)
    {
        DateTime? fromUtc = null;
        DateTime? toUtc = null;

        if (filter.FromUtc.HasValue)
        {
            var local = DateTime.SpecifyKind(filter.FromUtc.Value.Date, DateTimeKind.Local);
            fromUtc = local.ToUniversalTime();
        }

        if (filter.ToUtc.HasValue)
        {
            var localEnd = DateTime.SpecifyKind(filter.ToUtc.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Local);
            toUtc = localEnd.ToUniversalTime();
        }

        return new ReportsFilterDto
        {
            FromUtc = fromUtc ?? default,
            ToUtc = toUtc ?? default,
            MerchantId = filter.MerchantId,
            PaymentGatewayId = filter.PaymentGatewayId
        };
    }
}
