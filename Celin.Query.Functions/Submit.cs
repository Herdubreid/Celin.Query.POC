using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Celin
{
    public class Submit
    {
        E1Service E1 { get; }
        ParserService Parser { get; }
        QueryResponse ParseResult(AIS.DatabrowserRequest rq, JsonElement result)
        {
            var it = result.EnumerateObject();
            var rsp = new QueryResponse
            {
                Demo = !string.IsNullOrEmpty(rq.formServiceDemo)
            };
            if (it.MoveNext())
            {
                var n = it.Current.Name;
                if (n.StartsWith("fs_"))
                {
                    rsp.Title = it.Current.Value.GetProperty("title").GetString();
                    var fm = JsonSerializer.Deserialize<AIS.Form<AIS.FormData<JsonElement>>>(it.Current.Value.ToString());
                    rsp.Summary = fm.data.gridData.summary;
                    if (rsp.Summary.records > 0)
                    {
                        if (rsp.Demo)
                        {
                            rsp.Columns = new Dictionary<string, string>
                            {
                                { "0", "Title" },
                                { "1", "Table" },
                                { "2", "Alias" },
                                { "3", "Type" }
                            };
                            rsp.Data = fm.data.gridData.columns.Select(c =>
                            {
                                var m = Regex.Match(c.Key, "([^_]*)_?(.+)");
                                var t = fm.data.gridData.rowset[0].GetProperty(c.Key);
                                return new[]
                                {
                                    c.Value,
                                    m.Groups[1].Value,
                                    m.Groups[2].Value,
                                    t.ValueKind == JsonValueKind.Number ? t.GetInt32() as object : t.GetString()
                                };
                            });
                        }
                        else
                        {

                            var alias = fm.data.gridData.columns
                                .Select(c =>
                                {
                                    var m = Regex.Match(c.Key, "[^_]*_?(.+)");
                                    if (m.Success)
                                    {
                                        return (m.Groups[1].Value, c.Key, c.Value);
                                    }
                                    return (c.Key, c.Key, c.Value);
                                });
                            if (string.IsNullOrWhiteSpace(rq.returnControlIDs))
                            {
                                rsp.Columns = fm.data.gridData.columns;
                            }
                            else
                            {
                                rsp.Columns = new Dictionary<string, string>(rq.returnControlIDs.Split('|', StringSplitOptions.TrimEntries)
                                    .Select(c =>
                                    {
                                        foreach (var al in alias)
                                        {
                                            if (al.Item1.Equals(c) || al.Item2.Equals(c))
                                            {
                                                return new(al.Item2, al.Item3);
                                            }
                                        }
                                        return new KeyValuePair<string, string>(string.Empty, string.Empty);
                                    })
                                    .Where(c => !string.IsNullOrEmpty(c.Key)));
                            }
                            rsp.Data = fm.data.gridData.rowset.Select(json =>
                            {
                                return rsp.Columns.Select(c =>
                                {
                                    var p = json.GetProperty(c.Key);
                                    switch (p.ValueKind)
                                    {
                                        case JsonValueKind.Number:
                                            if (p.TryGetInt32(out var i)) return i;
                                            else return p.GetDecimal();
                                        case JsonValueKind.String:
                                            var m = Regex.Match(p.GetString(), "((?:19|20)[0-9][0-9])((?:0[1-9]|1[0-2]))((?:0[1-9]|[1-2][0-9]|3[0-1]))");
                                            if (m.Success)
                                            {
                                                return $"{int.Parse(m.Groups[1].Value)}-{int.Parse(m.Groups[2].Value)}-{int.Parse(m.Groups[3].Value)}";
                                            }
                                            return p.GetString() as object;
                                        default:
                                            return string.Empty;
                                    }
                                });
                            });
                        }
                    }
                }
                else if (n.StartsWith("ds_"))
                {
                    rsp.Title = n;
                    var ds = JsonSerializer.Deserialize<AIS.Output<JsonElement>>(it.Current.Value.ToString());
                    if (ds.error != null)
                    {
                        rsp.Error = ds.error.message;
                    }
                    else
                    {
                        rsp.Summary = new AIS.Summary
                        {
                            records = ds.output.Length,
                            moreRecords = false
                        };
                        if (rsp.Summary.records > 0)
                        {
                            rsp.Columns = ds.output[0].EnumerateObject()
                                .Aggregate(new Dictionary<string, string>(), (ag, c) =>
                                 {
                                     if (c.Value.ValueKind == JsonValueKind.Object)
                                     {
                                         foreach (var sc in c.Value.EnumerateObject())
                                         {
                                             ag.Add($"{c.Name}.{sc.Name}", string.Empty);
                                         }
                                     }
                                     else
                                     {
                                         ag.Add(c.Name, string.Empty);
                                     }
                                     return ag;
                                 });
                            int i = 0;
                            rsp.Data = ds.output.Select(r =>
                            {
                                var row = r.EnumerateObject()
                                .Aggregate(Enumerable.Empty<object>(), (ag, c) =>
                                {
                                    switch (c.Value.ValueKind)
                                    {
                                        case JsonValueKind.Object:
                                            if (c.Name.Equals("groupBy"))
                                            {
                                                foreach (var g in c.Value.EnumerateObject())
                                                {
                                                    if (g.Value.ValueKind == JsonValueKind.Number)
                                                    {
                                                        if (g.Value.TryGetInt32(out i)) ag = ag.Append(i);
                                                        else ag = ag.Append(c.Value.GetDecimal());
                                                    }
                                                    else
                                                    {
                                                        ag = ag.Append(g.Value.GetString());
                                                    }
                                                }
                                            }
                                            else if (c.Value.TryGetProperty("internalValue", out var value))
                                            {
                                                if (value.ValueKind == JsonValueKind.String)
                                                {
                                                    ag = ag.Append(c.Value.GetString());
                                                }
                                                else
                                                {
                                                    if (c.Value.TryGetInt32(out i)) ag = ag.Append(i);
                                                    else ag = ag.Append(c.Value.GetDecimal());
                                                }
                                            }
                                            break;
                                        case JsonValueKind.Number:
                                            if (c.Value.TryGetInt32(out i)) return ag.Append(i);
                                            else return ag.Append(c.Value.GetDecimal());
                                        case JsonValueKind.String:
                                            return ag.Append(c.Value.GetString());
                                        default:
                                            return ag.Append(string.Empty);
                                    }
                                    return ag;
                                });
                                return row;
                            });
                        }
                    }
                }
                else if (n.CompareTo("sysErrors") == 0)
                {
                    var errs = JsonSerializer.Deserialize<IEnumerable<Celin.AIS.ErrorWarning>>(it.Current.Value.ToString());
                    foreach (var e in errs)
                    {
                        rsp.Error = e.DESC;
                    }
                }
            }
            else
            {
                rsp.Error = "Empty result!";
            }

            return rsp;
        }
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

                    var response = req.CreateResponse(HttpStatusCode.OK);

                    await response.WriteAsJsonAsync(ParseResult(rq, rs));

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
