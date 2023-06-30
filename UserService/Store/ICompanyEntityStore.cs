using CompanyService.Entity.Api.Entities;
using CompanyService.Entity.Api.Repositories;
using Demo.Services.UserService.Entity.Api.Entities;
using Demo.Services.UserService.Entity.Api.Repositories;

namespace Demo.Services.UserService.Store;

public interface ICompanyEntityStore
{
    Task<CompanyEntity> FindByField(string name, string requestName);
    Task Create(CompanyEntity entity);
    Task<List<CompanyEntity>> GetAll();
    Task Delete(string value);
    Task Update(CompanyEntity entity);
}

public class CompanyEntityStore : ICompanyEntityStore
{
    private readonly ICompanyEntityRepository _companyRepo;

    public CompanyEntityStore(ICompanyEntityRepository companyRepo)
    {
        _companyRepo = companyRepo;
    }

    public async Task<CompanyEntity> FindByField(string fieldName, string value)
    {
        return await _companyRepo.FindByField(fieldName, value);
    }

    public async Task Create(CompanyEntity entity)
    {
        await _companyRepo.Create(entity);
        return;
    }

    public async Task<List<CompanyEntity>> GetAll()
    {
        return await _companyRepo.GetAll() as List<CompanyEntity>;
    }

    public async Task Delete(string value)
    {
         _companyRepo.Delete(value);
        return;
    }
    
    public async Task Update(CompanyEntity entity)
    {
        _companyRepo.Update(entity, entity.Id);
        return;
    }
}