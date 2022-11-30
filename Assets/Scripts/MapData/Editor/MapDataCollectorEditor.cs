using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapDataCollector))]
public class MapDataCollectorEditor : Editor
{
    private MapDataCollector script;

    private void OnEnable()
    {
        script = (MapDataCollector)target;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Collect Data"))
        {
            script.CollectData();
        }

        DrawDefaultInspector();

    }
}
