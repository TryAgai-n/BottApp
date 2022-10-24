using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BottApp.Data
{
    public abstract class AbstractRepository<T>
        where T : AbstractModel
    {
        protected readonly DbSet<T> DbModel;

        protected readonly PostgreSqlContext Context;

        protected readonly ILogger<T> Logger;


        protected AbstractRepository(PostgreSqlContext context, ILoggerFactory loggerFactory)
        {
            Context = context;
            DbModel = context.Set<T>();
            Logger = loggerFactory.CreateLogger<T>();
        }

        public async Task<T?> FindOne(int id)
        {
            var model = await DbModel.FindAsync(id);
            return model;
        }


        public void Create(T entity)
        {
            DbModel.Add(entity);
        }


        public void Delete(T entity)
        {
            DbModel.Remove(entity);
        }


        public Task<List<T>> Find(Expression<Func<T, bool>> predicate)
        {
            return DbModel.Where(predicate).ToListAsync();
        }


        protected async Task<T> CreateModelAsync(T model)
        {
            await Context.AddAsync(model);
            var result = await Context.SaveChangesAsync();
            if (result == 0)
            {
                throw new Exception("Db error. Not Create any model");
            }

            return model;
        }


        public Task<int> UpdateModelAsync(T model)
        {
            DbModel.Update(model);
            return Context.SaveChangesAsync();
        }


        public Task<int> UpdateCollectionAsync(List<T> collection)
        {
            foreach (var model in collection)
            {
                DbModel.Update(model);
            }

            return Context.SaveChangesAsync();
        }


        public async Task DeleteModel(T model)
        {
            Delete(model);
            var result = await Context.SaveChangesAsync();
            if (result == 0)
            {
                throw new Exception("Db error. Not deleted");
            }

            
        }
    }
}

