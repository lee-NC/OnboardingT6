namespace Services.CompanyService.Model.Response;

public class ApiResponse
{
    public ApiResponse()
    {
    }

    public ApiResponse(ResponseCode code, string message)
    {
        Code = (int)code;
        CodeDesc = code.ToString();
        Message = message;
    }

    public ApiResponse(ResponseCode code, string message, object content)
    {
        Code = (int)code;
        CodeDesc = code.ToString();
        Message = message;
        Content = content;
    }

    public int Code { get; set; }

    public string? CodeDesc { get; set; }

    public string? Message { get; set; }

    public object? Content { get; set; }
}