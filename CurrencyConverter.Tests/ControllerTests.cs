using CurrencyConverter.Controllers;
using CurrencyConverter.Models;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CurrencyConverter.Tests
{
    public class ControllerTests
    {
        Mock<IHttpClientFactory> factoryMock;
        Mock<HttpMessageHandler> handlerMock;
        RequestData requestData;
        ResponseData responseData;
        HttpResponseMessage response;

        public ControllerTests()
        {
            factoryMock = new Mock<IHttpClientFactory>();


            handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
                  )
               .ReturnsAsync(response);

            responseData = new ResponseData();
            requestData = new RequestData();
            response = new HttpResponseMessage();
        }

        [Fact]
        public async Task CheckCorrectDataIsRetrievedSuccessfullyFromAPI()
        {
            // Arrange
            response.StatusCode = HttpStatusCode.OK;
            response.Content = new StringContent(@"{ ""rates"": {""USD"" : 1.3710144928 },""base"": ""GBP"",""date"": ""2021 - 03 - 25""}");
            requestData.Price = 1;
            requestData.SourceCurrency = "GBP";
            requestData.TargetCurrency = "USD";

            var httpClient = new HttpClient(handlerMock.Object);
            factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient).Verifiable();

            // Act
            var controller = new GetCurrencyConversionDataController(factoryMock.Object);
            var getDataController = await controller.GetPrice(requestData) ;
            var expectedUri = new Uri("https://api.exchangeratesapi.io/latest?base=GBP&symbols=USD");

            //Assert
            Assert.NotNull(getDataController);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1),
               ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri == expectedUri),
               ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task CheckFailsWhenSourceCurrencyIsInvalid()
        {
            // Arrange
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Content = new StringContent(@"{ ""error"": ""Base 'XXX' is not supported.""}");
            requestData.Price = 1;
            requestData.SourceCurrency = "XXX";
            requestData.TargetCurrency = "USD";

            var httpClient = new HttpClient(handlerMock.Object);
            factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient).Verifiable();

            // Act
            var controller = new GetCurrencyConversionDataController(factoryMock.Object);
            var getDataController = await controller.GetPrice(requestData);
            var expectedUri = new Uri("https://api.exchangeratesapi.io/latest?base=XXX&symbols=USD");

            //Assert
            Assert.NotNull(getDataController);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1),
               ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri == expectedUri),
               ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task CheckFailsWhenTargetCurrencyIsInvalid()
        {
            // Arrange
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Content = new StringContent(@"{ ""error"": ""Symbols 'ZZZ' are invalid for date 2021-03-25.""}");
            requestData.Price = 1;
            requestData.SourceCurrency = "GBP";
            requestData.TargetCurrency = "ZZZ";

            var httpClient = new HttpClient(handlerMock.Object);
            factoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient).Verifiable();

            // Act
            var controller = new GetCurrencyConversionDataController(factoryMock.Object);
            var getDataController = await controller.GetPrice(requestData);
            var expectedUri = new Uri("https://api.exchangeratesapi.io/latest?base=GBP&symbols=ZZZ");

            //Assert
            Assert.NotNull(getDataController);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1),
               ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri == expectedUri),
               ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData("GBP", null)]
        [InlineData(null, "GBP")]

        public void CheckThrowsExceptionWhenSourceOrTargetIsNull(string source, string target)
        {
            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => GetCurrencyConversionDataController.ValidateInput(source, target));

            //Assert
            Assert.Equal("Value cannot be null. (Parameter 'Input Currencies cannot be Null.')", ex.Message);
        }


        [Fact]
        public void CheckThrowsExceptionWhenSourceAndTargetCurrenciesAreEqual()
        {
            // Act
            var ex = Assert.Throws<ArgumentException>(() => GetCurrencyConversionDataController.ValidateInput("GBP", "GBP"));

            //Assert
            Assert.Equal("Source and Target Currencies must be different.", ex.Message);
        }

    }

}
