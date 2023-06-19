namespace Demo.Services.AuthenService.Helper.ApiResponse;

public enum ResponseCode
{
    SUCCESS = 0,

    UNAUTHORIZED = 401,

    #region [CLIENT]

    CLIENT_INPUT_INVALID = 400,
    CUSTOMER_NOT_FOUND = 404,

    #endregion

    #region [SERVER]

    SERVER_INTERNAL_ERROR = 500,
    SERVER_METHOD_NOT_SUPPORTED = 501,
    SERVER_NO_RESULT = 502

    #endregion

    ,
}