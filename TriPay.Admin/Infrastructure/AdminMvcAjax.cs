using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TriPay.Admin.Infrastructure;

/// <summary>Admin panel AJAX (X-Requested-With) yardımcıları.</summary>
public static class AdminMvcAjax
{
    /// <summary>İstek XMLHttpRequest ile mi geldi.</summary>
    public static bool IsAjaxRequest(HttpRequest request) =>
        string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

    /// <summary>AJAX ise PartialView, değilse tam View döner.</summary>
    public static IActionResult ViewOrPartial(Controller controller, string viewName, object? model)
    {
        if (IsAjaxRequest(controller.Request))
            return controller.PartialView(viewName, model);
        return controller.View(viewName, model);
    }

    /// <summary>AJAX ise PartialView, değilse action adıyla View.</summary>
    public static IActionResult ViewOrPartial(Controller controller, object? model) =>
        ViewOrPartial(controller, controller.RouteData.Values["action"]?.ToString() ?? "Index", model);

    /// <summary>Form POST başarılı JSON yanıtı.</summary>
    public static JsonResult JsonSuccess(string message, string redirectUrl) =>
        new(new { success = true, message, redirectUrl });

    /// <summary>Form POST doğrulama hatası JSON yanıtı.</summary>
    public static JsonResult JsonValidationErrors(ModelStateDictionary modelState, string? message = null)
    {
        var errors = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                x => string.IsNullOrEmpty(x.Key) ? "" : x.Key,
                x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        return new JsonResult(new { success = false, message = message ?? "Doğrulama hatası.", errors })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    /// <summary>AJAX POST: geçersizse JSON, geçerliyse callback sonucu.</summary>
    public static IActionResult JsonForm(Controller controller, Func<IActionResult> onValid)
    {
        if (!IsAjaxRequest(controller.Request))
            return onValid();

        if (!controller.ModelState.IsValid)
            return JsonValidationErrors(controller.ModelState);

        return onValid();
    }

    /// <summary>AJAX POST: geçersizse JSON veya View, geçerliyse callback.</summary>
    public static async Task<IActionResult> JsonFormAsync(
        Controller controller,
        object? invalidModel,
        Func<Task<IActionResult>> onValid)
    {
        if (!IsAjaxRequest(controller.Request))
            return await onValid();

        if (!controller.ModelState.IsValid)
            return JsonValidationErrors(controller.ModelState);

        return await onValid();
    }

    /// <summary>AJAX POST: geçersizse JSON veya invalidView, geçerliyse callback.</summary>
    public static async Task<IActionResult> JsonFormAsync(
        Controller controller,
        string invalidViewName,
        object? invalidModel,
        Func<Task<IActionResult>> onValid)
    {
        if (!IsAjaxRequest(controller.Request))
            return await onValid();

        if (!controller.ModelState.IsValid)
            return JsonValidationErrors(controller.ModelState);

        return await onValid();
    }
}
