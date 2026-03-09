using System.Text;
using System.Xml.Serialization;
using Contract.Xml;

namespace Worker.Services;

public class CallbackService(IHttpClientFactory httpClientFactory)
{
    public async Task SendResultAsync(WorkerTaskResponse response)
    {
        var client = httpClientFactory.CreateClient();
        var serializer = new XmlSerializer(typeof(WorkerTaskResponse));
        using var stringWriter = new StringWriter();
        serializer.Serialize(stringWriter, response);
        var xml = stringWriter.ToString();
        var content = new StringContent(xml, Encoding.UTF8, "application/xml");
        await client.PatchAsync("http://manager/internal/api/manager/hash/crack/request", content);
    }
}