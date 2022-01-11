using Application.Example.Notifications;
using Application.Example.Queries;
using Application.Example.Streams;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class ExampleController : ApiController
    {
        [HttpPost("ok")]
        public async Task<IActionResult> GetOkMessage(OkQuery request)
        {
            var dto = await Mediator.Send(request);

            return Ok(dto);
        }

        [HttpPost("stream")]
        public IAsyncEnumerable<int> GetIntStream(StreamQuery request)
        {
            var stream = Mediator.CreateStream(request);

            return stream;
        }

        [HttpPost("publish")]
        public IActionResult Post(ExampleNotification notify)
        {
            Mediator.Publish(notify);

            return Ok();
        }
    }
}
