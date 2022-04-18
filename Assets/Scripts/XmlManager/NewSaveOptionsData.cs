using System.Xml;
using System.Xml.Serialization;

public class NewSaveOptionsData
{
    // set witch options can be saved and their default values
    [XmlAttribute("energy")]
    public float energy = 100f;
    [XmlAttribute("withDepa")]
    public bool withDepa = false;
    [XmlAttribute("time")]
    public float time = 0.33f;
    [XmlAttribute("gameDuration")]
    public float gameDuration = 0.0f;
    [XmlAttribute("fires")]
    public bool[] fires = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
    [XmlAttribute("playerPositionX")]
    public float playerPositionX = 1145.8f;
    [XmlAttribute("playerPositionY")]
    public float playerPositionY = 11.0f;
    [XmlAttribute("playerPositionZ")]
    public float playerPositionZ = -1263.6f;
    [XmlAttribute("stelePieces")]
    public bool[] stelePieces = new bool[] { false, false, false, false, false, false };
    [XmlAttribute("onStelePieces")]
    public bool[] onStelePieces = new bool[] { false, false, false, false, false, false };
    [XmlAttribute("mapCamps")]
    public bool[] mapCamps = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false };
    [XmlAttribute("mapCampsAlpha")]
    public bool[] mapCampsAlpha = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false };
    [XmlAttribute("mapLandMarks")]
    public bool[] mapLandMarks = new bool[] { false, false, false, false, false, false };
    [XmlAttribute("mapStelePieces")]
    public bool[] mapStelePieces = new bool[] { false, false, false, false, false, false };
    [XmlAttribute("mapPiecesOnStele")]
    public bool[] mapPiecesOnStele = new bool[] { false, false, false, false, false, false };
    // eventVoiceOverOncePlayed IDs
    //  0 : depaPop
    //  1 : storm
    //  2 : dawn
    //  3 : dusk
    //  4 : stelePiecesPickUp
    //  5 : runningWithDepa
    //  6 : depaPopOut
    //  7 : villageClose
    //  8 : villageCloser
    //  9 : completeStele
    // 10 : nightOutOfCamp
    // 11 : backPack
    // 12 : easterEgg
    // 13 : easterEggAction
    [XmlAttribute("eventVoiceOverOncePlayed")]
    public bool[] eventVoiceOverOncePlayed = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
}