using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BotApplication2
{
    public class QnaMarkerResult
    {
        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; }

        [JsonProperty(PropertyName = "score")]
        public string Score { get; set; }
    }

    public class QnaMarkerResultList
    {
        [JsonProperty(PropertyName = "answers")]
        public List<QnaMarkerResult> Answers { get; set; }
    }
}
