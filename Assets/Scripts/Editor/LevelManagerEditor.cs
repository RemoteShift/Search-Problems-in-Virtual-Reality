using Search.Levels;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default Inspector (serialized fields)
        DrawDefaultInspector();

        // Get a reference to the target script
        LevelManager myScript = LevelManager.Instance;

        // Draw a button
        if (GUILayout.Button("Start Search"))
        {
            myScript.Start();
            Repaint();
        }

        if(myScript.isStepped)
        {
            if (GUILayout.Button("Next Step"))
            {
                myScript.searchAlgorithm.AdvanceStep(); 
            }
        }
    }
}