using Api.Filters;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(SurrogateProblemDetailsResponseTypeForSwaggerDoc))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(UnauthorizedResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundResult))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(SurrogateProblemDetailsResponseTypeForSwaggerDoc))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(SurrogateProblemDetailsResponseTypeForSwaggerDoc))]
    public abstract class ApiController : ControllerBase
    {
        protected readonly IMediator Mediator;

        protected ApiController(IMediator mediator)
        {
            Mediator = mediator;
        }
    }
}
