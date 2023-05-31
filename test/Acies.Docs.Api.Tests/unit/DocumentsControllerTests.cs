using Acies.Docs.Api.Controllers.V1;
using Acies.Docs.Models;
using Acies.Docs.Services;
using Acies.Docs.Services.Amazon;
using Acies.Docs.Services.Repositories;
using Common.Models;
using DatabaseContext.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Acies.Docs.Api.Tests
{
    public class DocumentsControllerTests
    {
        [Fact]
        public async Task GetDocuments_ReturnsDocuments()
        {
            //var options = new DynamoDbDataRepositoryOptions()
            //{
            //    Table = "testtable"
            //};
            //var tenant = new TenantContext() { AccountId = "4711" };
            //var mock = new Mock<IOptionsSnapshot<DynamoDbDataRepositoryOptions>>();
            //mock.Setup(m => m.Value).Returns(options);
            //var documentRepository = new DocumentRepository(null, null, null, mock.Object.Value, tenant);
            //var documentService = new DocumentService(documentRepository);
            //var controller = new DocumentsController(documentService, tenant);
            //var httpContext = new DefaultHttpContext();
            //httpContext.Request.Headers.Add("Content-Type", "application/json");
            //httpContext.Request.QueryString = new QueryString("?name=test");
            //controller.ControllerContext.HttpContext = httpContext;
            //controller.Auth = new AuthObj() { AccountId = new Guid() };
            //// controller.Request.Path = new PathString("/");

            //await controller.GetAsync();

        }
    }
}