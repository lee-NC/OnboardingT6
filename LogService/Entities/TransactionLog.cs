using Demo.Common.DBBase.Models.Base;
using MongoDB.Bson.Serialization.Attributes;

namespace LogService.Entities;

[BsonIgnoreExtraElements]
public class TransactionLog : EntityBase
{
    [BsonElement("user")] public TransactionCustomer Customer { get; set; }

    [BsonElement("request")] public byte[] Request { get; set; }

    [BsonElement("rejectReason")] public string RejectReason { get; set; }

    [BsonElement("response")] public byte[] Response { get; set; }
}

public class TransactionCustomer
{
    [BsonElement("id")] public string Id { get; set; }
    [BsonElement("name")] public string Username { get; set; }
}