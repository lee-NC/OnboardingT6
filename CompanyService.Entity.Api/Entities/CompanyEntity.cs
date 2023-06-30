using Demo.Common.DBBase.Models.Base;
using Demo.Common.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CompanyService.Entity.Api.Entities;
[BsonIgnoreExtraElements]

public class CompanyEntity : EntityBase
{
    [BsonElement("name")] public string? Name { get; set; }
    [BsonElement("description")] public string? Description { get; set; }
    [BsonElement("businessAreas")] public BusinessArea? BusinessAreas { get; set; }
    
    [BsonElement("employees")] public List<Employee>? Employees { get; set; }
}

public class Employee
{
    [BsonElement("userId")] public ObjectId? UserId { get; set; }
    [BsonElement("sex")] public GenderEnum? Sex { get; set; }
    [BsonElement("lastName")] public string? LastName { get; set; }
    [BsonElement("firstName")] public string? FirstName { get; set; }
    
    [BsonElement("role")] public RoleEnum? Role { get; set; }
}