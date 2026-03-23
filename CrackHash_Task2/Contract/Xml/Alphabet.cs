using System.Xml.Serialization;

namespace Contract.Xml;

public class Alphabet
{
    [XmlElement("symbols")]
    public List<string> Symbols { get; set; } = new List<string>();
}