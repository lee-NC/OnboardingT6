using System.Linq.Expressions;
using Demo.Common.DBBase.Models.Base;
using Microsoft.EntityFrameworkCore;

namespace Demo.Common.DBBase.Repos.Base;

public interface IRepositoryBase<T> : IDisposable where T : EntityBase, new()
{
    DbSet<T> Table { get; }
    DbContext Context { get; }
    T Find(Guid? id);
    T FindAsNoTracking(Guid id);
    T FindIgnoreQueryFilters(Guid id);
    IEnumerable<T> GetAll();
    IEnumerable<T> GetAll(IList<(Expression<Func<T, object>> orderBy, bool desc)> orders);
    IEnumerable<T> GetRange(IQueryable<T> query, int skip, int take);
    int Add(T entity, bool persist = true);
    int AddRange(IEnumerable<T> entities, bool persist = true);
    int Update(T entity, bool persist = true);
    int UpdateRange(IEnumerable<T> entities, bool persist = true);
    int Delete(T entity, bool persist = true);
    int DeleteRange(IEnumerable<T> entities, bool persist = true);
    int SaveChanges();
}