using Newtonsoft.Json;
using System.Collections.Generic;

namespace CurrencyConverter.Models
{
    public class ConversionData
    {
        [JsonProperty(PropertyName = "rates")]
        public Dictionary<string, float> Rates { get; set; }
        public string Base { get; set; }
        public string Date { get; set; }
    }
}

