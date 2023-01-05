using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public abstract class ApiController : ControllerBase
    {
        protected readonly IMediator Mediator;

        protected ApiController(IMediator mediator)
        {
            Mediator = mediator;
        }
    }
}
