using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Acies.Docs.Api.Tests
{
    public class UseCaseTests
    {
        private readonly HttpClient _httpClient;

        public UseCaseTests()
        {
            var webAppFactory = new WebApplicationFactory<LocalEntryPoint>();
            _httpClient = webAppFactory.CreateDefaultClient();
        }

        [Trait("Category", "DevIntegration")]
        [Fact]
        public async Task GetDocumentByTags_GetOne()
        {

        }

    }
}
