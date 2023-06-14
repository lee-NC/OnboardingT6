using Demo.Common.DBBase.Context;
using Demo.Common.Utils;
using Demo.Services.Entities;
using MongoDB.Driver;

namespace Demo.Services.UserService.Repositories;

public partial class UserEntityRepository : MongoDbRepositoryBase<UserEntity>, IUserEntityRepository
{
    public UserEntityRepository(IMongoDbContext context) : base(context)
    {
    }

    public async Task<UserEntity> FindByField(string fieldName, string value)
    {
        FilterDefinition<UserEntity> filter = Builders<UserEntity>.Filter.Eq(fieldName, value);

        return await _dbCollection.FindAsync(filter).Result.FirstOrDefaultAsync();
    }

    public async Task<List<UserEntity>> GetAllByCompanyId(string? companyId)
    {
        FilterDefinition<UserEntity> filter = Builders<UserEntity>.Filter
            .Where(c => c.CompanyId!.ToLower() == companyId!.ToLower());

        var all = await _dbCollection.FindAsync(filter);
        return await all.ToListAsync();
    }

    public async Task<object> SignOn(string username, string password, string ip)
    {
        if(username == null)
        {
            throw new ArgumentNullException(nameof(username));
        }

        FilterDefinition<UserEntity> filter = Builders<UserEntity>.Filter
            .Where(c => c.UserPass!.Username!.ToLower() == username!.ToLower());

        var sort = Builders<UserEntity>.Sort.Descending(nameof(UserEntity.DataCreated));

        var options = new FindOptions<UserEntity>
        {
            Sort = sort
        };

        var customer = await _dbCollection.FindAsync(filter, options).Result.FirstOrDefaultAsync();
        if (customer == null)
        {
            return null;
        }


        var checkAuthenFailIp = await CheckAuthenFailIp(ip, customer);
        if (!checkAuthenFailIp)
        {
            return null;
        }
        
        if (customer.Status == UserEntity.Statuses.ACTIVE)
        {
            return null;
        }

        var credential = customer.UserPass;

        var result = EntityUtils.VerifyPassword(
            credential.PasswordHash, password, credential.PasswordSalt);
        if (!result)
        {
            await UpdateFailAuthen(ip, customer);
            return null;
        }

        await UpdateSuccessAuthen(ip, customer);
        return customer;
    }

    public async Task<UserEntity> FindByUsername(string value)
    {
        FilterDefinition<UserEntity> filter = Builders<UserEntity>.Filter
            .Where(c => c.UserPass.Username!.ToLower() == value!.ToLower());
        var customer = await _dbCollection.FindAsync(filter).Result.FirstOrDefaultAsync();
        return customer;
    }

    public async Task<UserEntity?> SignOut(string? username, string? password, string? ip)
    {
        if (username == null)
        {
            throw new ArgumentNullException(nameof(username));
        }

        FilterDefinition<UserEntity> filter = Builders<UserEntity>.Filter
            .Where(c => c.UserPass.Username!.ToLower() == username!.ToLower());

        var sort = Builders<UserEntity>.Sort.Descending(nameof(UserEntity.DataCreated));

        var options = new FindOptions<UserEntity>
        {
            Sort = sort
        };

        var customer = await _dbCollection.FindAsync(filter, options).Result.FirstOrDefaultAsync();
        if (customer == null)
        {
            return null;
        }


        var checkAuthenFailIp = await CheckAuthenFailIp(ip, customer);
        if (!checkAuthenFailIp)
        {
            return null;
        }

        var credential = customer.UserPass;
        if (credential == null) return null;

        var result = EntityUtils.VerifyPassword(
            credential.PasswordHash, password, credential.PasswordSalt);
        if (!result)
        {
            await UpdateFailAuthen(ip, customer);
            return null;
        }

        await UpdateSuccessAuthen(ip, customer);
        return customer;
    }

    public async Task<bool> CheckAuthenFailIp(string? ip, UserEntity customer)
    {
        if (customer.FailLoginIpList == null || customer.FailLoginIpList.Count == 0)
        {
            return true;
        }

        var ipObj = customer.FailLoginIpList.Where(e => e.Ip == ip).FirstOrDefault();
        if (ipObj == null)
        {
            return true;
        }

        if (ipObj.LastFailAuthenTime.AddMinutes(FailLoginIp.LOCK_MINUTES) < DateTime.UtcNow)
        {
            customer.FailLoginIpList.Remove(ipObj);
            await Update(customer, customer.Id);
            return true;
        }

        if (ipObj.FailAttempCount > FailLoginIp.MAX_ATTEMPS)
        {
            return false;
        }

        return true;
    }

    public async Task UpdateFailAuthen(string? ip, UserEntity customer)
    {
        if (customer.FailLoginIpList == null || customer.FailLoginIpList.Count == 0)
        {
            customer.FailLoginIpList = new List<FailLoginIp>();
        }

        var ipObj = customer.FailLoginIpList.Where(e => e.Ip == ip).FirstOrDefault();
        if (ipObj == null)
        {
            ipObj = new FailLoginIp
            {
                FailAttempCount = 0,
                Ip = ip
            };
            customer.FailLoginIpList.Add(ipObj);
        }

        ipObj.LastFailAuthenTime = DateTime.UtcNow;
        ipObj.FailAttempCount++;

        await Update(customer, customer.Id);
    }

    public async Task UpdateSuccessAuthen(string? ip, UserEntity customer)
    {
        if (customer.FailLoginIpList == null || customer.FailLoginIpList.Count == 0)
        {
            return;
        }

        var ipObj = customer.FailLoginIpList.Where(e => e.Ip == ip).FirstOrDefault();
        if (ipObj == null)
        {
            return;
        }

        customer.FailLoginIpList.Remove(ipObj);

        await Update(customer, customer.Id);
    }


    public async Task<UserEntity?> UserPassCheck(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        FilterDefinition<UserEntity> filter = Builders<UserEntity>.Filter
            .Where(c => c.UserPass.Username!.ToLower() == username.ToLower());

        var sort = Builders<UserEntity>.Sort.Descending(nameof(UserEntity.DataCreated));

        var options = new FindOptions<UserEntity>
        {
            Sort = sort
        };

        var customer = await _dbCollection.FindAsync(filter, options).Result.FirstOrDefaultAsync();
        if (customer == null)
        {
            return null;
        }

        var credential = customer.UserPass;
        if (credential == null) return null;

        var result = EntityUtils.VerifyPassword(
            credential.PasswordHash, password, credential.PasswordSalt);
        if (!result)
        {
            return null;
        }

        return customer;
    }
}