using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pathfinder))]
public class PathfinderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Pathfinder myTarget = (Pathfinder)target;

        if (GUILayout.Button("Generate Network"))
        {
            myTarget.GenerateNetwork();
        }
    }
}
