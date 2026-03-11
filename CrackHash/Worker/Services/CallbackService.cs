using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Contract.Xml;

namespace Worker.Services;

public class CallbackService(IHttpClientFactory httpClientFactory)
{
    // public async Task SendResultAsync(WorkerTaskResponse response)
    // {
    //     var client = httpClientFactory.CreateClient();
    //     var serializer = new XmlSerializer(typeof(WorkerTaskResponse));
    //     var settings = new XmlWriterSettings
    //     {
    //         Encoding = Encoding.UTF8,
    //         OmitXmlDeclaration = false
    //     };
    //     using var stringWriter = new StringWriter();
    //     serializer.Serialize(stringWriter, response);
    //     var xml = stringWriter.ToString();
    //     var content = new StringContent(xml, Encoding.UTF8, "application/xml");
    //     await client.PatchAsync("http://manager:8080/internal/api/manager/hash/crack/request", content);
    // }
    
    public async Task SendResultAsync(WorkerTaskResponse response)
    {
        var client = httpClientFactory.CreateClient();
        var serializer = new XmlSerializer(typeof(WorkerTaskResponse));
        using var stream = new MemoryStream();
        var settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };
        using (var writer = XmlWriter.Create(stream, settings))
        {
            serializer.Serialize(writer, response);
        }
        var xml = Encoding.UTF8.GetString(stream.ToArray());
        var content = new StringContent(xml, Encoding.UTF8, "application/xml");
        await client.PatchAsync(
            "http://manager:8080/internal/api/manager/hash/crack/request",
            content);
    }
}