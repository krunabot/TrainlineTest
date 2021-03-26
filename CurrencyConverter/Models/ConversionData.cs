using Newtonsoft.Json;
using System.Collections.Generic;

namespace CurrencyConverter.Models
{
    public class Rootobject
    {
        [JsonProperty(PropertyName = "rates")]
        public Dictionary<string, float> Rates { get; set; }
        public string Base { get; set; }
        public string date { get; set; }
    }
}

