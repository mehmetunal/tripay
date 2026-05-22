using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriPay.Admin.Application.Dtos.Outbox;
using TriPay.Admin.Application.Mappings;
using TriPay.Admin.Application.Services;
using TriPay.Admin.Authorization;
using TriPay.Admin.Infrastructure;
using TriPay.Admin.Models.Outbox;
using TriPay.Admin.Models.Shared;
using TriPay.Data.Identity;

namespace TriPay.Admin.Controllers;

/// <summary>Webhook outbox kuyruğu.</summary>
[Authorize(Policy = AdminPolicies.OutboxView)]
public sealed class OutboxController : Controller
{
    private readonly IAdminOutboxService _outbox;

    public OutboxController(IAdminOutboxService outbox) => _outbox = outbox;

    public async Task<IActionResult> Index(PaginationQuery query, bool? unpublishedOnly, CancellationToken cancellationToken)
    {
        var page = await _outbox.ListAsync(new OutboxListQueryDto
        {
            UnpublishedOnly = unpublishedOnly,
            Page = query.Page,
            PageSize = query.PageSize
        }, cancellationToken);

        ViewBag.UnpublishedOnly = unpublishedOnly;
        ViewData["AdminModule"] = "outbox.index";
        var model = new PagedResult<OutboxListItem>
        {
            Items = page.Items.Select(AdminDtoMapper.ToListItem).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        };

        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_IndexContent", model) : View(model);
    }

    public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
    {
        var item = await _outbox.GetDetailAsync(id, cancellationToken);
        if (item == null)
            return NotFound();

        ViewData["AdminModule"] = "outbox.details";
        return AdminMvcAjax.IsAjaxRequest(Request) ? PartialView("_DetailsContent", item) : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AdminPolicies.OutboxManage)]
    public async Task<IActionResult> Requeue(long id, CancellationToken cancellationToken)
    {
        if (!await _outbox.RequeueAsync(id, cancellationToken))
            return NotFound();

        if (AdminMvcAjax.IsAjaxRequest(Request))
            return AdminMvcAjax.JsonSuccess("Outbox mesajı yeniden kuyruğa alındı.", Url.Action(nameof(Details), new { id })!);

        TempData["Success"] = "Outbox mesajı yeniden kuyruğa alındı; dispatcher yayınlayacaktır.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
