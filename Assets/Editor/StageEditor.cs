using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Stage))]
public class StageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    
        if(GUILayout.Button("Reset"))
        {
            var stage = (Stage)target;
            stage.OnReset();    
        }
    
    
    
    
    
    }




}
