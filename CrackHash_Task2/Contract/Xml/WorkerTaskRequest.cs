namespace Contract.Xml;

public class WorkerTaskRequest
{
    public string RequestId { get; set; }
    public int PartNumber { get; set; }
    public int PartCount { get; set; }
    public string Hash { get; set; }
    public int MaxLength { get; set; }
    public Alphabet Alphabet { get; set; }
}