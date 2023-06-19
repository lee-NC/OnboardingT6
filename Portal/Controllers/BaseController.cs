using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Demo.Portal.Controllers;

public class BaseController : Controller
{
    private static string _accessToken = "";
    private static long _tokenExpiredAt;
    protected readonly ILogger<BaseController> _logger;

    protected async Task<string> GetAccessToken()
    {
        var now = DateTime.Now;
        if (string.IsNullOrEmpty(_accessToken) || _tokenExpiredAt - now.Ticks <= 300000)
            try
            {
                _accessToken = await HttpContext.GetTokenAsync("access_token");
                var expAtStr = await HttpContext.GetTokenAsync("expires_at");
                var expirer = Convert.ToDateTime(expAtStr);
                _tokenExpiredAt = expirer.Ticks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "GetAccessToken Error");
            }

        return _accessToken;
    }

    public void UserAccessDenied()
    {
        TempData["AccessDenied"] = "Access Denied";
    }

    public void ShowNotifyError(string str)
    {
        TempData["NotifyError"] = str;
    }

    public void ShowNotifySuccess(string str)
    {
        TempData["NotifySuccess"] = str;
    }

    public void ShowNotifyWarning(string str)
    {
        TempData["NotifyWarning"] = str;
    }

    public void ShowNotifyInfo(string str)
    {
        TempData["NotifyInfo"] = str;
    }

    public void ShowNotifyException()
    {
        TempData["NotifyError"] = "Có lỗi xảy ra. Liên hệ quản trị viên";
    }
}

public static class ControllerUtis
{
    public static string ToStringError(this ModelStateDictionary modelState)
    {
        var msg = "";
        var ltError = modelState.Values.SelectMany(x => x.Errors);
        if (ltError != null && ltError.Count() > 0)
        {
            var ltErrorMess = ltError.Select(y => y.ErrorMessage);
            if (ltErrorMess != null && ltErrorMess.Count() > 0)
                msg = string.Join("; ", ltErrorMess);
        }

        return msg;
    }

    public static ModelStateDictionary NotValidateModel(this ModelStateDictionary modelState, object obj)
    {
        if (obj != null)
            foreach (var prop in obj.GetType().GetProperties())
                if (modelState.ContainsKey(prop.Name))
                    modelState[prop.Name].Errors.Clear();
        return modelState;
    }
}