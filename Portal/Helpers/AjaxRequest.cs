using Newtonsoft.Json;

namespace Demo.Portal.Helpers
{
    public static class AjaxRequest
    {
        public enum AjaxRequestStatus
        {
            Success = 0,
            Error = 1,
            Warning = 3,
            LoginRequest = 2
        }
        public class AjaxResult
        {
            public int Status { get; set; }
            public object Object { get; set; }
            public string Message { get; set; }
        }
        public static AjaxResult ToSuccessAjaxResult(this object data)
        {
            AjaxResult result = new AjaxResult
            {
                Status = (int)AjaxRequestStatus.Success,
                Object = JsonConvert.SerializeObject(data)
            };
            return (result);
        }
        public static AjaxResult ToErrorAjaxResult(this string mess)
        {
            AjaxResult result = new AjaxResult
            {
                Status = (int)AjaxRequestStatus.Error,
                Object = mess
            };
            return (result);
        }
        public static AjaxResult ToWarningAjaxResult(this string mess)
        {
            AjaxResult result = new AjaxResult
            {
                Status = (int)AjaxRequestStatus.Warning,
                Object = mess
            };
            return (result);
        }
        public static AjaxResult ToExeptionAjaxResult(this Exception ex)
        {
            AjaxResult result = new AjaxResult
            {
                Status = (int)AjaxRequestStatus.Error,
                Object = "Có lỗi xảy ra. Liên hệ quản trị viên",
                Message = ex.Message
            };

            if (ex.Message == "AccessTokenNull")
            {
                result = new AjaxResult
                {
                    Status = (int)AjaxRequestStatus.LoginRequest,
                    Object = "Vui lòng đăng nhập hệ thống",
                    Message = ""
                };
            }
            return (result);
        }
    }
}