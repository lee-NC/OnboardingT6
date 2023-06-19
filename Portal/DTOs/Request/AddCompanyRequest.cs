using Demo.Common.Utils;

namespace Demo.ApiGateway.DTOs;

public class AddCompanyRequest
{
    public string Name { get; set; }

    public string Description { get; set; }

    public BusinessArea BusinessAreas { get; set; }
}