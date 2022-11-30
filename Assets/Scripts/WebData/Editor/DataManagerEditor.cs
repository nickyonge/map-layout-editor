using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(DataManager))]
public class DataManagerEditor : Editor {

    private DataManager script;

    private void OnEnable() {
        script = (DataManager)target;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Test Download")) {
            script.TestDownload();
        }
    }

}