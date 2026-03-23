using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Contract.Xml;

namespace Manager.Clients;

public class WorkerClient(IHttpClientFactory clientFactory)
{
    // public async Task SendTaskAsync(string workerUrl, WorkerTaskRequest request)
    // {
    //     var client = clientFactory.CreateClient();
    //     var serializer = new XmlSerializer(typeof(WorkerTaskRequest)); 
    //     var settings = new XmlWriterSettings
    //     {
    //         Encoding = Encoding.UTF8,
    //         OmitXmlDeclaration = false
    //     };
    //     using var stringWriter = new StringWriter();
    //     serializer.Serialize(stringWriter, request);
    //     var xml = stringWriter.ToString();
    //     var content = new StringContent(xml, Encoding.UTF8, "application/xml");
    //     var response = await client.PostAsync($"{workerUrl}/internal/api/worker/hash/crack/task", content);
    //     response.EnsureSuccessStatusCode();
    // }
    public async Task SendTaskAsync(string workerUrl, WorkerTaskRequest request)
    {
        var client = clientFactory.CreateClient();
        var serializer = new XmlSerializer(typeof(WorkerTaskRequest));
        using var stream = new MemoryStream();
        var settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };
        using (var writer = XmlWriter.Create(stream, settings))
        {
            serializer.Serialize(writer, request);
        }
        var xml = Encoding.UTF8.GetString(stream.ToArray());
        var content = new StringContent(xml, Encoding.UTF8, "application/xml");
        await client.PostAsync(
            $"{workerUrl}/internal/api/worker/hash/crack/task",
            content);
        // response.EnsureSuccessStatusCode();
    }
}