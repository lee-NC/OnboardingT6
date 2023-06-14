
namespace Services.UserService.Model.Response
{
    public class ApiResponse
    {
        private int _code;
        public int Code { get { return _code; } set { _code = value; } }
        public string? CodeDesc { get; set; }

        public string? Message { get; set; }

        public object? Content { get; set; }

        public ApiResponse() { }

        public ApiResponse(ResponseCode code, string message)
        {
            _code = (int)code;
            CodeDesc = code.ToString();
            Message = message;
        }

        public ApiResponse(ResponseCode code, string message, object content)
        {
            _code = (int)code;
            CodeDesc = code.ToString();
            Message = message;
            Content = content;
        }
    }
}
