using Application.Example.Notifications;
using Application.Example.Queries;
using Application.Example.Streams;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Api.Controllers
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
        public async Task<IActionResult> GetOkMessage(OkQuery request)
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
        public IActionResult Publish(ExampleNotification notify)
        {
            Mediator.Publish(notify);

            return Ok();
        }
    }
}
