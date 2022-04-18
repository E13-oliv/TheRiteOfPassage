using UnityEngine;

public class DepaPath : MonoBehaviour
{
    [Header("Path Points")]
    [SerializeField]
    private GameObject[] points;

    [Header("Path Start & End")]
    [SerializeField]
    private int startPoint;
    [SerializeField]
    private int endPoint;

    // public get methods
    public GameObject[] GetPoints()
    {
        return points;
    }

    public int GetStartPoint()
    {
        return startPoint;
    }

    public int GetEndPoint()
    {
        return endPoint;
    }
}