using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;
using TurnerStarterApi.Core.Data;

namespace TurnerStarterApi.Core.Features.Common
{
    [DoNotValidate]
    public class GetAllAdvancedRequest<TEntity, TDto> : IRequest<AdvancedQueryPage<TDto>>
    {
        public AdvancedQueryRequest Request { get; set; }

        public Expression<Func<TEntity, bool>> Filter { get; set; }
    }

    public class GetAllAdvancedRequestHandler<TEntity, TDto> 
        : IRequestHandler<GetAllAdvancedRequest<TEntity, TDto>, AdvancedQueryPage<TDto>> 
            where TEntity : class, IEntity
            where TDto : class
    {
        private readonly DbContext _context;
        private readonly ProjectionParameters _projectionParameters;

        public GetAllAdvancedRequestHandler(
            DbContext context,
            ProjectionParameters projectionParameters
        )
        {
            _context = context;
            _projectionParameters = projectionParameters;
        }

        public async Task<Response<AdvancedQueryPage<TDto>>> HandleAsync(GetAllAdvancedRequest<TEntity, TDto> query)
        {
            var filteredResult = _context.Set<TEntity>()
                .AsQueryable()
                .Where(x => !x.IsDeleted);

            if (query.Filter != null)
            {
                filteredResult = filteredResult
                    .Where(query.Filter);
            }

            var result = await filteredResult
                .ProjectTo<TDto>(_projectionParameters)
                .PageAsync(query.Request);

            return result.AsResponse();
        }
    }

    public static class AdvancedQueryParserExtensions
    {
        public static Task<AdvancedQueryPage<T>> PageAsync<T>(this IQueryable<T> query, AdvancedQueryRequest request) where T : class
        {
            return new AdvancedQueryParser<T>().ParseAsync(query, request);
        }
    }

    public class AdvancedQueryParser<T> where T : class
    {
        private readonly IDictionary<string, PropertyMap> _propertyMap;
        private readonly Type _type;

        private readonly Dictionary<string, Expression> _converters = new Dictionary<string, Expression>();
        private readonly Type[] _convertable =
        {
            typeof(int),
            typeof(int?),
            typeof(decimal),
            typeof(decimal?),
            typeof(float),
            typeof(float?),
            typeof(double),
            typeof(double?),
            typeof(DateTime),
            typeof(DateTime?)
        };

        public AdvancedQueryParser()
        {
            _type = typeof(T);
            _propertyMap = (from prop in _type.GetProperties()
                           select new
                           {
                               name = prop.Name.ToLower(),
                               map = new PropertyMap
                               {
                                   Property = prop,
                                   Searchable = true,
                                   Orderable = true
                               }
                           }).ToDictionary(k => k.name, v => v.map);

            if (_propertyMap.Count == 0)
            {
                throw new Exception("No properties were found in request. Please map datatable field names to properties in T");
            }
        }

        public async Task<AdvancedQueryPage<T>> ParseAsync(IQueryable<T> queryable, AdvancedQueryRequest request)
        {
            var page = new AdvancedQueryPage<T>();

            var filteredQuery = ApplySort(queryable, request)
                .Where(GenerateEntityFilter(request));

            var count = await filteredQuery.CountAsync();

            if (request.PageSize == 0)
            {
                page.PageCount = 1;
                page.Data = filteredQuery.ToList();
                return page;
            }

            page.PageCount = (int)Math.Ceiling((double)count / request.PageSize);

            page.Data = filteredQuery
                .Skip(request.PageSize * request.Page)
                .Take(request.PageSize)
                .ToList();

            return page;
        }

        public AdvancedQueryParser<T> SetConverter(Expression<Func<T, object>> property, Expression<Func<T, string>> tostring)
        {
            var memberExp = ((UnaryExpression)property.Body).Operand as MemberExpression;

            if (memberExp == null)
            {
                throw new ArgumentException("Body in property must be a member expression");
            }

            _converters[memberExp.Member.Name] = tostring.Body;

            return this;
        }

        private IQueryable<T> ApplySort(IQueryable<T> queryable, AdvancedQueryRequest request)
        {
            var sorted = false;
            var paramExpr = Expression.Parameter(_type, "val");

            foreach (var param in request.Sorted)
            {
                if (!_propertyMap.ContainsKey(param.Id.ToLower()) || !_propertyMap[param.Id.ToLower()].Orderable)
                {
                    continue;
                }

                var sortProperty = _propertyMap[param.Id.ToLower()].Property;
                var expression1 = Expression.Property(paramExpr, sortProperty);
                var propType = sortProperty.PropertyType;
                var delegateType = Expression.GetFuncType(_type, propType);
                var propertyExpr = Expression.Lambda(delegateType, expression1, paramExpr);

                string methodName;
                if (!param.Desc)
                {
                    methodName = sorted ? "ThenBy" : "OrderBy";
                }
                else
                {
                    methodName = sorted ? "ThenByDescending" : "OrderByDescending";
                }

                queryable = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                                && method.IsGenericMethodDefinition
                                && method.GetGenericArguments().Length == 2
                                && method.GetParameters().Length == 2)
                        .MakeGenericMethod(_type, propType)
                        .Invoke(null, new object[] { queryable, propertyExpr }) as IOrderedQueryable<T>;

                sorted = true;
            }

            //Linq to entities needs a sort to implement skip
            if (!sorted)
            {
                var firstProp = Expression.Property(paramExpr, _propertyMap.First().Value.Property);
                var propType = _propertyMap.First().Value.Property.PropertyType;
                var delegateType = Expression.GetFuncType(_type, propType);
                var propertyExpr = Expression.Lambda(delegateType, firstProp, paramExpr);

                queryable = typeof(Queryable).GetMethods().Single(method =>
                         method.Name == "OrderBy"
                         && method.IsGenericMethodDefinition
                         && method.GetGenericArguments().Length == 2
                         && method.GetParameters().Length == 2)
                 .MakeGenericMethod(_type, propType)
                 .Invoke(null, new object[] { queryable, propertyExpr }) as IOrderedQueryable<T>;
            }

            return queryable;
        }

        private Expression<Func<T, bool>> GenerateEntityFilter(AdvancedQueryRequest request)
        {
            var paramExpression = Expression.Parameter(_type, "val");
            List<MethodCallExpression> searchProps = new List<MethodCallExpression>();
            var modifier = new ModifyParam(paramExpression);

            foreach (var filtered in request.Filtered)
            {
                if (!_propertyMap.ContainsKey(filtered.Id.ToLower()) || !_propertyMap[filtered.Id.ToLower()].Orderable)
                {
                    continue;
                }

                var propMap = _propertyMap[filtered.Id.ToLower()];
                var prop = propMap.Property;

                var isString = prop.PropertyType == typeof(string);
                var hasCustom = _converters.ContainsKey(prop.Name);

                if ((!prop.CanWrite || !propMap.Searchable || (_convertable.All(t => t != prop.PropertyType) && !isString)) && !hasCustom)
                {
                    continue;
                }

                Expression propExp = Expression.Property(paramExpression, prop);

                if (hasCustom)
                {
                    propExp = modifier.Visit(_converters[prop.Name]);
                }
                else if (!isString)
                {
                    var toString = prop.PropertyType.GetMethod("ToString", Type.EmptyTypes);

                    propExp = Expression.Call(propExp, toString);
                }

                var toLower = Expression.Call(propExp, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
                var searchExpression = Expression.Constant(filtered.Value.ToLower());
                searchProps.Add(Expression.Call(toLower, typeof(string).GetMethod("Contains"), searchExpression));
            }

            var propertyQuery = searchProps.ToArray();

            if (propertyQuery.Length == 0)
            {
                return x => true;
            }

            Expression compoundExpression = propertyQuery[0];

            for (int i = 1; i < propertyQuery.Length; i++)
            {
                compoundExpression = Expression.Or(compoundExpression, propertyQuery[i]);
            }

            return Expression.Lambda<Func<T, bool>>(compoundExpression, paramExpression);
        }

        public class ModifyParam : ExpressionVisitor
        {
            private readonly ParameterExpression _replace;

            public ModifyParam(ParameterExpression p)
            {
                _replace = p;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _replace;
            }
        }

        private class PropertyMap
        {
            public PropertyInfo Property { get; set; }
            public bool Orderable { get; set; }
            public bool Searchable { get; set; }
        }
    }

    public class AdvancedQueryRequest
    {
        public int Page { get; set; }

        public int PageSize { get; set; } = 50;

        public List<AdvancedQueryColumnSearch> Filtered { get; set; } = new List<AdvancedQueryColumnSearch>();

        public List<AdvancedQueryColumnSort> Sorted { get; set; } = new List<AdvancedQueryColumnSort>();
    }

    public class AdvancedQueryColumnSearch
    {
        public string Id { get; set; }

        public string Value { get; set; }
    }

    public class AdvancedQueryColumnSort
    {
        public string Id { get; set; }

        public bool Desc { get; set; }
    }

    public class AdvancedQueryPage<T>
    {
        public int PageCount { get; set; }

        public List<T> Data { get; set; }
    }
}
