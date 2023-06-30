namespace Demo.Services.UserService.Model.Response;

public enum ResponseCode
{
    SUCCESS = 0,

    #region [CUSTOMER]

    CUSTOMER_NOT_FOUND = 10000,

    #endregion
    
    #region [COMPANY]

    COMPANY_NOT_FOUND = 10000,

    #endregion

    #region [CLIENT]

    CLIENT_INPUT_INVALID = 30000,

    #endregion

    #region [SERVER]

    INTERNAL_SERVICE_ERROR = 50000,

    #endregion
}