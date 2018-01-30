using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TurnerStarterApi.Core.Data;

namespace TurnerStarterApi.Tests.Core.Builders
{
    public class Builder<TBuilder, TEntity>
        where TEntity : class, IEntity, new()
        where TBuilder : Builder<TBuilder, TEntity>, new()
    {
        protected TEntity Entity = new TEntity();

        public static TBuilder Instance()
        {
            return new TBuilder();
        }

        public TBuilder AsDeleted()
        {
            Entity.IsDeleted = true;

            return (TBuilder)this;
        }

        public async Task<TEntity> PersistAndBuild(DbContext context)
        {
            context.Set<TEntity>().Add(Entity);
            await context.SaveChangesAsync();

            return Entity;
        }

        public TEntity Build()
        {
            return Entity;
        }
    }
}
