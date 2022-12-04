using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataDownloader))]
public class DataDownloaderEditor : Editor {

    private DataDownloader dataDownloader;

    protected virtual void OnEnable() {
        dataDownloader = (DataDownloader)target;
    }

    public override void OnInspectorGUI() {

        base.OnInspectorGUI();

        GUILayout.Space(5);

        if (GUILayout.Button("Test Download")) {
            dataDownloader.TestDownload();
        }
    }

}