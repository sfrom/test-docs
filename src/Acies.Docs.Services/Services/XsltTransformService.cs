using Acies.Docs.Models;
using Newtonsoft.Json;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace Acies.Docs.Services
{
    public class XsltTransformService : ITransformService
    {
        public string Transform(string data, Stream stream)
        {
            string xmlInput = ObjectIntoXML(data);
            return ApplyXSLT(xmlInput, stream);
        }

        private string ObjectIntoXML1(object obj)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
            StringWriter stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, obj);
            return stringWriter.ToString();
        }

        private string? ObjectIntoXML(string data)
        {
            XmlDocument? doc = (XmlDocument?)JsonConvert.DeserializeXmlNode(data, "Root");
            return doc?.OuterXml;
        }

        private string ApplyXSLT(string xmlInput, Stream stream)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlInput);

            XslCompiledTransform xsltTransform = new XslCompiledTransform();
            XmlReader reader = XmlReader.Create(stream);
            xsltTransform.Load(reader);

            MemoryStream memoreStream = new MemoryStream();
            xsltTransform.Transform(xmlDocument, null, memoreStream);
            memoreStream.Position = 0;

            StreamReader streamReader = new StreamReader(memoreStream);
            return streamReader.ReadToEnd();
        }
    }
}