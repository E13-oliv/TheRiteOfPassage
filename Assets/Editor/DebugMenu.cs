using UnityEngine;
using UnityEditor;

public static class DebugMenu
{
    [MenuItem("Debug/Print Global Position & Map Position")]
    public static void PrintGlobalPosition()
    {
        float mapFactor = 0.177777f;

        if (Selection.activeGameObject != null)
        {
            Debug.Log(Selection.activeGameObject.name + " is at " + Selection.activeGameObject.transform.position);
            Debug.Log("Map position _ x : " + Selection.activeGameObject.transform.position.x * mapFactor + " | y : " + Selection.activeGameObject.transform.position.z * mapFactor);
        }
    }
}