using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataManager))]
public class DataManagerEditor : DataDownloaderEditor
{

    private DataManager dataManager;

    private SerializedProperty _scriptReference;
    private SerializedProperty _loadingParams;
    private SerializedProperty _cityDatasets;
    private SerializedProperty _countryDatasets;
    private SerializedProperty _continentDatasets;
    private SerializedProperty _exportSourceParams;

    private GUIStyle _foldoutHeader;

    private static bool showDatasets;
    private static bool showExports;

    protected override void OnEnable()
    {
        try
        {
            dataManager = (DataManager)target;
            _scriptReference = serializedObject.FindProperty("m_Script");
            dataManager.Initialize();
            _foldoutHeader = new GUIStyle(EditorStyles.foldoutHeader);
            _loadingParams = serializedObject.FindProperty("loadingParams");
            _cityDatasets = serializedObject.FindProperty("cityDatasets");
            _countryDatasets = serializedObject.FindProperty("countryDatasets");
            _continentDatasets = serializedObject.FindProperty("continentDatasets");
            _exportSourceParams = serializedObject.FindProperty("exportSourceParams");
            base.OnEnable();
        }
        catch
        {
            // error, do nothing except close foldouts
            showDatasets = false;
            showExports = false;
        }
    }

    public override void OnInspectorGUI()
    {
        // show script property up top 
        bool en = GUI.enabled;// preserve GUI.enabled state 
        GUI.enabled = false;
        EditorGUILayout.PropertyField(_scriptReference);
        GUI.enabled = en;

        if (Section("Datasets", showDatasets, out showDatasets))
        {
            try
            {
                EditorGUILayout.PropertyField(_loadingParams);
            } catch {
                OnEnable();
                OnInspectorGUI();
                return;
            }

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (Btn("Load Dataset"))
            {
                dataManager.LoadAllDataFiles();
            }
            if (Btn("Clear Datasets", 120))
            {
                dataManager.ClearDataFiles();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.PropertyField(_cityDatasets);
            EditorGUILayout.PropertyField(_countryDatasets);
            EditorGUILayout.PropertyField(_continentDatasets);

            EndSection();
        }

        if (Section("Export Data", showExports, out showExports))
        {
            try
            {
                EditorGUILayout.PropertyField(_exportSourceParams);
            } catch {
                OnEnable();
                OnInspectorGUI();
                return;
            }

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (Btn("Generate Export Data"))
            {
                dataManager.GenerateNewEntries();
            }
            if (Btn("Clear ExpData", 100))
            {
                dataManager.ClearDataEntries();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (Btn("Continents"))
            {
                dataManager.GenerateNewContinentEntries();
            }
            if (Btn("Countries"))
            {
                dataManager.GenerateNewCountryEntries();
            }
            if (Btn("Cities"))
            {
                dataManager.GenerateNewCityEntries();
            }
            GUILayout.EndHorizontal();

            EndSection();
        }

        serializedObject.ApplyModifiedProperties();


        DrawPropertiesExcluding(serializedObject, new string[] {
                "m_Script", "loadingParams",
                "cityDatasets", "countryDatasets", "continentDatasets",
                "exportSourceParams", });

    }

    private bool Btn(string label, int width = 0)
    {
        if (width > 0)
        {
            return GUILayout.Button(label, GUILayout.MaxWidth(width));
        }
        return GUILayout.Button(label);
    }

    private bool Section(string label, bool stateIn, out bool stateOut)
    {
        GUILayout.Space(5);
        stateOut = EditorGUILayout.Foldout(
            stateIn, " " + label, true, _foldoutHeader);
        Line();
        if (stateOut) { EditorGUI.indentLevel++; }
        return stateOut;
    }
    private void EndSection() { GUILayout.Space(10); EditorGUI.indentLevel--; }
    private void Header(string label, bool withLine = true)
    {
        GUILayout.Space(5);
        EditorGUILayout.LabelField(" " + label, EditorStyles.boldLabel);
        if (withLine) { Line(); }
    }
    private void Line()
    {
        GUILayout.Space(-9);
        bool en = GUI.enabled;
        GUI.enabled = false;
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUI.enabled = en;
    }

}