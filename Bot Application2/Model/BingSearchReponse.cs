using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotApplication2
{
    public class BingSearchReponse
    {
        public WebPages webPages { get; set; }
    }

    public class Value
    {
        public string displayUrl { get; set; }
        public string snippet { get; set; }

        public override string ToString()
        {
            return $"WEB: {displayUrl} Descrip: {snippet}";
        }
    }

    public class WebPages
    {
        public List<Value> value { get; set; }
    }
}
