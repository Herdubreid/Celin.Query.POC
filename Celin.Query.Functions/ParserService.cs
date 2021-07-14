using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;
using System.Text.Json;

namespace Celin
{
    public class ParserService
    {
        public AIS.DatabrowserRequest Parse(string qry) =>
            Try(AIS.Data.CombinedFileRequest.Parser).Or(AIS.Data.DataRequest.Parser).Before(End).ParseOrThrow(qry);
        public string ToString(string qry)
        {
            try
            {
                var request = Parse(qry);
                return JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    WriteIndented = true
                });
            }
            catch (ParseException e)
            {
                return e.Message;
            }
        }
    }
}
