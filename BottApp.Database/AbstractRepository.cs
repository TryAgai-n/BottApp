using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BottApp.Database;

public abstract class AbstractRepository<T> where T : AbstractModel
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
    
    
}