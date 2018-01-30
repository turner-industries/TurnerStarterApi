using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;
using TurnerStarterApi.Core.Data;
using TurnerStarterApi.Core.Extensions;

namespace TurnerStarterApi.Core.Features.Common
{
    [DoNotValidate]
    public class GetByIdRequest<TEntity, TDto> : IRequest<TDto>
    {
        public int Id { get; set; }

        public Expression<Func<TEntity, bool>> Filter { get; set; }
    }

    public class GetByIdRequestHandler<TEntity, TDto> 
        : IRequestHandler<GetByIdRequest<TEntity, TDto>, TDto> where TEntity : class, IEntity
    {
        private readonly DbContext _context;
        private readonly ProjectionParameters _projectionParameters;

        public GetByIdRequestHandler(DbContext context, ProjectionParameters projectionParameters)
        {
            _context = context;
            _projectionParameters = projectionParameters;
        }

        public async Task<Response<TDto>> HandleAsync(GetByIdRequest<TEntity, TDto> request)
        {
            var filteredResult = _context.Set<TEntity>().AsQueryable();

            if (request.Filter != null)
            {
                filteredResult = filteredResult.Where(request.Filter);
            }

            var result = await filteredResult
                .Where(x => x.Id == request.Id && !x.IsDeleted)
                .ProjectTo<TDto>(_projectionParameters)
                .SingleOrDefaultAsync();

            if (result == null)
            {
                return request.HasError($"{typeof(TEntity).Name.SplitWords()} " +
                                        $"with ID {request.Id} not found.", nameof(IEntity.Id));
            }

            return result.AsResponse();
        }
    }
}
