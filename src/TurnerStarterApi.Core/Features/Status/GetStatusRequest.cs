using System.Threading.Tasks;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;

namespace TurnerStarterApi.Core.Features.Status
{
    [DoNotValidate]
    public class GetStatusRequest : IRequest<string>
    {
    }

    public class GetStatusRequestHandler : IRequestHandler<GetStatusRequest, string>
    {
        public Task<Response<string>> HandleAsync(GetStatusRequest request)
        {
            return "We're up and running!".AsResponseAsync();
        }
    }
}
