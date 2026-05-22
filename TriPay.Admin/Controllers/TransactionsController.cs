using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TriPay.Admin.Application.Mappings;
using TriPay.Admin.Application.Services;
using TriPay.Admin.Authorization;
using TriPay.Admin.Infrastructure;
using TriPay.Admin.Models.Shared;
using TriPay.Admin.Models.Transactions;
using TriPay.Data.Constants;
using TriPay.Data.Identity;

namespace TriPay.Admin.Controllers;

/// <summary>Ödeme işlemleri inceleme.</summary>
[Authorize(Policy = AdminPolicies.TransactionsView)]
public sealed class TransactionsController : Controller
{
    private readonly IAdminTransactionService _transactions;

    public TransactionsController(IAdminTransactionService transactions) => _transactions = transactions;

    public async Task<IActionResult> Index(TransactionListFilter filter, CancellationToken cancellationToken)
    {
        var result = await _transactions.GetIndexAsync(AdminDtoMapper.ToQueryDto(filter), cancellationToken);

        ViewBag.Filter = filter;
        ViewBag.Merchants = new SelectList(result.Merchants, "Id", "Name", filter.MerchantId);
        ViewBag.Gateways = new SelectList(result.Gateways, "Id", "Name", filter.PaymentGatewayId);
        ViewBag.Statuses = new SelectList(new[]
        {
            TransactionStatuses.Pending,
            TransactionStatuses.Success,
            TransactionStatuses.Failed
        }, filter.Status);

        ViewData["AdminModule"] = "transactions.index";
        var model = new PagedResult<TransactionListItem>
        {
            Items = result.Page.Items.Select(AdminDtoMapper.ToListItem).ToList(),
            Page = result.Page.Page,
            PageSize = result.Page.PageSize,
            TotalCount = result.Page.TotalCount
        };

        return AdminMvcAjax.IsAjaxRequest(Request)
            ? PartialView("_IndexContent", model)
            : View(model);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var dto = await _transactions.GetDetailAsync(id, cancellationToken);
        if (dto == null)
            return NotFound();

        var model = AdminDtoMapper.ToDetailViewModel(dto);
        return AdminMvcAjax.ViewOrPartial(this, "_DetailsContent", model);
    }
}
