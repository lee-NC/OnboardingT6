namespace Demo.Services.AuthenService.Helper.ApiResponse
{
    public class ApiResponse
    {
        private int _code;
        public int Code { get { return _code; } set { _code = value; } }
        public string CodeDesc { get; set; }

        public string Message { get; set; }

        public object? Content { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="mesg"></param>
        public ApiResponse(ResponseCode code, string mesg)
        {
            _code = (int)code;
            CodeDesc = code.ToString();
            Message = mesg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="mesg"></param>
        /// <param name="cont"></param>
        public ApiResponse(ResponseCode code, string mesg, object? cont)
        {
            _code = (int)code;
            CodeDesc = code.ToString();
            Message = mesg;
            Content = cont;
        }
    }
}
