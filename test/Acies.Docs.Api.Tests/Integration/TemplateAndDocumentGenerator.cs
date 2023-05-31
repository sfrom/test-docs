using Acies.Docs.Api;
using Acies.Docs.Api.Models;
using Acies.Docs.Models;
using Acies.Docs.Services.Amazon;
using Acies.Docs.Services.Repositories;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Common.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Acies.Docs.Services.Tests.Integration
{
    public class TemplateAndDocumentGenerator
    {
        private readonly HttpClient _httpClient;

        public TemplateAndDocumentGenerator()
        {
            Environment.SetEnvironmentVariable("DynamoDbDataRepositoryOptions__TABLE", "sfrdevstackdocs-TestStack-168KJ0I921GF7-DocsTable-1DN0BW9GWL8Y0");
            Environment.SetEnvironmentVariable("SNS", "arn:aws:sns:eu-central-1:864358974821:sfrdevstackdocs-LocalSNS-15C72KmVXr6B");
            Environment.SetEnvironmentVariable("SERVICENAME", "docs");
            var webAppFactory = new WebApplicationFactory<LocalEntryPoint>();
            _httpClient = webAppFactory.CreateDefaultClient();
        }

        [Fact]
        public void DeserializePDFOutputString()
        {
            var input = "{\r\n    \"type\": \"pdf\",\r\n    \"layout\": {\r\n        \"format\": \"A3\",\r\n        \"margins\": {\r\n            \"top\": 10,\r\n            \"bottom\": 12,\r\n            \"left\": 13,\r\n            \"right\": 11\r\n        },\r\n        \"header\": {\r\n            \"content\": \"<div>Header<\\/div>\",\r\n            \"style\": null,\r\n            \"contentFileName\": null\r\n        },\r\n        \"body\": {\r\n            \"content\": \"<html>Myhtml<\\/html>\",\r\n            \"style\": null,\r\n            \"contentFileName\": null\r\n        },\r\n        \"footer\": {\r\n            \"content\": \"<div>Footer<\\/div>\",\r\n            \"style\": null,\r\n            \"contentFileName\": null\r\n        },\r\n        \"assets\": [\r\n            {\r\n                \"uri\": \"\",\r\n                \"path\": \"\"\r\n            },\r\n            {\r\n                \"uri\": \"\",\r\n                \"path\": \"\"\r\n            }\r\n        ]\r\n    },\r\n    \"name\": \"1212\",\r\n    \"tags\": {\r\n        \"type\": \"Order\"\r\n    }\r\n}";
            var serializer = new JsonSerializerService();
            var test = serializer.Deserialize<PdfOutput>(input);
            Assert.True(test != null);
        }

        [Fact]
        public void CreateDocumentOutputs()
        {
            dynamic dataInput = new ExpandoObject();
            dataInput.Name = "Ole Nielsen";
            dataInput.OrderNumber = "1212";
            dataInput.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            ISerializer ser = new JsonSerializerService();
            var input = ser.Serialize(dataInput);

            var templateVersion = new TemplateVersion()
            {
                Outputs = new List<TemplateOutputBase>() { new PdfOutput() { Name = "{{ OrderNumber }}" } }
            };

            var ds = new DocumentService(null, null, null, null, null);

            var result = ds.GenerateDocumentOutputs(input, templateVersion);

            Assert.True(result.Count > 0);
            Assert.True(result[0].Name == dataInput.OrderNumber + ".pdf");
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetDocumentByTags_Ok()
        {
            //Arrange
            IAmazonDynamoDB a = new AmazonDynamoDBClient();
            IDynamoDBContext c = new DynamoDBContext(a);
            ISerializer ser = new JsonSerializerService();
            var tenant = new TenantContext() { AccountId = Guid.NewGuid().ToString() };
            var options = new DynamoDbDataRepositoryOptions()
            {
                Table = Environment.GetEnvironmentVariable("DynamoDbDataRepositoryOptions__TABLE")
            };
            var mock = new Mock<IOptionsSnapshot<DynamoDbDataRepositoryOptions>>();
            mock.Setup(m => m.Value).Returns(options);

            var dbs = new TemplateRepository(a, c, ser, mock.Object.Value, tenant);

            ITemplateService s = new TemplateService(dbs);

            var data = new TemplateCreateData
            {
                Name = "My template",
                Input = new TemplateInput(),
                Outputs = new List<TemplateOutputBase>
                {
                    new PdfOutput
                    {
                        Name="{{ OrderNumber }}",
                        Layout = new Layout
                        {
                            Format="A3",
                            Margins=new Margins
                            {
                                Top=10,
                                Right=11,
                                Bottom=12,
                                Left=13,
                            },
                            Header = new Models.TemplateRef
                            {
                                Content="<div>Header</div>",
                            },
                            Body = new Models.TemplateRef
                            {
                                Content ="<html>Myhtml</html>",
                            },
                            Footer = new Models.TemplateRef
                            {
                                Content= "<div>Footer</div>",
                            },
                            Assets=new List<Asset>
                            {
                               new Asset
                               {
                                   Path="/images/acies-logo.png",
                               },
                               new Asset
                               {
                                   Path="/docs/terms",
                               }
                            },
                        },
                        Tags=new Dictionary<string, string>
                        {
                            {"Type", "Order" },
                        },
                    },
                },
                Tags = new Dictionary<string, string>
                {
                    {"Type","Order" },
                    {"Subtype","EndCostumerNr" },
                    {"Number","1217" },
                },
            };

            var r = await s.CreateTemplateAsync(data);

            var tv = await s.GetTemplateVersionByKeyAndVersionAsync(r.Id, r.Version);

            dynamic dataInput = new ExpandoObject();
            dataInput.Name = "Ole Nielsen";
            dataInput.OrderNumber = "1212";
            dataInput.DrawingUrl = "https://drawing.cdn.acies.dk/124.pdf";

            var documentData = new DocumentCreateData
            {
                Input = ser.Serialize(dataInput),
                Tags = new Dictionary<string, string>
                {
                    {"Type","Order" },
                    {"Subtype","EndCostumerNr" },
                    {"Number","1217" },
                },
                TemplateId = r.Id,
                TemplateVersion = r.Version,
            };

            //IDocumentService ds = new DocumentService(ddbs, mockSns.Object);
            _httpClient.DefaultRequestHeaders.Add("x-account-id", tenant.AccountId);
            var responsePost = await _httpClient.PostAsJsonAsync("/documents", documentData);
            //Act
            //var created = await ds.CreateDocumentAsync(documentData);
            var response = await _httpClient.GetAsync($"/documents?Number=1217");
            //var read = await ds.GetDocumentsByTagsAsync(new Dictionary<string, string> { { "Number", "1217" } });
            var stringResult = await response.Content.ReadAsStringAsync();
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)["result"];
            var responseGet = resultGet.ToObject<List<DocumentResource>>();

            //Assert
            Assert.NotNull(responseGet);
            Assert.Equal("1217", responseGet.First().Tags["number"]);
        }


        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetDocumentByTagsFullOrderPrintExample_Ok()
        {
            //Arrange

            IAmazonDynamoDB a = new AmazonDynamoDBClient();
            IDynamoDBContext c = new DynamoDBContext(a);
            ISerializer ser = new JsonSerializerService();

            var tenant = new TenantContext()
            {
                AccountId = $"{Guid.NewGuid()}"
            };

            var options = new DynamoDbDataRepositoryOptions()
            {
                Table = Environment.GetEnvironmentVariable("DynamoDbDataRepositoryOptions__TABLE")
            };

            var mock = new Mock<IOptionsSnapshot<DynamoDbDataRepositoryOptions>>();

            mock.Setup(m => m.Value).Returns(options);

            var templateRepository = new TemplateRepository(a, c, ser, mock.Object.Value, tenant);

            ITemplateService templateService = new TemplateService(templateRepository);

            var data = new TemplateCreateData
            {
                Name = "My template",
                Input = new TemplateInput(),
                Outputs = new List<TemplateOutputBase>
                {
                    new PdfOutput
                    {
                        Name="{{ OrderNumber }}",
                        Layout = new Layout
                        {
                            Format="A4",
                            Margins=new Margins
                            {
                                Top=140,
                                Right=10,
                                Bottom=80,
                                Left=10,
                            },
                            Header = new Models.TemplateRef
                            {
                                Content = CreateDummyHeaderTemplateHtml()
                            },
                            Body = new Models.TemplateRef
                            {
                                Content = CreateDummyBodyTemplateHtml()
                            },
                            Footer = new Models.TemplateRef
                            {
                                Content = CreateDummyFooterTemplateHtml()
                            },
                            Assets=new List<Asset>
                            {
                               new Asset
                               {
                                   Path="/images/acies-logo.png",
                               },
                               new Asset
                               {
                                   Path="/docs/terms",
                               }
                            },
                        },
                        Tags=new Dictionary<string, string>
                        {
                            {"Type", "Order" },
                        },
                    },
                },
                Tags = new Dictionary<string, string>
                {
                    {"Type","Order" },
                    {"Subtype","EndCostumerNr" },
                    {"Number","1217" },
                },
            };

            var template = await templateService.CreateTemplateAsync(data);

            var templateVersion = await templateService.GetTemplateVersionByKeyAndVersionAsync(template.Id!, template.Version);

            dynamic dataInput = CreateDummyInputData();

            var documentData = new DocumentCreateData
            {
                Input = JsonConvert.SerializeObject(dataInput, Formatting.None, new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }),
                Tags = new Dictionary<string, string>
                {
                    {"Type","Order" },
                    {"Subtype","EndCostumerNr" },
                    {"Number","1217" },
                },
                TemplateId = template.Id,
                TemplateVersion = template.Version,
            };

            _httpClient.DefaultRequestHeaders.Add("x-account-id", tenant.AccountId);

            // Act

            var responsePost = await _httpClient.PostAsJsonAsync("/documents", documentData);
            var response = await _httpClient.GetAsync("/documents?Number=1217");
            var stringResult = await response.Content.ReadAsStringAsync();

            JsonSerializerSettings settings = new()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var resultGet = JsonConvert.DeserializeObject<JObject>(stringResult, settings)!["result"];
            var responseGet = resultGet?.ToObject<List<DocumentResource>>();

            // Assert

            Assert.NotNull(responseGet);
            Assert.Equal("1217", responseGet?.FirstOrDefault()?.Tags?.FirstOrDefault(x => x.Key.Equals("number")).Value);
        }

        private static dynamic CreateDummyInputData()
        {
            return new
            {
                OrderNumber = 1234,
                Id = 223,
                Name = "Peter",
                MoreName = "Mikkelsen",
                PaymentAddress = new
                {
                    Attention = "Peter Mikkelsen",
                    City = "Silkeborg",
                    Zip = "8600",
                    Id = 12,
                    Name = "BitstoreCompagny",
                    Street = "Lysbrohøjen 3",
                },
                DeliveryAddress = new
                {
                    Attention = "1Peter Mikkelsen",
                    City = "Sil1keborg",
                    Zip = "86010",
                    Id = 121,
                    Name = "Ne1w Bitstore",
                    Street = "S1ilkeborg vej 145",
                },
                Head = new
                {
                    CorrectionDeadline = System.DateTime.Now.AddDays(30),
                    Date = System.DateTime.Now,
                    DebitorNo = "86802250",
                    PaymentDate = System.DateTime.Now.AddDays(70),
                    CvrNo = "123456789",
                    Id = 12,
                    Mobile = "21617232",
                    OrderNo = "1001007",
                    OurRef = "Peter Mikkelsen",
                    Phone = "+45 80225050",
                    RekvisitionNo = "0A197",
                    TermsOfPayment = "Lb + 30",
                    YourRef = "Sune Maagaard Andersen",
                },
                Colors = new List<dynamic>
                {
                    new
                    {
                        Description = "Karm farve",
                        Value = "RAL 9010 Hvid",
                    },
                    new
                    {
                        Description = "Karm alu. farve",
                        Value = "RAL 9010 Hvid (Glans 30)",
                    },
                    new
                    {
                        Description = "Ramme farve",
                        Value = "RAL 9010 Hvid",
                    },
                    new
                    {
                        Description = "Ramme alu. farve",
                        Value = "RAL 9010 Hvid (Glans 30)",
                    },
                },
                OrderLines = new List<dynamic> {
                    new {
                        Id = 35,
                        Position = "1a",
                        Model = "TA-DB12-FOR-",
                        Quantity = 2,
                        Unit = "stk",
                        Description = "1 fag dannebrog",
                        Width = 1200,
                        Height = 1000,
                        UnitPrice = 3925.5,
                        Price = 15700.20,
                        DrawingUrl = "images/my1.svg",
                        DetailDescription = "",
                    },
                    new {
                        Id = 35,
                        Position = "2",
                        Model = "TA-DB12-FOR-",
                        Quantity = 4.5,
                        Unit = "stk",
                        Description = "1 fag dannebrog",
                        Width = 1200,
                        Height = 1000,
                        UnitPrice = 3925.5,
                        Price = 15700.00,
                        DrawingUrl = "images/my2.svg",
                        DetailDescription = "",
                    },
                    new {
                        Id = 35,
                        Position = "3",
                        Model = "TA-DB12-FOR-",
                        Quantity = 8.5,
                        Unit = "stk",
                        Description = "1 fag dannebrog",
                        Width = 1200,
                        Height = 1000,
                        UnitPrice = 3925.5,
                        Price = 15700.00,
                        DrawingUrl = "images/my3.svg",
                        DetailDescription = "",
                    },
                    new {
                        Id = 35,
                        Position = "4",
                        Model = "TA-DB12-FOR-",
                        Quantity = 5,
                        Unit = "stk",
                        Description = "1 fag dannebrog",
                        Width = 1200,
                        Height = 1000,
                        UnitPrice = 3925.5,
                        Price = 15700.00,
                        DrawingUrl = "images/my4.svg",
                        DetailDescription = "",
                    },
                    new {
                        Id = 35,
                        Position = "5",
                        Model = "TA-DB12-FOR-",
                        Quantity = 7.25,
                        Unit = "stk",
                        Description = "1 fag dannebrog",
                        Width = 1200,
                        Height = 1000,
                        UnitPrice = 3925.5,
                        Price = 15700.00,
                        DrawingUrl = "images/my5.svg",
                        DetailDescription = "",
                    },
                },
            };
        }
        private static string CreateDummyHeaderTemplateHtml()
        {
            return @"<!DOCTYPE html><html><head>    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8""> <style>        .header {            width: 100%;            float: left;            position: relative;        }        .logo {            position: relative;            width: 200px;            float: left;            height: auto;            margin: 0px 0px 0px 10px;            padding: 0px;        }        .header-text {            float: right;            position: relative;            font-size: 10px;            color: black;            margin: 0px 10px 10px 0px;        }        .header-text p {            margin: 0px;        }    </style></head>    <body>        <div class=""header"">            <div class=""logo"">                <img src=""{{inject-local-image 'images/acies-blaa-rgb.svg'}}"" style=""width:100%;height:100%;"" />            </div>            <div class=""header-text"">                <p>Acies A/S</p>                <p>Lysbrohøjen 3</p>                <p>8600 Silkeborg</p>                <p>86806300</p>                <p>info@acies.dk</p>                <p>www.acies.dk</p>            </div>        </div>    </body></html>";
        }
        private static string CreateDummyBodyTemplateHtml()
        {
            return @"<!DOCTYPE html><html><head>    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8""> <style>        .sideo svg .inside {            display: none;        }        .sidei svg .outside {            display: none;        }        path[data-coating] {            fill: #e5e5e5;            stroke-width: 1px;            stroke: #1b6ec2;        }        path.clearances[data-coating] {            fill: rgba(174, 235, 248, 0.50);            stroke-width: 1px;            stroke: #1b6ec2;        }        path.frame[data-coating*=""9004""], path.sash[data-coating*=""9004""] {            fill: #0e0e10;            stroke-width: 1px;            stroke: #1b6ec2;        }        path[data-coating*=""9004""], path.clearances[data-coating*=""9004""] {            fill: #2b2b2c;            stroke-width: 1px;            stroke: #1b6ec2;        }        path.frame[data-coating*=""9010""], path.sash[data-coating*=""9010""] {            fill: #e1dcd1;            stroke-width: 1px;            stroke: #1b6ec2;        }        path[data-coating*=""9010""], path.clearances[data-coating*=""9010""] {            fill: #f1ece1;            stroke-width: 1px;            stroke: #1b6ec2;        }        path.frame[data-coating*=""1013""], path.sash[data-coating*=""1013""] {            fill: #d3c9b6;            stroke-width: 0px;            stroke: #1b6ec2;        }        path[data-coating*=""1013""], path.clearances[data-coating*=""1013""] {            fill: #e3d9c6;            stroke-width: 0;            stroke: #808080;        }        path.frame[data-coating*=""9017""], path.sash[data-coating*=""9017""] {            fill: #1a191a;        }        path[data-coating*=""9017""], path.clearances[data-coating*=""9017""] {            fill: #1e1e1e;        }        path.frame[data-coating*=""3009""], path.sash[data-coating*=""3009""] {            fill: rgb(63,34,38);            stroke-width: 1px;            stroke: #808080;        }        path[data-coating*=""3009""], path.clearances[data-coating*=""3009""] {            fill: rgb(109,49,41);            stroke-width: 1px;            stroke: #808080;        }        path.frame[data-coating*=""3011""], path.sash[data-coating*=""3011""] {            fill: #642424;            stroke-width: 1px;            stroke: #808080;        }        path[data-coating*=""3011""], path.clearances[data-coating*=""3011""] {            fill: #781f19;            stroke-width: 1px;            stroke: #808080;        }        path.frame[data-coating*=""6009""], path.sash[data-coating*=""6009""] {            fill: #1f3a3d;            stroke-width: 1px;            stroke: #808080;        }        path[data-coating*=""6009""], path.clearances[data-coating*=""6009""] {            fill: #31372b;            stroke-width: 1px;            stroke: #808080;        }        path.frame[data-coating*=""7035""], path.sash[data-coating*=""7035""] {            fill: #d5d5d5;            stroke-width: 1px;            stroke: #808080;        }        path[data-coating*=""7035""], path.clearances[data-coating*=""7035""] {            fill: #d7d7d7;            stroke-width: 1px;            stroke: #808080;        }        path.frame[data-coating*=""9005""], path.sash[data-coating*=""9005""] {            fill: #090909;            stroke-width: 1px;            stroke: #808080;        }        path[data-coating*=""9005""], path.clearances[data-coating*=""9005""] {            fill: #0a0a0a;            stroke-width: 1px;            stroke: #808080;        }        path.effekt-frame[data-coating]:hover,        path.frame[data-coating]:hover,        path.effekt-sash[data-coating]:hover,        path.sash[data-coating]:hover {            fill: #808080;        }        .opening {            fill: none;            stroke-width: 1;        }            .opening.in {                stroke: blue;                stroke-dasharray: 10,10;            }            .opening.out {                stroke: red;            }            .opening.fixed {                stroke: red;            }        .measurement {            stroke: #ff0000;            fill: #ff0000;            stroke-width: 1px;        }            .measurement text {                text-anchor: middle;                font-size: 70px;                dominant-baseline: text-after-edge;            }        g#vent {            fill: blue;        }        use.symbol {            fill: #7d7d7d;            stroke-width: 1px;            stroke: #808080;        }        pattern line {            stroke: rgb(255,0,0);            stroke-width: 1;        }        path.clearances-overlay {            display: none;        }            path.clearances-overlay[data-filltype=""Vertical""] {                fill: url(#pattern-stripe90);                display: block;            }            path.clearances-overlay[data-filltype=""Dots""] {                fill: url(#pattern-dots);                display: block;            }            path.clearances-overlay[data-filltype=""Diagonal45""] {                fill: url(#pattern-stripe45);                display: block;            }            path.clearances-overlay[data-filltype=""Diagonal135""] {                fill: url(#pattern-stripe135);                display: block;            }            path.clearances-overlay[data-filltype=""Fishbone""] {                fill: url(#pattern-fishbone);                display: block;            }    </style>    <style>        .sideo {            min-width: 100px;            min-height: 100px;        }        html {            font-size: 14px;        }        @media (min-width: 768px) {            html {                font-size: 16px;            }        }        html {            position: relative;            min-height: 100%;        }        @media print {            body {                padding: 0px 0 0 0;            }            .orderlines .page-break {                break-inside: avoid;            }        }        @media (min-width: 800px) {            .page {                box-shadow: 0 0 15px #999;            }        }        body {            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;            margin: 0 auto;            width: 850px;        }        @media screen {            body {                background: #e2e2e2;            }        }        div[xmlns]:hover {            outline: dotted 1px gray;            cursor: pointer;        }        [data-template]:hover {            outline: dotted 1px gray;            cursor: pointer;            background-color: azure;        }        p:hover {            outline: solid 1px gray;        }        div, p {            box-sizing: border-box;        }        .dropshadow {            display: none;        }        .page {            background: #fff;            padding: 0 25px        }        .scustomer-address, .scustomer-delevery-address {            display: inline-block;            margin: 40px 200px 0 0;        }        .customer-address p, .customer-delevery-address p {            margin: 0;        }        .customer-address h3, .customer-delevery-address h3 {            margin: 0;        }        h1 {            margin: 50px 0 0 0;        }        .order-info table {            border-top: solid 1px black;            border-left: solid 1px black;            border-right: solid 1px black;            width: 100%;            border-spacing: 0;        }            .order-info table tr {                border-spacing: 0;            }            .order-info table td {                border-bottom: solid 1px black;            }            .order-info table th {                text-align: left;            }        dl {            width: 100%;        }        dt:before {            content: '';            position: absolute;            bottom: 0.2rem;            width: 100%;            height: 0;            line-height: 0;            border-bottom: 2px dotted;        }        dt {            width: 300px;            position: relative;            float: left;            background-color: white;        }            dt span {                background-color: inherit;                position: relative;                padding-right: 2px;            }        dd {            position: relative;            margin-left: 306px;        }        .qr-code {            max-width: 100px;            position: relative;            top: -121px;            right: 20px;        }        .orderlines svg {            position: relative;            width: 300px;            height: 200px;            overflow: unset;        }        .orderlines table {            width: 100%;            border-collapse: collapse;        }        .orderlines .itemheader td:nth-child(1),        .orderlines .itemheader td:nth-child(3) {            text-align: center;        }        .orderlines thead th:nth-child(6),        .orderlines thead th:nth-child(7),        .orderlines thead th:nth-child(8),        .orderlines thead th:nth-child(9),        .orderlines .itemheader td:nth-child(6),        .orderlines .itemheader td:nth-child(7),        .orderlines .itemheader td:nth-child(8),        .orderlines .itemheader td:nth-child(9) {            text-align: right;        }        .orderlines .itemheader td:nth-child(6) {            text-align: right;        }        .orderlines:nth-child(3) {            margin-top: 200px;        }        .description {            padding: 15px 0;        }            .description p {                margin: 0;            }        .itemheader td {            border-top: solid 1px #808080;            background-color: #f2f2f2;        }        .itemheader {            border-top: solid 1px #808080;            page-break-before: always;            position: relative;        }        .orderlines table thead {            display: table-header-group;            text-align: left;        }        .orderlines table tbody {            display: table-row-group;            position: relative;        }        .orderlines table tfoot {            display: table-footer-group;        }        .total {            background-color: #f2f2f2;            border-top: solid 1px;            border-bottom: double;        }            .total p {                margin: 0;                display: inline-block;            }                .total p:nth-child(2) {                    float: right;                }    </style></head>    <body>        <div class=""page"">            <div class=""customer-address"" data-template=""date-field"">                <h3>                    Faktura adresse                </h3>                <p>                    {{this.deliveryAddress.name}}                </p>                <p>                    {{this.deliveryAddress.attention}}                </p>                <p>                    {{this.deliveryAddress.street}}                </p>                <p>                    {{this.deliveryAddress.zip}}                    {{this.deliveryAddress.city}}                </p>            </div>            <div class=""customer-address"" data-template=""date-field"">                <h3>                    Leverings adresse                </h3>                <p>                    {{this.paymentAddress.name}}                </p>                <p>                    {{this.paymentAddress.attention}}                </p>                <p>                    {{this.paymentAddress.street}}                </p>                <p>                    {{this.paymentAddress.zip}}                    {{this.paymentAddress.city}}                </p>            </div>            <h1>Ordrebekræftelse</h1>            <div class=""order-info"">                <table>                    <thead>                        <tr>                            <th>Debitornr.</th>                            <th>CVR nr.</th>                            <th>Rekvisition</th>                            <th>Deres ref.</th>                            <th>Rettefrist.</th>                            <th>Dato</th>                        </tr>                    </thead>                    <tbody>                        <tr>                            <td>                                {{this.head.debitorNo}}                            </td>                            <td>                                {{this.head.cvrNo}}                            </td>                            <td>                                {{this.head.rekvisitionNo}}                            </td>                            <td>                                {{this.head.yourRef}}                            </td>                            <td>                                {{format-date this.head.correctionDeadline}}                            </td>                            <td>                                {{format-date this.head.date}}                            </td>                        </tr>                    </tbody>                    <thead>                        <tr>                            <th>Telefon</th>                            <th>Mobil</th>                            <th>Betalingsbetingelser</th>                            <th>Vores ref.</th>                            <th>Betalingsdato</th>                            <th>Ordre nr.</th>                        </tr>                    </thead>                    <tbody>                        <tr>                            <td>                                {{this.head.phone}}                            </td>                            <td>                                {{this.head.mobile}}                            </td>                            <td>                                {{this.head.termsOfPayment}}                            </td>                            <td>                                {{this.head.ourRef}}                            </td>                            <td>                                {{format-date this.head.paymentDate}}                            </td>                            <td>                                {{this.head.orderNo}}                            </td>                        </tr>                    </tbody>                </table>            </div>            <div class=""colors"" data-template=""colors"">                <dl>                    {{#each this.colors.color}}                    <dt>                        <span>                            {{this.description}}                        </span>                    </dt>                    <dd>                        {{this.value}}                    </dd>                    {{/each}}                </dl>            </div>            <div class=""orderlines"" data-template=""orderlines"">                {{#each this.orderLines}}                <table>                    <thead>                        <tr>                            <th>Pos</th>                            <th>Modelnr</th>                            <th>Antal</th>                            <th>Enhed</th>                            <th>Beskrivelse</th>                            <th>Bredde</th>                            <th>Højde</th>                            <th>Enhedspris</th>                            <th>Subtotal</th>                        </tr>                    </thead>                    <tbody>                        <tr class=""itemheader"">                            <td>                                {{this.position}}                            </td>                            <td>                                {{this.model}}                            </td>                            <td>                                {{format-number this.quantity 2 'da-DK'}}                            </td>                            <td>                                {{this.unit}}                            </td>                            <td>                                {{this.description}}                            </td>                            <td>                                {{format-number this.width 2 'da-DK'}}                            </td>                            <td>                                {{format-number this.height 2 'da-DK'}}                            </td>                            <td>                                {{format-number this.unitPrice 2 'da-DK'}}                            </td>                            <td>                                {{format-number (math-multiply this.quantity this.unitPrice) 2 'da-DK'}}                            </td>                        </tr>                        <tr>                            <td colspan=""4"">                                <div class=""sideo"">                                    {{inject-local-svg this.drawingUrl}}                                </div>                                <div class=""sidei"">                                    {{inject-local-svg this.drawingUrl}}                                </div>                            </td>                            <td colspan=""4"" class=""description page-break"">                                <p>Træ/Alu standard A</p>                                <p>Certificeret xxx-x www.energivinduer.dk</p>                                <p>Uw: 1.38W/m2K Ew: -37.7 kWh/m2</p>                                <p>LT: 82 % g: 73 % Glasandel: 62 %</p>                                <p>Modelareal: 1.20 m2 Vægt: 46 kg</p>                                <p>Profiler: T/A Udadg. vindue 100mm karm Kehl</p>                                <p>Ramme 1 Indhold: Vin ram 1 felt</p>                                <p>Ramme 1 Beslåning: TA Sidehængt VU</p>                                <p>Ramme 2 Indhold: Vin ram 1 felt</p>                                <p>Ramme 2 Beslåning: TA Sidehængt VU</p>                                <p>RAMME: 1-2</p>                                <p>Felt 1 Indhold: 2 lags - 24mm energiglas</p>                                <p>Standard 2 lags</p>                                <p>RAMME: 1-2</p>                                <p>Felt: Sort standard spacer (RAL9005)</p>                                <p>Glas: Fals: Ant: Tot:</p>                                <p>654 x 815 662.0 x 822.7 1 4</p>                                <p>654 x 419 662.0 x 427.3 1 4</p>                            </td>                            <td>                                <img src=""https://chart.googleapis.com/chart?chs=150x150&amp;cht=qr&amp;chl={{this.drawingUrl}}"" class=""qr-code"" />                            </td>                        </tr>                    </tbody>                </table>                {{#unless @islast }}                {{#if (math-mod @index 2 = 1)}}                <div class=""page-break""></div>                {{/if}}                {{/unless}}                {{/each}}                <div class=""total"">                    <p>Total</p>                    <p>{{format-number (specific-orderline-sumproducts this.orderLines) 2 'da-DK'}}</p>                </div>            </div>        </div>    </body></html>";
        }
        private static string CreateDummyFooterTemplateHtml()
        {
            return @"<!DOCTYPE html><html><head>    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8""> <style type=""text/css"">      .content-footer {        background-color: white;        color: black;        font-size: 10px;        position: relative;        float:left;        margin: 0px 0px 0px 10px;      }    </style>  </head>  <body>    <div class=""content-footer"">        <p>Side <span class=""pageNumber""></span> af <span class=""totalPages""></span></p>    </div>  </body></html>";
        }
    }
}