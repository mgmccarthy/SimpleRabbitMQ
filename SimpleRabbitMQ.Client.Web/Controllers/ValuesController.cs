using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using NServiceBus;
using SimpleRabbitMQ.Messages;

namespace SimpleRabbitMQ.Client.Web.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public async Task<IHttpActionResult> Post([FromBody]string value)
        {
            //commenting out the RequireImmediateDispatch had a significant impact on now many req per second went through both WebAPI (via West Wind Web Surge)
            //and the amount of messages per second both ep's could process. About 23 req per sec vs. 208 req per sec. Significant
            //var options = new SendOptions();
            //options.RequireImmediateDispatch();
            //await WebApiApplication.Endpoint.Send(new TestCommand(), options).ConfigureAwait(false);
            await WebApiApplication.Endpoint.Send(new TestCommand { OrderId = Guid.NewGuid() });
            return Ok();
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}