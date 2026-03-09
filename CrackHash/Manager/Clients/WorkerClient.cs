using System.Text;
using System.Xml.Serialization;
using Contract.Xml;

namespace Manager.Clients;

public class WorkerClient(IHttpClientFactory clientFactory)
{
    public async Task SendTaskAsync(string workerUrl, WorkerTaskRequest request)
    {
        var client = clientFactory.CreateClient();
        var serializer = new XmlSerializer(typeof(WorkerTaskRequest)); 
        using var stringWriter = new StringWriter();
        serializer.Serialize(stringWriter, request);
        var xml = stringWriter.ToString();
        var content = new StringContent(xml, Encoding.UTF8, "application/xml");
        var response = await client.PostAsync($"{workerUrl}/internal/api/worker/hash/crack/task", content);
        response.EnsureSuccessStatusCode();
    }
}