using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;
using TurnerStarterApi.Core.Data;

namespace TurnerStarterApi.Core.Features.Common
{
    [DoNotValidate]
    public class GetAllRequest<TEntity, TDto> : IRequest<List<TDto>>
    {
        public Func<TDto, object> SortBy { get; set; }

        public Func<TDto, object> SortByDescending { get; set; }

        public Expression<Func<TEntity, bool>> Filter { get; set; }
    }

    public class GetAllRequestHandler<TEntity, TDto> 
        : IRequestHandler<GetAllRequest<TEntity, TDto>, List<TDto>> where TEntity : class, IEntity
    {
        private readonly DbContext _context;
        private readonly ProjectionParameters _projectionParameters;

        public GetAllRequestHandler(
            DbContext context,
            ProjectionParameters projectionParameters
        )
        {
            _context = context;
            _projectionParameters = projectionParameters;
        }

        public Task<Response<List<TDto>>> HandleAsync(GetAllRequest<TEntity, TDto> query)
        {
            var filteredResult = _context.Set<TEntity>()
                .AsQueryable()
                .Where(x => !x.IsDeleted);

            if (query.Filter != null)
            {
                filteredResult = filteredResult
                    .Where(query.Filter);
            }

            if (query.SortBy != null)
            {
                return filteredResult
                    .ProjectTo<TDto>(_projectionParameters)
                    .AsEnumerable()
                    .OrderBy(query.SortBy)
                    .ToList()
                    .AsResponseAsync();
            }

            if (query.SortByDescending != null)
            {
                return filteredResult
                    .ProjectTo<TDto>(_projectionParameters)
                    .AsEnumerable()
                    .OrderByDescending(query.SortByDescending)
                    .ToList()
                    .AsResponseAsync();
            }

            return filteredResult
                .ProjectTo<TDto>(_projectionParameters)
                .ToList()
                .AsResponseAsync();
        }
    }
}