using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Turner.Infrastructure.Mediator;
using TurnerStarterApi.Core.Data;
using TurnerStarterApi.Core.Extensions;

namespace TurnerStarterApi.Core.Features.Common
{
    public interface IEditRequest
    {
        int Id { get; set; }
    }

    public class EditRequestHandler<TRequest, TEntity, TGetDto> : IRequestHandler<TRequest, TGetDto>
        where TRequest : IRequest<TGetDto>, IEditRequest
        where TEntity : class, IEntity
    {
        protected readonly DbContext Context;

        protected EditRequestHandler(DbContext context)
        {
            Context = context;
        }

        public async Task<Response<TGetDto>> HandleAsync(TRequest request)
        {
            var entity = await GetEntity(request);

            if (entity == null)
            {
                return request.HasError($"{typeof(TEntity).Name.SplitWords()} " +
                                        $"with ID {request.Id} not found.", nameof(IEntity.Id));
            }

            Mapper.Map(request, entity);

            BeforeSave(request, entity);

            await Context.SaveChangesAsync();

            return entity
                .To<TGetDto>()
                .AsResponse();
        }

        protected virtual async Task<TEntity> GetEntity(TRequest request)
        {
            return await Context.Set<TEntity>()
                .Where(x => !x.IsDeleted)
                .SingleOrDefaultAsync(x => x.Id == request.Id);
        }

        protected virtual void BeforeSave(TRequest request, TEntity entity)
        {
        }
    }
}
