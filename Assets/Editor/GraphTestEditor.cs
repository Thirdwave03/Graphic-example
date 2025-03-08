using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GraphTest))]
public class GraphTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Display default Inspector UIs
        DrawDefaultInspector();

        if (GUILayout.Button("Reset"))
        {
            var graphTest = (GraphTest)target;
            graphTest.CallAlgorithm();
        }
    }
}
