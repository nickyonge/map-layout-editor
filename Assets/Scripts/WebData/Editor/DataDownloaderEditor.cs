using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataDownloader))]
public class DataDownloaderEditor : Editor {

    private DataDownloader script;

    private void OnEnable() {
        script = (DataDownloader)target;
    }

    public override void OnInspectorGUI() {

        base.OnInspectorGUI();

        GUILayout.Space(5);

        if (GUILayout.Button("Test Download")) {
            script.TestDownload();
        }
    }

}