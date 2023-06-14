using Demo.Common.DBBase.Models.Base;
using Demo.Common.Utils;
using MongoDB.Bson.Serialization.Attributes;

namespace Demo.Services.Entities
{
    [BsonIgnoreExtraElements]
    public class UserEntity : EntityBase
    {
        [BsonElement("sex")] public GenderEnum? Sex { get; set; }
        [BsonElement("lastName")] public string? LastName { get; set; }
        [BsonElement("firstName")] public string? FirstName { get; set; }
        [BsonElement("dateOfBirth")] public DateOnly? DateOfBirth { get; set; }
        [BsonElement("email")] public string? Email { get; set; }
        [BsonElement("companyId")] public string? CompanyId { get; set; }
        
        [BsonElement("userPass")] public UserPassCredential? UserPass { get; set; }
        [BsonElement("clientIp")] public ClientIpCredential? ClientIP { get; set; }
        public IList<FailLoginIp>? FailLoginIpList { get; set; }
        [BsonElement("role")] public RoleEnum? Role { get; set; }
        private Statuses _status;

        [BsonElement("status")]
        public Statuses Status
        {
            get { return _status; }
            set
            {
                _status = value;
                StatusDesc = _status.ToString();
            }
        }

        [BsonElement("statusDesc")] public string StatusDesc { get; set; }

        public enum Statuses : byte
        {
            NEW = 0,
            ACTIVE = 1,
            LOCK = 2,
            SUSPENDED = 3,
            INACTIVE = 98,
            REMOVED = 99
        }
    }

    public class FailLoginIp
    {
        public string? Ip { get; set; }
        public int FailAttempCount { get; set; }
        public DateTime LastFailAuthenTime { get; set; }

        public const int MAX_ATTEMPS = 5;
        public const int LOCK_MINUTES = 5;
    }
    
    public class BaseCredential
    {
        private Statuses _status;

        [BsonElement("status")]
        public Statuses Status
        {
            get { return _status; }
            set
            {
                _status = value;
                StatusDesc = _status.ToString();
            }
        }

        [BsonElement("statusDesc")] public string? StatusDesc { get; set; }
        [BsonElement("createDate")] public DateTime? CreatedDate { get; set; }
        [BsonElement("modDate")] public DateTime? UpdatedDate { get; set; }

        public enum Statuses : byte
        {
            ENABLE = 200,
            DISABLE = 100
        }
    }

    [BsonIgnoreExtraElements]
    public class ClientIpCredential : BaseCredential
    {
        [BsonElement("ip")] public string? IP { get; set; }
        [BsonElement("invalidAttemptsCount")] public byte InvalidAttemptsCount { get; set; }
    }
    
    [BsonIgnoreExtraElements]
    public class UserPassCredential : BaseCredential
    {
        [BsonElement("username")]
        public string? Username { get; set; }
        [BsonElement("passHash")]
        public byte[]? PasswordHash { get; set; }
        [BsonElement("passSalt")]
        public byte[]? PasswordSalt { get; set; }
        [BsonElement("accessFailedCount")] public int AccessFailedCount { get; set; }
        [BsonElement("lockedDate")]
        public DateTime? LockedDate { get; set; }

        [BsonElement("isLocked")]
        public bool IsLocked { get; set; }
        //Trạng thái tài khoản đang bị khóa đăng nhập (sẽ tự mở sau 5 phút từ khi bị khóa)

        public bool IsLocking
        {
            get
            {
                return IsLocked && LockedDate.HasValue && LockedDate.Value.AddMinutes(5) > DateTime.UtcNow;
            }
        }
    }
}