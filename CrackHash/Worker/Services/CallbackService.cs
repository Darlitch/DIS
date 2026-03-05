using System.Xml.Serialization;
using Contract.Xml;

namespace Worker.Services;

public class CallbackService(IHttpClientFactory httpClientFactory)
{
    public async Task SendResultAsync(WorkerTaskResponse result)
    {
        var client = httpClientFactory.CreateClient();
        var serializer = new XmlSerializer(typeof(WorkerTaskResponse));
    }
}