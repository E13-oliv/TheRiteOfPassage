using UnityEngine;

public class TerrainAmbiance : MonoBehaviour
{
    enum StormLevels
    {
        None = 0,
        Quite = 1,
        Normal = 2,
        Stormy = 3
    };
    [Header("Tempest level")]
    [SerializeField]
    private StormLevels stormLevels;
    private string stormLevel;

    public string getStormLevel()
    {
        stormLevel = stormLevels.ToString();

        return stormLevel;
    }
}