using Demo.Common.Utils;

namespace Demo.ApiGateway.DTOs;

public class UpdateCompanyRequest
{
    public string Id { get; set; }
    
    public string Description { get; set; } 
    
    public BusinessArea BusinessAreas { get; set; } 

}