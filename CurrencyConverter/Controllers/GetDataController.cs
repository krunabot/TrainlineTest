using CurrencyConverter.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetDataController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        public GetDataController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET api/Currency
        [Route("GetPrice")]
        [HttpGet()]
        public async Task<ActionResult<ResponseData>> GetPrice(Decimal price, string sourceCurrency, string targetCurrency)
        {
            var responseData = new ResponseData();

            try
            {
                ValidateInput(sourceCurrency, targetCurrency);
                var source = sourceCurrency.ToUpper();
                var target = targetCurrency.ToUpper();
                var URL = $"https://api.exchangeratesapi.io/latest?base={source}&symbols={target}";

                using (var response = await _httpClient.GetAsync(URL))
                {
                    var apiResponse = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode == false)
                    {
                        var errMsg = JsonConvert.DeserializeObject<Dictionary<string, string>>(apiResponse);
                        var msg = errMsg.GetValueOrDefault("error");
                        throw new Exception(msg);
                    }

                    var data = JsonConvert.DeserializeObject<Rootobject>(apiResponse);
                    responseData.Price = (decimal)data.Rates.GetValueOrDefault(target) * price;
                    responseData.TargetCurrency = target;
                }

                return Ok(responseData);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public static void ValidateInput(string sourceCurrency, string targetCurrency)
        {
            StringBuilder msg = new StringBuilder();

            if (sourceCurrency == null || targetCurrency == null)
            {
                msg.Append("Input Currencies cannot be Null.");
                throw new ArgumentNullException(msg.ToString());
            }

            if (sourceCurrency == targetCurrency)
            {
                msg.Append("Source and Target Currencies must be different.");
                throw new ArgumentException(msg.ToString());
            }

        }
    }
}
