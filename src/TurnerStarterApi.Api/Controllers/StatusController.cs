using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Turner.Infrastructure.Mediator;
using TurnerStarterApi.Core.Features.Status;

namespace TurnerStarterApi.Api.Controllers
{
    [Route("api/v1/status")]
    public class StatusController : BaseApiController
    {
        [HttpGet]
        [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
        public Task<IActionResult> GetStatusAsync()
        {
            return HandleAsync(new GetStatusRequest());
        }
    }
}
