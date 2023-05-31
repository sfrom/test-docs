using Acies.Docs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Acies.Docs.Services.Tests
{
    public class JsonSerializerTests
    {
        [Fact]
        public void Serialize_StaticAndPdf_Ok()
        {
            var d = new List<TemplateOutputBase>
            {
                new PdfOutput
                {
                    Name="Peter",
                    Layout= new Layout
                    {
                        Assets=new List<Asset>
                        {
                            new Asset
                            {
                                Path="/images/1.png",
                            }
                        },
                        Body=new TemplateRef
                        {
                            ContentFileName="C:\\sdfsdf",
                            Content="<head></head>",
                        }
                    }
                },
                new StaticOutput
                {
                    Asset=new Asset
                    {
                            Path="/images/2.png",
                    },
                    Name="Static type",
                    Tags = new Dictionary<string, string>
                    {
                        {"Type","Order" },
                        {"SubType","EndCostumerNr" },
                        {"Number","1214" },
                    },
                }
            };

            var j = new JsonSerializerService();
            var r = j.Serialize(d);

            var rr = j.Deserialize<List<TemplateOutputBase>>(r);

            Assert.NotNull(r);
            Assert.Contains("\"pdf\"", r);
            Assert.Contains("\"static\"", r);
        }

        [Fact]
        public void Serialize_NoSupportedOutput_Exception()
        {
            var d = new List<TemplateOutputBase>
            {
                new PdfOutput
                {
                    Name="Peter",
                    Layout = new Layout
                    {
                        Assets=new List<Asset>
                        {
                            new Asset
                            {
                                Path="/images/1.png",
                            }
                        },
                    },
                },
                new NotSupportedOutput
                {
                    Name =  "NotSupportedOutput",
                }
            };

            var j = new JsonSerializerService();
            var a = () => j.Serialize(d);

            var r = Assert.Throws<NotSupportedException>(a);
            Assert.True(r.Message.StartsWith("Type NotSupportedOutput not supported for serialization. The unsupported member type is located"), "NotSupportedException message should start with: Type NotSupportedOutput not supported for serialization. The unsupported member type is located");
        }

        [Fact]
        public void Serialize_NoElements_Ok()
        {
            var d = new List<TemplateOutputBase>();

            var j = new JsonSerializerService();
            var r = j.Serialize(d);
        }
    }

    public class NotSupportedOutput : TemplateOutputBase
    {
        public override OutputTypes Type => OutputTypes.None;
    }
}