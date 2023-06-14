namespace Demo.Services.AuthenService.Helper.ApiResponse
{
    public enum ResponseCode : int
    {
        SUCCESS = 0,

        UNAUTHORIZED = 401,

        #region [CLIENT]
        CLIENT_INPUT_INVALID = 400,
        #endregion

        #region [SERVER]
        SERVAR_INTERNAL_ERROR = 500,
        SERVER_METHOD_NOT_SUPPORTED = 501,
        SERVER_NO_RESULT = 502
        #endregion
    }
}
