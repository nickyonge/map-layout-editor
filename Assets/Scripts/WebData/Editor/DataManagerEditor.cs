using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataManager))]
public class DataManagerEditor : DataDownloaderEditor
{

    private DataManager dataManager;

    private SerializedProperty _scriptReferece;
    private SerializedProperty _loadingParams;

    protected override void OnEnable()
    {
        dataManager = (DataManager)target;
        dataManager.Initialize();
        _scriptReferece = serializedObject.FindProperty("m_Script");
        _loadingParams = serializedObject.FindProperty("loadingParams");
        base.OnEnable();
    }

    public override void OnInspectorGUI()
    {
        // show script property up top 
        bool en = GUI.enabled;// preserve GUI.enabled state 
        GUI.enabled = false;
        EditorGUILayout.PropertyField(_scriptReferece);
        GUI.enabled = en;

        // show property
        EditorGUILayout.PropertyField(_loadingParams);

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        if (Btn("Load Data Files"))
        {
            dataManager.LoadAllDataFiles();
        }
        if (Btn("Clear Files", 90)) {
            dataManager.ClearDataFiles();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (Btn("Generate All Data"))
        {
            dataManager.GenerateNewEntries();
        }
        if (Btn("Clear Data", 90)) {
            dataManager.ClearDataEntries();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (Btn("Generate Countries"))
        {
            dataManager.GenerateNewCountryEntries();
        }
        if (Btn("Generate Cities"))
        {
            dataManager.GenerateNewCityEntries();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(5);

        serializedObject.ApplyModifiedProperties();


            DrawPropertiesExcluding(serializedObject, new string[] { 
                "m_Script", "loadingParams" });

    }

    private bool Btn(string label, int width = 0)
    {
        if (width > 0) {
            return GUILayout.Button(label, GUILayout.MaxWidth(width));
        }
        return GUILayout.Button(label);
    }

}