using System.Xml;
using System.Xml.Serialization;

public class ConfigOptionsData
{
    // set witch options can be saved and their default values
    [XmlAttribute("sfx")]
    public float sfxVolume = 3f;
    [XmlAttribute("voices")]
    public float voicesVolume = 4f;
    [XmlAttribute("difficulty")]
    public string difficulty = "Normal";
}
