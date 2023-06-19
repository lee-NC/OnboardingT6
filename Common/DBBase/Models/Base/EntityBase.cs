using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Demo.Common.DBBase.Models.Base;

public class EntityBase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
    [BsonRepresentation(BsonType.String)]
    public ObjectId Id { get; set; }

    [Timestamp]
    [BsonElement("data_created")]
    [BsonIgnoreIfDefault]
    public DateTime DataCreated { get; set; } = DateTime.UtcNow;

    [Timestamp]
    [BsonElement("date_updated")]
    [BsonIgnoreIfNull]
    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
}