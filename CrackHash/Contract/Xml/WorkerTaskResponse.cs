using System.Xml.Serialization;

namespace Contract.Xml;

[XmlRoot("CrackHashWorkerResponse", Namespace = "http://ccfit.nsu.ru/schema/crack-hash-response")]
public class WorkerTaskResponse
{
    public string RequestId { get; set; }
    public int PartNumber { get; set; }
    public Answers Answers { get; set; }
}