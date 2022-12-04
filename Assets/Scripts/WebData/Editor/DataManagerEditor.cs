using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataManager))]
public class DataManagerEditor : DataDownloaderEditor
{

    private DataManager dataManager;

    private void OnEnable()
    {
        dataManager = (DataManager)target;
    }

    public override void OnInspectorGUI()
    {

        base.OnInspectorGUI();

        GUILayout.Space(5);

        if (GUILayout.Button("Load All Data"))
        {
            dataManager.LoadAllDataFiles();
        }
    }

}