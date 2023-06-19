using Demo.Common.DBBase.Models.Base;
using Demo.Common.Utils;
using MongoDB.Bson.Serialization.Attributes;

namespace CompanyService.Entity.Api.Entities;
[BsonIgnoreExtraElements]

public class CompanyEntity : EntityBase
{
    [BsonElement("name")] public string? Name { get; set; }
    [BsonElement("description")] public string? Description { get; set; }
    [BsonElement("businessAreas")] public BusinessArea? BusinessAreas { get; set; }
}