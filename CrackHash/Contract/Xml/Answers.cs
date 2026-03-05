using System.Xml.Serialization;

namespace Contract.Xml;

public class Answers
{
    [XmlElement("words")]
    public List<string> Words { get; set; } = new();
}