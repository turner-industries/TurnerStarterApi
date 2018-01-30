using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Turner.Infrastructure.Mediator;
using TurnerStarterApi.Core.Features.Users;

namespace TurnerStarterApi.Api.Controllers
{
    public class BaseApiController : Controller
    {
        public User CurrentUser { get; set; }

        public IMediator Mediator { get; set; }

        protected async Task<IActionResult> HandleAsync(IRequest request,
            HttpStatusCode successStatusCode = HttpStatusCode.OK)
        {
            var response = await Mediator.HandleAsync(request);
            return HandleResponse(response, successStatusCode);
        }

        protected async Task<IActionResult> HandleAsync<TResult>(IRequest<TResult> request,
            HttpStatusCode successStatusCode = HttpStatusCode.OK)
        {
            var response = await Mediator.HandleAsync(request);
            return HandleResponse(response, successStatusCode);
        }

        protected IActionResult HandleResponse(Response response, HttpStatusCode successStatusCode)
        {
            return response.HasErrors ? BadRequest(response) : StatusCode((int)successStatusCode, response);
        }
    }
}
