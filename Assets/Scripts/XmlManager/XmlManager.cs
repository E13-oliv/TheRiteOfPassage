using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class XmlManager
{
    private ConfigData configData = new ConfigData();
    private readonly string configPath = "config";
    private readonly string configFilename = "config.xml";

    private SaveData saveData = new SaveData();
    private readonly string savePath = "save";
    private readonly string saveFilename = "save.xml";

    private NewSaveData newSaveData = new NewSaveData();

    private string filePath;
    private string fileName;

    //private static readonly string PrivateKey = SystemInfo.deviceUniqueIdentifier.Replace("-", string.Empty);
    private static readonly string PrivateKey = "theRiteOfPassage";

    // gameplay public methods
    public bool getNewGame()
    {
        return saveData.saveOptionsData.newGame;
    }

    public void setNewGame(bool newGame)
    {
        saveData.saveOptionsData.newGame = newGame;
    }

    public float getPlayerEnergy()
    {
        return saveData.saveOptionsData.energy;
    }

    public void setPlayerEnergy(float energy)
    {
        saveData.saveOptionsData.energy = energy;
    }

    public bool getWithDepa()
    {
        return saveData.saveOptionsData.withDepa;
    }

    public void setWithDepa(bool withDepa)
    {
        saveData.saveOptionsData.withDepa = withDepa;
    }

    public float getSaveTime()
    {
        return saveData.saveOptionsData.time;
    }

    public float getNewSaveTime()
    {
        return newSaveData.newSaveOptionsData.time;
    }

    public void setSaveTime(float time)
    {
        saveData.saveOptionsData.time = time;
    }

    public float getGameDuration()
    {
        return saveData.saveOptionsData.gameDuration;
    }

    public void setGameDuration(float duration)
    {
        saveData.saveOptionsData.gameDuration = duration;
    }

    public Vector3 getPlayerPosition()
    {
        float posX = saveData.saveOptionsData.playerPositionX;
        float posY = saveData.saveOptionsData.playerPositionY;
        float posZ = saveData.saveOptionsData.playerPositionZ;

        Vector3 playerPosition = new Vector3(posX, posY, posZ);

        return playerPosition;
    }

    public void setPlayerPosition(Vector3 playerPosition)
    {
        saveData.saveOptionsData.playerPositionX = playerPosition.x;
        saveData.saveOptionsData.playerPositionY = playerPosition.y;
        saveData.saveOptionsData.playerPositionZ = playerPosition.z;
    }

    // prefereances public methods
    public void setSFXVolume(float sfxVolume)
    {
        configData.configOptionsData.sfxVolume = sfxVolume;
    }

    public float getSFXVolume()
    {
        return configData.configOptionsData.sfxVolume;
    }

    public void setVoicesVolume(float voicesVolume)
    {
        configData.configOptionsData.voicesVolume = voicesVolume;
    }

    public string getDifficulty()
    {
        return configData.configOptionsData.difficulty;
    }

    public void setDifficulty(string difficulty)
    {
        configData.configOptionsData.difficulty = difficulty;
    }

    public float getVoicesVolume()
    {
        return configData.configOptionsData.voicesVolume;
    }

    public bool getFireStatus(int fireID)
    {
        return saveData.saveOptionsData.fires[fireID];
    }

    public void setFireStatus(int fireID)
    {
        saveData.saveOptionsData.fires[fireID] = true;
    }

    public bool[] getStelePiecesStatus()
    {
        return saveData.saveOptionsData.stelePieces;
    }

    public bool getSteleStatus(int stelePieceID)
    {
        return saveData.saveOptionsData.stelePieces[stelePieceID];
    }

    public void setSteleStatus(int stelePieceID)
    {
        saveData.saveOptionsData.stelePieces[stelePieceID] = true;
    }

    public bool getOnSteleStatus(int stelePieceID)
    {
        return saveData.saveOptionsData.onStelePieces[stelePieceID];
    }

    public void setOnSteleStatus(int stelePieceID)
    {
        saveData.saveOptionsData.onStelePieces[stelePieceID] = true;
    }

    public void setMapCamp(int mapCampID)
    {
        saveData.saveOptionsData.mapCamps[mapCampID] = true;
    }

    public bool[] getMapCamps()
    {
        return saveData.saveOptionsData.mapCamps;
    }

    public bool getMapCamp(int mapCampID)
    {
        return saveData.saveOptionsData.mapCamps[mapCampID];
    }

    public void setMapCampAlpha(int mapCampID)
    {
        saveData.saveOptionsData.mapCampsAlpha[mapCampID] = true;
    }

    public bool[] getMapCampsAlpha()
    {
        return saveData.saveOptionsData.mapCampsAlpha;
    }

    public bool getMapCampAlpha(int mapCampID)
    {
        return saveData.saveOptionsData.mapCampsAlpha[mapCampID];
    }

    public void setMapStelePieces(int mapSteleID)
    {
        saveData.saveOptionsData.mapStelePieces[mapSteleID] = true;
    }

    public bool[] getMapStelePieces()
    {
        return saveData.saveOptionsData.mapStelePieces;
    }

    public bool getMapStelePieces(int mapSteleID)
    {
        return saveData.saveOptionsData.mapStelePieces[mapSteleID];
    }

    public void setMapPiecesOnStele(int mapSteleID)
    {
        saveData.saveOptionsData.mapPiecesOnStele[mapSteleID] = true;
    }

    public bool[] getMapPiecesOnStele()
    {
        return saveData.saveOptionsData.mapPiecesOnStele;
    }

    public bool getMapPiecesOnStele(int mapSteleID)
    {
        return saveData.saveOptionsData.mapPiecesOnStele[mapSteleID];
    }

    public void setMapLandMark(int mapLandMarkID)
    {
        saveData.saveOptionsData.mapLandMarks[mapLandMarkID] = true;
    }

    public bool[] getMapLandMarks()
    {
        return saveData.saveOptionsData.mapLandMarks;
    }

    public bool getMapLandMark(int mapLandMarkID)
    {
        return saveData.saveOptionsData.mapLandMarks[mapLandMarkID];
    }

    public void setEventVoiceOverOncePlayed(int eventVoiceOverOncePlayedID)
    {
        saveData.saveOptionsData.eventVoiceOverOncePlayed[eventVoiceOverOncePlayedID] = true;
    }

    public bool getEventVoiceOverOncePlayed(int eventVoiceOverOncePlayedID)
    {
        return saveData.saveOptionsData.eventVoiceOverOncePlayed[eventVoiceOverOncePlayedID];
    }
    //eventVoiceOverOncePlayed

    public bool doSaveExists()
    {
        string directory = Path.Combine(Application.persistentDataPath, savePath);

        string path = Path.Combine(directory, saveFilename);

        // if file doesn't exist –> create it
        if (File.Exists(path))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // set type of file needed
    private void setFileType(string fileType)
    {
        if (fileType == "config")
        {
            filePath = configPath;
            fileName = configFilename;
        }
        else
        {
            filePath = savePath;
            fileName = saveFilename;
        }
    }

    // xml file write method
    public void saveFile(string fileType)
    {
        setFileType(fileType);

        string directory = Path.Combine(Application.persistentDataPath, filePath);

        // if directory does not exist –> create it
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string path = Path.Combine(directory, fileName);

        if (fileType == "config")
        {
            // write through xml file
            XmlSerializer serializer = new XmlSerializer(typeof(ConfigData));
            FileStream stream = new FileStream(path, FileMode.Create);
            serializer.Serialize(stream, configData);
            stream.Close();
        }
        else
        {
            // write through xml file
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            FileStream stream = new FileStream(path, FileMode.Create);
            serializer.Serialize(stream, saveData);
            stream.Close();
        }
    }

    // if new save –> overwrite exsting with defaut values
    public void newSaveFile()
    {
        string directory = Path.Combine(Application.persistentDataPath, savePath);

        // if directory does not exist –> create it
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string path = Path.Combine(directory, saveFilename);

        // write through xml file
        XmlSerializer serializer = new XmlSerializer(typeof(NewSaveData));
        FileStream stream = new FileStream(path, FileMode.Create);
        serializer.Serialize(stream, newSaveData);

        stream.Close();
    }

    // xml file read method
    public void loadFile(string fileType)
    {
        setFileType(fileType);

        string directory = Path.Combine(Application.persistentDataPath, filePath);

        // if directory does not exist –> create it
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string path = Path.Combine(directory, fileName);

        // if file doesn't exist –> create it
        if (!File.Exists(path))
        {
            saveFile(fileType);
        }

        if (fileType == "config")
        {
            // read through xml file
            XmlSerializer serializer = new XmlSerializer(typeof(ConfigData));
            StreamReader fileStream = new StreamReader(path);
            configData = serializer.Deserialize(fileStream) as ConfigData;
            fileStream.Close();
        }
        else
        {
            // read through xml file
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            StreamReader fileStream = new StreamReader(path);
            saveData = serializer.Deserialize(fileStream) as SaveData;
            fileStream.Close();
        }
    }

    public void deleteFile(string fileType)
    {
        setFileType(fileType);
        string directory = Path.Combine(Application.persistentDataPath, filePath);
        string path = Path.Combine(directory, fileName);

        File.Delete(path);
    }
}