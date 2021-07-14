using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Celin
{
    public class Submit
    {
        E1Service E1 { get; }
        ParserService Parser { get; }

        [Function("Submit")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
                FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Function1");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            string msg;
            try
            {
                var data = JsonSerializer.Deserialize<JsonElement>(requestBody);
                if (data.TryGetProperty("code", out var code))
                {
                    var rq = Parser.Parse(code.GetString());

                    var rs = await E1.RequestAsync<JsonElement>(rq);
                    var qr = new QueryResponse(rq, rs);

                    var response = req.CreateResponse(HttpStatusCode.OK);

                    await response.WriteAsJsonAsync(qr);

                    return response;
                }
                msg = "Missing 'code' in body!";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            var err = req.CreateResponse(HttpStatusCode.BadRequest);
            await err.WriteAsJsonAsync(new
            {
                Error = msg
            }); ;

            return err;
        }
        public Submit(E1Service e1, ParserService parser)
        {
            E1 = e1;
            Parser = parser;
        }
    }
}
