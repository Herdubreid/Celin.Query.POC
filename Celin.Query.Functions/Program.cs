using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Celin.Query.Functions
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(service =>
                {
                    service.AddHttpClient();
                    service.AddSingleton<E1Service>();
                    service.AddSingleton<ParserService>();
                })
                .Build();

            host.Run();
        }
    }
}