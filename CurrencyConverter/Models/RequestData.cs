using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Models
{
    public class RequestData  
    {
        [Required]
        public Decimal Price { get; set; }
        [Required]
        public string SourceCurrency { get; set; }
        [Required]
        public string TargetCurrency { get; set; }

    }
}
