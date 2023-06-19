namespace Demo.Services.AuthenService.Helper.ApiResponse;

public class ApiResponse
{
    /// <summary>
    /// </summary>
    /// <param name="code"></param>
    /// <param name="mesg"></param>
    public ApiResponse(ResponseCode code, string mesg)
    {
        Code = (int)code;
        CodeDesc = code.ToString();
        Message = mesg;
    }

    /// <summary>
    /// </summary>
    /// <param name="code"></param>
    /// <param name="mesg"></param>
    /// <param name="cont"></param>
    public ApiResponse(ResponseCode code, string mesg, object? cont)
    {
        Code = (int)code;
        CodeDesc = code.ToString();
        Message = mesg;
        Content = cont;
    }

    public int Code { get; set; }

    public string CodeDesc { get; set; }

    public string Message { get; set; }

    public object? Content { get; set; }
}