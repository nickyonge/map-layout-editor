using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataManager))]
public class DataManagerEditor : DataDownloaderEditor
{

    private DataManager dataManager;

    protected override void OnEnable()
    {
        dataManager = (DataManager)target;
        base.OnEnable();
    }

    public override void OnInspectorGUI()
    {

        base.OnInspectorGUI();

        GUILayout.Space(5);

        if (Btn("Load Data Files"))
        {
            dataManager.LoadAllDataFiles();
        }

        if (Btn("Generate All Data")) {
            dataManager.GenerateNewEntries();
        }

        GUILayout.BeginHorizontal();
        if (Btn("Generate Countries")) {
            dataManager.GenerateNewCountryEntries();
        }
        if (Btn("Generate Cities")) {
            dataManager.GenerateNewCityEntries();
        }
        GUILayout.EndHorizontal();

    }

    private bool Btn(string label) {
        return GUILayout.Button(label);
    }

}