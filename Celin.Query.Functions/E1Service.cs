using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Celin
{
    public class E1Service : AIS.Server
    {
        public E1Service(IConfiguration config, ILogger<E1Service> log, IHttpClientFactory httpClientFactory)
            : base(config["BaseUrl"], log, httpClientFactory.CreateClient())
        {
            AuthRequest.username = config["User"];
            AuthRequest.password = config["Password"];
        }
    }
}
