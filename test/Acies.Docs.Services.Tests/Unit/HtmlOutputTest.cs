using Acies.Docs.Models;
using Acies.Docs.Services.Generators;
using System.Threading.Tasks;
using System.Dynamic;
using Xunit;
using Moq;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;

namespace Acies.Docs.Services.Tests
{
    public class HtmlOutputTest
    {
        [Fact]
        public async Task GenerateHtml()
        {
            Mock<IReadOnlyStreamRepository> templateStreamMock = new();
            Mock<IWriteOnlyStreamRepository> writeOnlyStreamMock = new();
            Mock<ITransformService> transformerMock = new();
            Mock<IContentRepository> contentRepositoryMock = new();
            DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);

            var documentVersion = new DocumentVersion
            {
                Id = "D737800E-2301-4696-8B16-C8BCFBB323E4",
                Input = "",
                Output = new List<DocumentOutput>
                {
                    new DocumentOutput {
                        Id = "8D00A9C8-83A2-49B2-B29C-AB739E2F768A",
                        Status = Status.Pending,
                        Type = OutputTypes.Pdf,
                    },
                    new DocumentOutput {
                        Id = "092C3463-89AA-413F-A6B8-739F969842E4",
                        Status = Status.Pending,
                        Type = OutputTypes.Html,
                    },
                },
                Version = 2,
            };

            var template = new Template
            {
                Id = "B1363645-742A-4AC9-AA3E-A064EB70F028",
                Name = "My test template",
                Managed = true,
                CreatedAt = dto.ToUnixTimeSeconds(),
                Version = 1,
            };

            var templateVersion = new TemplateVersion
            {
                Id = "B1363645-742A-4AC9-AA3E-A064EB70F028",
                Version = 1,
                Name = "My template version",
                CreatedAt = template.CreatedAt,
                Input = new TemplateInput(),
                Outputs = new List<TemplateOutputBase>
                {
                    new PdfOutput
                    {
                        Layout = new Layout
                        {
                            Body = new TemplateRef
                            {
                                Content="<html></html>",
                            },
                        }
                    },
                },
            };

            GeneratorInput generatorInput = new GeneratorInput
            {
                DocumentVersion = documentVersion,
                Template = template,
                TemplateVersion = templateVersion,
            };

            var s = new HtmlRenderer(templateStreamMock.Object, transformerMock.Object, contentRepositoryMock.Object);

            await s.GenerateAsync<PdfOutput>(generatorInput);
        }



        // [Fact]
        // public async Task GenerateHtml_BodyFromOutputContent()
        // {
        //     //Arrange
        //     const string outputFileName = "/myoutput.pdf";
        //     const string content = "<html></html>";

        //     byte[] byteArray = Encoding.UTF8.GetBytes(content);
        //     var stream = new MemoryStream(byteArray);

        //     Mock<IReadOnlyStreamRepository> templateStreamMock = new();

        //     Mock<ITransformService> transformMock = new();
        //     transformMock.Setup(c => c.Transform(It.IsAny<string>(), It.IsAny<Stream>())).Returns(content);

        //     Mock<IContentRepository> contentRepositoryMock = new();
        //     contentRepositoryMock.Setup(c => c.GetContentAsync(content)).ReturnsAsync(content);

        //     Mock<IWriteOnlyStreamRepository> writeOnlyStreamMock = new();

        //     var templateVersion = new TemplateVersion
        //     {
        //         Outputs = new List<TemplateOutputBase>
        //         {
        //             new PdfOutput
        //             {
        //                 Layout = new Layout
        //                 {
        //                     Body = new TemplateRef
        //                     {
        //                         Content = content,
        //                     },
        //                 },
        //             },
        //         },
        //     };

        //     GeneratorInput generatorInput = new GeneratorInput
        //     {
        //         DocumentVersion = new DocumentVersion(),
        //         Template = new Template(),
        //         TemplateVersion = templateVersion,
        //     };

        //     var s = new HtmlRenderer(templateStreamMock.Object, transformMock.Object, contentRepositoryMock.Object);

        //     //Act
        //     await s.GenerateAsync<PdfOutput>(generatorInput);

        //     //Assert
        //     templateStreamMock.Verify(c => c.GetStreamAsync(It.IsAny<string>()), Times.Never);
        //     transformMock.Verify(c => c.Transform(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);
        //     contentRepositoryMock.Verify(c => c.GetContentAsync(content), Times.Once);
        //     writeOnlyStreamMock.Verify(c => c.WriteAsync(It.IsAny<Stream>(), outputFileName), Times.Once);
        // }


        // [Fact]
        // public async Task GenerateHtml_BodyFromFile()
        // {
        //     //Arrange
        //     const string outputFileName = "/myoutput.pdf";
        //     const string contentFileName = "/template/123.html";
        //     const string content = "<html></html>";

        //     byte[] byteArray = Encoding.UTF8.GetBytes(content);
        //     var stream = new MemoryStream(byteArray);

        //     Mock<IReadOnlyStreamRepository> templateStreamMock = new();
        //     templateStreamMock.Setup(c => c.GetStreamAsync(contentFileName)).ReturnsAsync(stream);

        //     Mock<ITransformService> transformMock = new();
        //     transformMock.Setup(c => c.Transform(It.IsAny<string>(), It.IsAny<Stream>())).Returns(content);

        //     Mock<IContentRepository> contentRepositoryMock = new();
        //     contentRepositoryMock.Setup(c => c.GetContentAsync(content)).ReturnsAsync(content);

        //     Mock<IWriteOnlyStreamRepository> writeOnlyStreamMock = new();

        //     var templateVersion = new TemplateVersion
        //     {
        //         Outputs = new List<TemplateOutputBase>
        //         {
        //             new PdfOutput
        //             {
        //                 Layout = new Layout
        //                 {
        //                     Body = new TemplateRef
        //                     {
        //                         ContentFileName=contentFileName,
        //                     },
        //                 }
        //             },
        //         },
        //     };

        //     GeneratorInput generatorInput = new GeneratorInput
        //     {
        //         DocumentVersion = new DocumentVersion(),
        //         Template = new Template(),
        //         TemplateVersion = templateVersion,
        //     };

        //     var s = new HtmlRenderer(templateStreamMock.Object, transformMock.Object, contentRepositoryMock.Object);

        //     //Act
        //     await s.GenerateAsync<PdfOutput>(generatorInput);

        //     //Assert
        //     templateStreamMock.Verify(c => c.GetStreamAsync(contentFileName), Times.Once);
        //     transformMock.Verify(c => c.Transform(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once);
        //     contentRepositoryMock.Verify(c => c.GetContentAsync(content), Times.Once);
        //     writeOnlyStreamMock.Verify(c => c.WriteAsync(It.IsAny<Stream>(), outputFileName), Times.Once);
        // }

        // [Fact]
        // public async Task GenerateHtml_NoBodyException()
        // {
        //     //Arrange
        //     Mock<IReadOnlyStreamRepository> templateStreamMock = new();
        //     Mock<IWriteOnlyStreamRepository> writeOnlyStreamMock = new();
        //     Mock<ITransformService> transformerMock = new();
        //     Mock<IContentRepository> contentRepositoryMock = new();

        //     var templateVersion = new TemplateVersion
        //     {
        //         Outputs = new List<TemplateOutputBase>
        //         {
        //             new PdfOutput
        //             {
        //                 Layout = new Layout
        //                 {
        //                     Body = new TemplateRef(),
        //                 },
        //             },
        //         },
        //     };

        //     GeneratorInput generatorInput = new GeneratorInput
        //     {
        //         DocumentVersion = new DocumentVersion(),
        //         Template = new Template(),
        //         TemplateVersion = templateVersion,
        //     };

        //     var s = new HtmlRenderer(templateStreamMock.Object, transformerMock.Object, contentRepositoryMock.Object);

        //     //Act
        //     var f = () => s.GenerateAsync<PdfOutput>(generatorInput);

        //     //Assert
        //     var e = await Assert.ThrowsAsync<NullReferenceException>(f);
        //     Assert.Equal("Html body not set for Pdf output.", e.Message);
        // }
    }
}
