using System;
using System.Collections.Generic;

namespace Celin
{
    public class QueryResponse
    {
        public string Title { get; set; }
        public IDictionary<string, string> Columns { get; set; }
        public string Environment { get; set; }
        public DateTime Submitted { get; set; }
        public AIS.Summary Summary { get; set; }
        public IEnumerable<object> Data { get; set; }
        public bool Demo { get; set; }
        public string Error { get; set; }
    }
}
