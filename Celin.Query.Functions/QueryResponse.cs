using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Celin
{
    public class QueryResponse
    {
        public string Title { get; set; }
        public IDictionary<string, string> Columns { get; set; }
        public string Environment { get; set; }
        public DateTime Submitted { get; set; }
        public AIS.Summary Summary { get; set; } = new AIS.Summary();
        public List<object> Data { get; set; } = new List<object>();
        public bool Demo { get; set; }
        public string Error { get; set; }
        readonly Regex ALIAS = new Regex("[^_]*_?(.+)");
        readonly Regex DATE = new Regex("((?:19|20)[0-9][0-9])((?:0[1-9]|1[0-2]))((?:0[1-9]|[1-2][0-9]|3[0-1]))");
        readonly string FS = "fs_";
        readonly string DS = "ds_";
        readonly string ERR = "sysErrors";
        readonly string GB = "groupBy";
        public QueryResponse(AIS.DatabrowserRequest rq, JsonElement result)
        {
            Demo = !(string.IsNullOrEmpty(rq.formServiceDemo) || rq.batchDataRequest.HasValue);

            #region ParseColumns

            var cols = new List<(string, string)>();

            var rqIt = rq.batchDataRequest.HasValue
                ? rq.dataRequests.GetEnumerator()
                : null;
            var rsIt = result.EnumerateObject();
            while (rsIt.MoveNext())
            {
                var rqNext = rq.batchDataRequest.HasValue && rqIt.MoveNext()
                    ? rqIt.Current
                    : rq;

                var n = rsIt.Current.Name;
                if (n.StartsWith(FS))
                {
                    var fm = JsonSerializer.Deserialize<AIS.Form<AIS.FormData<JsonElement>>>(rsIt.Current.Value.ToString());
                    Summary.records += fm.data.gridData.summary.records;
                    Summary.moreRecords = Summary.moreRecords || fm.data.gridData.summary.moreRecords;
                    if (Demo)
                    {
                        Columns = new Dictionary<string, string>
                            {
                                { "0", "Title" },
                                { "1", "Table" },
                                { "2", "Alias" },
                                { "3", "Type" }
                            };
                        Data.AddRange(fm.data.gridData.columns.Select(c =>
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
                        }));
                        Summary.records = Data.Count;
                        return;
                    }
                    else
                    {
                        var alias = fm.data.gridData.columns
                            .Select(c =>
                            {
                                var m = ALIAS.Match(c.Key);
                                if (m.Success)
                                {
                                    return (m.Groups[1].Value, c.Key, c.Value);
                                }
                                return (c.Key, c.Key, c.Value);
                            });
                        if (string.IsNullOrWhiteSpace(rqNext.returnControlIDs))
                        {
                            cols.AddRange(alias.Select(c => (c.Item1, c.Item3)));
                        }
                        else
                        {
                            cols.AddRange((rqNext.returnControlIDs.Split('|', StringSplitOptions.TrimEntries)
                                .Select(c =>
                                {
                                    foreach (var al in alias)
                                    {
                                        if (al.Item1.Equals(c) || al.Item2.Equals(c))
                                        {
                                            return (al.Item1, al.Item3);
                                        }
                                    }
                                    return (string.Empty, string.Empty);
                                })
                                .Where(c => !string.IsNullOrEmpty(c.Item1))));
                        }
                    }
                }
                else if (n.StartsWith(DS))
                {
                    var ds = JsonSerializer.Deserialize<AIS.Output<JsonElement>>(rsIt.Current.Value.ToString());
                    if (ds.error != null)
                    {
                        Error = ds.error.message;
                    }
                    else
                    {
                        Summary.records += ds.output.Length;
                        if (ds.output.Length > 0)
                        {
                            cols = ds.output[0].EnumerateObject()
                                .Aggregate(cols, (ag, c) =>
                                {
                                    if (c.Value.ValueKind == JsonValueKind.Object)
                                    {
                                        foreach (var sc in c.Value.EnumerateObject())
                                        {
                                            ag.Add(($"{c.Name}.{sc.Name}", string.Empty));
                                        }
                                    }
                                    else
                                    {
                                        ag.Add((c.Name, string.Empty));
                                    }
                                    return ag;
                                });
                        }
                    }
                }
                else if (n.CompareTo(ERR) == 0)
                {
                    var errs = JsonSerializer.Deserialize<IEnumerable<Celin.AIS.ErrorWarning>>(rsIt.Current.Value.ToString());
                    foreach (var e in errs)
                    {
                        Error = e.DESC;
                    }
                }

                if (!rq.batchDataRequest.HasValue)
                {
                    break;
                }
            }
            Columns = cols.Distinct().ToDictionary(r => r.Item1, r => r.Item2);
            #endregion
            #region ParseData
            rqIt = rq.batchDataRequest.HasValue
                ? rq.dataRequests.GetEnumerator()
                : null;
            rsIt = result.EnumerateObject();
            while (rsIt.MoveNext())
            {
                var rqNext = rq.batchDataRequest.HasValue && rqIt.MoveNext()
                    ? rqIt.Current
                    : rq;

                var n = rsIt.Current.Name;
                if (n.StartsWith(FS))
                {
                    var fm = JsonSerializer.Deserialize<AIS.Form<AIS.FormData<JsonElement>>>(rsIt.Current.Value.ToString());
                    if (fm.data.gridData.summary.records > 0)
                    {
                        Data.AddRange(fm.data.gridData.rowset.Select(json =>
                        {
                            return Columns.Select(c =>
                            {
                                if (json.TryGetProperty($"{rqNext.targetName}_{c.Key}", out var p))
                                {
                                    switch (p.ValueKind)
                                    {
                                        case JsonValueKind.Number:
                                            if (p.TryGetInt32(out var i)) return i;
                                            else return p.GetDecimal();
                                        case JsonValueKind.String:
                                            var m = DATE.Match(p.GetString());
                                            if (m.Success)
                                            {
                                                return $"{int.Parse(m.Groups[1].Value)}-{int.Parse(m.Groups[2].Value)}-{int.Parse(m.Groups[3].Value)}";
                                            }
                                            return p.GetString() as object;
                                        default:
                                            return string.Empty;
                                    }
                                }
                                return string.Empty;
                            });
                        }));
                    }
                    if (!rq.batchDataRequest.HasValue)
                    {
                        break;
                    }
                }
                else if (n.StartsWith(DS))
                {
                    var ds = JsonSerializer.Deserialize<AIS.Output<JsonElement>>(rsIt.Current.Value.ToString());
                    if (ds.error != null)
                    {
                        Error = ds.error.message;
                    }
                    else
                    {
                        Summary = new AIS.Summary
                        {
                            records = ds.output.Length,
                            moreRecords = false
                        };
                        if (Summary.records > 0)
                        {
                            int i = 0;
                            Data.AddRange(ds.output.Select(r =>
                            {
                                return Columns.Aggregate(Enumerable.Empty<object>(), (ag, col) =>
                                {
                                    bool found = r.TryGetProperty(col.Key, out var c);
                                    if (!found && col.Key.StartsWith(GB))
                                    {
                                        var subKey = col.Key.Split('.')[1];
                                        if (r.TryGetProperty(GB, out var g))
                                        {
                                            found = g.TryGetProperty(subKey, out c);
                                        }
                                    }
                                    if (!found)
                                    {
                                        return ag.Append(null);
                                    }
                                    switch (c.ValueKind)
                                    {
                                        case JsonValueKind.Object:
                                            if (c.TryGetProperty("internalValue", out var value))
                                            {
                                                if (value.ValueKind == JsonValueKind.String)
                                                {
                                                    ag = ag.Append(c.GetString());
                                                }
                                                else
                                                {
                                                    if (c.TryGetInt32(out i)) ag = ag.Append(i);
                                                    else ag = ag.Append(c.GetDecimal());
                                                }
                                            }
                                            break;
                                        case JsonValueKind.Number:
                                            if (c.TryGetInt32(out i)) return ag.Append(i);
                                            else return ag.Append(c.GetDecimal());
                                        case JsonValueKind.String:
                                            return ag.Append(c.GetString());
                                        default:
                                            return ag.Append(string.Empty);
                                    }
                                    return ag;
                                });
                            }));
                        }
                    }
                }
                else if (n.CompareTo(ERR) == 0)
                {
                    var errs = JsonSerializer.Deserialize<IEnumerable<Celin.AIS.ErrorWarning>>(rsIt.Current.Value.ToString());
                    foreach (var e in errs)
                    {
                        Error = e.DESC;
                    }
                }
                #endregion
            }
        }
    }
}
