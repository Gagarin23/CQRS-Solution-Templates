using Application.Example.Notifications;
using Application.Example.Queries;
using Application.Example.Streams;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Api.Controllers
{
    public class ExampleController : ApiController
    {
        private readonly IDatabaseContext _context;

        public ExampleController(IDatabaseContext context)
        {
            _context = context;
        }
        
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
        public IActionResult Post([FromBody] ExampleNotification notify)
        {
            Mediator.Publish(notify);

            return Ok();
        }

        [HttpPost("test")]
        public IActionResult test()
        {
            _context.Database.CanConnect();

            return Ok();
        }
    }
}
