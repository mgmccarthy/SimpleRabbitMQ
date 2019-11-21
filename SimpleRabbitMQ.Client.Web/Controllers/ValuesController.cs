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
            var options = new SendOptions();
            options.RequireImmediateDispatch();
            await WebApiApplication.Endpoint.Send(new TestCommand(), options).ConfigureAwait(false);
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
