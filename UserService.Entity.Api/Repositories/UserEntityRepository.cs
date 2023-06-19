using Demo.Common.DBBase.Context;
using Demo.Common.Utils;
using Demo.Services.UserService.Entity.Api.Entities;
using MongoDB.Driver;

namespace Demo.Services.UserService.Entity.Api.Repositories;

public class UserEntityRepository : MongoDbRepositoryBase<UserEntity>, IUserEntityRepository
{
    public UserEntityRepository(IMongoDbContext context) : base(context)
    {
    }

    public async Task<UserEntity> FindByField(string fieldName, string value)
    {
        var filter = Builders<UserEntity>.Filter.Eq(fieldName, value);

        return await _dbCollection.FindAsync(filter).Result.FirstOrDefaultAsync();
    }

    public async Task<List<UserEntity>> GetAllByCompanyId(string? companyId)
    {
        var filter = Builders<UserEntity>.Filter
            .Where(c => c.CompanyId!.ToLower() == companyId!.ToLower());

        var all = await _dbCollection.FindAsync(filter);
        return await all.ToListAsync();
    }

    public async Task<object> SignOn(string username, string password, string ip)
    {
        if (username == null) throw new ArgumentNullException(nameof(username));

        var filter = Builders<UserEntity>.Filter
            .Where(c => c.UserPass!.Username!.ToLower() == username!.ToLower());

        var sort = Builders<UserEntity>.Sort.Descending(nameof(UserEntity.DataCreated));

        var options = new FindOptions<UserEntity>
        {
            Sort = sort
        };

        var userEntity = await _dbCollection.FindAsync(filter, options).Result.FirstOrDefaultAsync();
        if (userEntity == null) return null;


        var checkAuthenFailIp = await CheckAuthenFailIp(ip, userEntity);
        if (!checkAuthenFailIp) return null;

        // if (userEntity.Status == UserEntity.Statuses.ACTIVE) return null;

        var credential = userEntity.UserPass;

        var result = EntityUtils.VerifyPassword(
            credential.PasswordHash, password, credential.PasswordSalt);
        if (!result)
        {
            await UpdateFailAuthen(ip, userEntity);
            return null;
        }

        await UpdateSuccessAuthen(ip, userEntity);

        // userEntity.Status = UserEntity.Statuses.ACTIVE;
        // userEntity.StatusDesc = UserEntity.Statuses.ACTIVE.ToString();
        Update(userEntity, userEntity.Id);
        return userEntity;
    }

    public async Task<UserEntity> FindByUsername(string value)
    {
        var filter = Builders<UserEntity>.Filter
            .Where(c => c.UserPass.Username!.ToLower() == value!.ToLower());
        var userEntity = await _dbCollection.FindAsync(filter).Result.FirstOrDefaultAsync();
        return userEntity;
    }

    public async Task<UserEntity?> SignOut(string? username)
    {
        if (username == null) throw new ArgumentNullException(nameof(username));

        var filter = Builders<UserEntity>.Filter
            .Where(c => c.UserPass.Username!.ToLower() == username!.ToLower());

        var sort = Builders<UserEntity>.Sort.Descending(nameof(UserEntity.DataCreated));

        var options = new FindOptions<UserEntity>
        {
            Sort = sort
        };

        var userEntity = await _dbCollection.FindAsync(filter, options).Result.FirstOrDefaultAsync();
        if (userEntity == null) return null;

        // userEntity.Status = UserEntity.Statuses.INACTIVE;
        // userEntity.StatusDesc = UserEntity.Statuses.INACTIVE.ToString();
        Update(userEntity, userEntity.Id);
        return userEntity;
    }

    public async Task<bool> CheckAuthenFailIp(string? ip, UserEntity userEntity)
    {
        if (userEntity.FailLoginIpList == null || userEntity.FailLoginIpList.Count == 0) return true;

        var ipObj = userEntity.FailLoginIpList.Where(e => e.Ip == ip).FirstOrDefault();
        if (ipObj == null) return true;

        if (ipObj.LastFailAuthenTime.AddMinutes(FailLoginIp.LOCK_MINUTES) < DateTime.UtcNow)
        {
            userEntity.FailLoginIpList.Remove(ipObj);
            await Update(userEntity, userEntity.Id);
            return true;
        }

        if (ipObj.FailAttempCount > FailLoginIp.MAX_ATTEMPS) return false;

        return true;
    }

    public async Task UpdateFailAuthen(string? ip, UserEntity userEntity)
    {
        if (userEntity.FailLoginIpList == null || userEntity.FailLoginIpList.Count == 0)
            userEntity.FailLoginIpList = new List<FailLoginIp>();

        var ipObj = userEntity.FailLoginIpList.Where(e => e.Ip == ip).FirstOrDefault();
        if (ipObj == null)
        {
            ipObj = new FailLoginIp
            {
                FailAttempCount = 0,
                Ip = ip
            };
            userEntity.FailLoginIpList.Add(ipObj);
        }

        ipObj.LastFailAuthenTime = DateTime.UtcNow;
        ipObj.FailAttempCount++;

        await Update(userEntity, userEntity.Id);
    }

    public async Task UpdateSuccessAuthen(string? ip, UserEntity userEntity)
    {
        if (userEntity.FailLoginIpList == null || userEntity.FailLoginIpList.Count == 0) return;

        var ipObj = userEntity.FailLoginIpList.Where(e => e.Ip == ip).FirstOrDefault();
        if (ipObj == null) return;

        userEntity.FailLoginIpList.Remove(ipObj);

        await Update(userEntity, userEntity.Id);
    }


    public async Task<UserEntity?> UserPassCheck(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return null;

        var filter = Builders<UserEntity>.Filter
            .Where(c => c.UserPass.Username!.ToLower() == username.ToLower());

        var sort = Builders<UserEntity>.Sort.Descending(nameof(UserEntity.DataCreated));

        var options = new FindOptions<UserEntity>
        {
            Sort = sort
        };

        var userEntity = await _dbCollection.FindAsync(filter, options).Result.FirstOrDefaultAsync();
        if (userEntity == null) return null;

        var credential = userEntity.UserPass;
        if (credential == null) return null;

        var result = EntityUtils.VerifyPassword(
            credential.PasswordHash, password, credential.PasswordSalt);
        if (!result) return null;

        return userEntity;
    }
}