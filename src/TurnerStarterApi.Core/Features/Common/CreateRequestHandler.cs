using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Turner.Infrastructure.Mediator;
using TurnerStarterApi.Core.Extensions;

namespace TurnerStarterApi.Core.Features.Common
{
    public class CreateRequestHandler<TRequest, TEntity, TGetDto> : IRequestHandler<TRequest, TGetDto> 
        where TRequest : IRequest<TGetDto> 
        where TEntity : class
    {
        protected readonly DbContext DataContext;

        protected CreateRequestHandler(DbContext dataContext)
        {
            DataContext = dataContext;
        }

        public async Task<Response<TGetDto>> HandleAsync(TRequest request)
        {
            var entity = request.To<TEntity>();

            await DataContext.AddAsync(entity);

            await BeforeSave(entity);

            await DataContext.SaveChangesAsync();

            var dto = entity.To<TGetDto>();

            await BeforeReturn(dto);

            return dto.AsResponse();
        }

        protected virtual Task BeforeSave(TEntity entity)
        {
            return Task.FromResult(0);
        }

        protected virtual Task BeforeReturn(TGetDto dto)
        {
            return Task.FromResult(0);
        }
    }
}
