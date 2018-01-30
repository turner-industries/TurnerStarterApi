using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using Turner.Infrastructure.Mediator;
using TurnerStarterApi.Api.Controllers;
using TurnerStarterApi.Core.Features.Security;

namespace TurnerStarterApi.Api.Security
{
    public class AuthorizeUserAttribute : TypeFilterAttribute
    {
        public AuthorizeUserAttribute() : base(typeof(AuthorizeUserFilter))
        {
            Arguments = new object[] { new string[0] };
        }
    }

    public class AuthorizeUserFilter : IActionFilter
    {
        private readonly string[] _authorizedRoles;

        public AuthorizeUserFilter(string[] authorizedRoles)
        {
            _authorizedRoles = authorizedRoles;
        }

        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            var container = context.HttpContext.RequestServices.GetService<Container>();
            var identityContext = container.GetInstance<IIdentityContext>();

            var user = identityContext.RequestingUser;
            if (user == null)
            {
                SetResult(context, "Unauthorized", 401);
                return;
            }

            if (context.Controller is BaseApiController apiController)
            {
                apiController.CurrentUser = user;
                apiController.Mediator = container.GetInstance<IMediator>();
            }

            if (!_authorizedRoles.Any())
            {
                return;
            }

            if (!_authorizedRoles.Contains(user.Role))
            {
                SetResult(context, "You do not have permission to perform this action.", 403);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        private static void SetResult(ActionExecutingContext context, string errorMessage, int? statusCode)
        {
            var response = new Response();
            response.Errors.Add(new Error
            {
                ErrorMessage = errorMessage
            });

            context.Result = new ObjectResult(response)
            {
                StatusCode = statusCode
            };
        }
    }
}
