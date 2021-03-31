using CurrencyConverter.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetCurrencyConversionDataController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        public GetCurrencyConversionDataController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // GET api/Currency
        [Route("GetPrice")]
        [HttpGet()]
        public async Task<ActionResult<ResponseData>> GetPrice([FromQuery] RequestData requestData)
        {
            var responseData = new ResponseData();

            try
            {
                var source = requestData.SourceCurrency.ToUpper();
                var target = requestData.TargetCurrency.ToUpper();

                var validMsg = ValidateInput(source, target);
                if (validMsg.Length > 0 )
                {
                    return BadRequest(validMsg);
                }

                ValidateInput(requestData.SourceCurrency, requestData.TargetCurrency);



                var URL = $"https://api.exchangeratesapi.io/latest?base={source}&symbols={target}";
                var client = _clientFactory.CreateClient();

                using (var response = await client.GetAsync(URL))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var conversionData = await response.Content.ReadFromJsonAsync<ConversionData>();
                        responseData.Price = CalculateConvertedPrice(requestData, conversionData, target);
                        responseData.TargetCurrency = target;
                        return Ok(responseData);
                    }

                    var apiResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    return BadRequest(GetErrorMessage(apiResponse));
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private static decimal CalculateConvertedPrice(RequestData requestData, ConversionData conversionData, string target)
        {
            return (decimal)conversionData.Rates.GetValueOrDefault(target) * requestData.Price;
        }

        private static string GetErrorMessage(Dictionary<string, string> apiResponse)
        {
            var msg = apiResponse.GetValueOrDefault("error");
            return msg;
        }

        public static string ValidateInput(string sourceCurrency, string targetCurrency)
        {
            var msg = string.Empty;

            if (sourceCurrency == targetCurrency)
            {
                msg = "Source and Target Currencies must be different.";
            }

            return msg;

        }
    }
}
