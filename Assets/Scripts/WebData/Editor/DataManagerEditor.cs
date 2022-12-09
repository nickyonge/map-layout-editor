using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataManager))]
public class DataManagerEditor : DataDownloaderEditor
{
    private const int BTN_HEIGHT_BIG = 30;
    private const int BTN_HEIGHT_SMALL = 20;

    private DataManager dataManager;

    private SerializedProperty _scriptReference;
    private SerializedProperty _loadingParams;
    private SerializedProperty _cityDatasets;
    private SerializedProperty _countryDatasets;
    private SerializedProperty _continentDatasets;
    private SerializedProperty _internalReferenceParams;
    private SerializedProperty _exportSourceParams;

    private SerializedProperty _internalMapCities;
    private SerializedProperty _internalMapCountries;
    private SerializedProperty _internalMapContinents;
    private SerializedProperty _referenceCities;
    private SerializedProperty _referenceCountries;
    private SerializedProperty _referenceContinents;


    private GUIStyle _foldoutHeader;

    private static bool showLoadDatasets;
    private static bool showInternalRefs;
    private static bool showExportData;


    private static bool _repeatMethodFailsafe = false;

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
            _internalReferenceParams = serializedObject.FindProperty("referenceSourceFiles");
            _exportSourceParams = serializedObject.FindProperty("exportSourceParams");
            _internalMapCities = serializedObject.FindProperty("internalMapCities");
            _internalMapCountries = serializedObject.FindProperty("internalMapCountries");
            _internalMapContinents = serializedObject.FindProperty("internalMapContinents");
            _referenceCities = serializedObject.FindProperty("referenceCities");
            _referenceCountries = serializedObject.FindProperty("referenceCountries");
            _referenceContinents = serializedObject.FindProperty("referenceContinents");
            base.OnEnable();
        }
        catch
        {
            // error, do nothing except close foldouts
            showLoadDatasets = false;
            showInternalRefs = false;
            showExportData = false;
        }
    }

    public override void OnInspectorGUI()
    {
        // editor reload hotfix, wrap entire OnInspectorGUI in a try/catch
        try
        {
            DataManagerInspectorGUI();
        }
        catch
        {
            if (!_repeatMethodFailsafe)
            {
                _repeatMethodFailsafe = true;
                OnEnable();
                OnInspectorGUI();
            }
            return;
        }
        _repeatMethodFailsafe = false;
    }

    private void DataManagerInspectorGUI()
    {
        // show script property up top 
        bool en = GUI.enabled;// preserve GUI.enabled state 
        GUI.enabled = false;
        EditorGUILayout.PropertyField(_scriptReference);
        GUI.enabled = en;

        if (Section("Dataset Loader", showLoadDatasets, out showLoadDatasets))
        {
            EditorGUILayout.PropertyField(_loadingParams);

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (Btn("Load Dataset", 0, BTN_HEIGHT_BIG))
            {
                dataManager.LoadAllDataFiles();
            }
            if (Btn("Clear Datasets", 120, BTN_HEIGHT_BIG))
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

        if (Section("Internal Reference Data", showInternalRefs, out showInternalRefs))
        {
            EditorGUILayout.PropertyField(_internalReferenceParams);

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (Btn("Load Internal References", 0, BTN_HEIGHT_BIG))
            {
                dataManager.LoadInternalReferences();
            }
            if (Btn("Clear InRefs", 100, BTN_HEIGHT_BIG))
            {
                dataManager.ClearInternalReferences();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            GUILayout.Space(8);
            if (Btn("Continents", 0, BTN_HEIGHT_SMALL))
            {
                dataManager.LoadInternalContinentReferences();
            }
            if (Btn("Countries", 0, BTN_HEIGHT_SMALL))
            {
                dataManager.LoadInternalCountryReferences();
            }
            if (Btn("Cities", 0, BTN_HEIGHT_SMALL))
            {
                dataManager.LoadInternalCityReferences();
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            
            EditorGUILayout.PropertyField(_internalMapCities);
            EditorGUILayout.PropertyField(_internalMapCountries);
            EditorGUILayout.PropertyField(_internalMapContinents);

            GUILayout.Space(5);

            EditorGUILayout.PropertyField(_referenceCities);
            EditorGUILayout.PropertyField(_referenceCountries);
            EditorGUILayout.PropertyField(_referenceContinents);

            EndSection();
        }

        if (Section("Export Data", showExportData, out showExportData))
        {
            EditorGUILayout.PropertyField(_exportSourceParams);

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (Btn("Export All Data", 0, BTN_HEIGHT_BIG))
            {
                dataManager.ExportData();
            }
            if (Btn("Clear ExpData", 100, BTN_HEIGHT_BIG))
            {
                dataManager.ClearExportData();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            GUILayout.Space(8);
            if (Btn("Continents", 0, BTN_HEIGHT_SMALL))
            {
                dataManager.ExportContinentsData();
            }
            if (Btn("Countries", 0, BTN_HEIGHT_SMALL))
            {
                dataManager.ExportCountriesData();
            }
            if (Btn("Cities", 0, BTN_HEIGHT_SMALL))
            {
                dataManager.ExportCitiesData();
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            EndSection();
        }

        serializedObject.ApplyModifiedProperties();

        DrawPropertiesExcluding(serializedObject, new string[] {
                "m_Script", "loadingParams",
                "cityDatasets", "countryDatasets", "continentDatasets",
                "referenceSourceFiles", "exportSourceParams",
                "referenceCities", "referenceCountries", "referenceContinents",
                "internalMapCities","internalMapCountries","internalMapContinents", });
    }

    private bool Btn(string label, int width = 0, int height = 0)
    {
        if (width > 0)
        {
            if (height > 0)
            {
                return GUILayout.Button(label,
                    GUILayout.Width(width), GUILayout.Height(height));
            }
            return GUILayout.Button(label, GUILayout.Width(width));
        }
        else if (height > 0)
        {
            return GUILayout.Button(label, GUILayout.Height(height));
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