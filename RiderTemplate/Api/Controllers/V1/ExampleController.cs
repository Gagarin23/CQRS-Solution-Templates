using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Features.V1.Example.Notifications;
using Application.Features.V1.Example.Queries;
using Application.Features.V1.Example.Streams;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1
{
    /// <summary>
    /// My api
    /// </summary>
    public class ExampleController : ApiController
    {
        /// <inheritdoc />
        public ExampleController(IMediator mediator)
            : base(mediator) { }

        /// <summary>
        /// Send and get message
        /// </summary>
        /// <param name="request">Message</param>
        /// <returns>Message</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        public async Task<IActionResult> GetOkMessageAsync(OkQuery request)
        {
            var dto = await Mediator.Send(request);

            return Ok(dto);
        }

        /// <summary>
        /// Get stream
        /// </summary>
        /// <param name="request">Number of iterations of the cycle</param>
        /// <returns>Stream</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IAsyncEnumerable<int>))]
        public IAsyncEnumerable<int> GetIntStream(StreamQuery request)
        {
            var stream = Mediator.CreateStream(request);

            return stream;
        }

        /// <summary>
        /// Publish notify
        /// </summary>
        /// <param name="notify">Empty command</param>
        /// <returns></returns>
        /// <response code="200">Always ok. Publishing asynchronous</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PublishAsync(ExampleNotification notify)
        {
            await Mediator.Publish(notify);

            return Ok();
        }
    }
}
