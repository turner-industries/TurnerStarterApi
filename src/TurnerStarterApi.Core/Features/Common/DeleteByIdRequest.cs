using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;
using TurnerStarterApi.Core.Data;
using TurnerStarterApi.Core.Extensions;
using TurnerStarterApi.Core.Features.Security;

namespace TurnerStarterApi.Core.Features.Common
{
    [DoNotValidate]
    public class DeleteByIdRequest<TEntity> : IRequest
    {
        public int Id { get; set; }

        public Expression<Func<TEntity, bool>> Filter { get; set; }

        public Dictionary<Type, Func<TEntity, int>> DependenciesToDelete { get; set; }
    }

    public class DeleteByIdRequestHandler<TEntity> 
        : IRequestHandler<DeleteByIdRequest<TEntity>> where TEntity : class, IEntity
    {
        private readonly DbContext _context;
        private readonly IIdentityContext _identityContext;

        public DeleteByIdRequestHandler(DbContext context, IIdentityContext identityContext)
        {
            _context = context;
            _identityContext = identityContext;
        }

        public async Task<Response> HandleAsync(DeleteByIdRequest<TEntity> request)
        {
            var filteredResult = _context.Set<TEntity>()
                .Where(x => !x.IsDeleted)
                .AsQueryable();

            if (request.Filter != null)
            {
                filteredResult = filteredResult.Where(request.Filter);
            }

            var entity = await filteredResult.SingleOrDefaultAsync(x => x.Id == request.Id);
            if (entity == null)
            {
                return request.HasError($"{typeof(TEntity).Name.SplitWords()} " +
                                        $"with ID {request.Id} not found.", nameof(IEntity.Id));
            }

            if (request.DependenciesToDelete != null)
            {
                foreach (var dependency in request.DependenciesToDelete)
                {
                    var dependentEntity = _context.Find(dependency.Key, dependency.Value(entity));
                    _context.Remove(dependentEntity);
                }
            }

            _context.Remove(entity);

            _context.SaveChanges();

            return new Response();
        }
    }
}
