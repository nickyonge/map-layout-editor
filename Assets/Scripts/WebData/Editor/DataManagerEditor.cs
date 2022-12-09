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
    private const int BTN_CLEAR_WIDTH = 110;

    private DataManager dataManager;

    private SerializedProperty _scriptReference;
    private SerializedProperty _loadingParams;
    private SerializedProperty _cityDatasets;
    private SerializedProperty _countryDatasets;
    private SerializedProperty _continentDatasets;
    private SerializedProperty _mapReferenceParams;
    private SerializedProperty _internalReferenceParams;
    private SerializedProperty _exportSourceParams;

    private SerializedProperty _mapCities;
    private SerializedProperty _mapCountries;
    private SerializedProperty _mapContinents;
    private SerializedProperty _referenceCities;
    private SerializedProperty _referenceCountries;
    private SerializedProperty _referenceContinents;


    private GUIStyle _foldoutHeader;

    private static bool showLoadDatasets;
    private static bool showMapReferences;
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
            _mapReferenceParams = serializedObject.FindProperty("referenceSourceFiles");
            _internalReferenceParams = serializedObject.FindProperty("referenceSourceFiles");
            _exportSourceParams = serializedObject.FindProperty("exportSourceParams");
            _mapCities = serializedObject.FindProperty("mapCities");
            _mapCountries = serializedObject.FindProperty("mapCountries");
            _mapContinents = serializedObject.FindProperty("mapContinents");
            _referenceCities = serializedObject.FindProperty("referenceCities");
            _referenceCountries = serializedObject.FindProperty("referenceCountries");
            _referenceContinents = serializedObject.FindProperty("referenceContinents");
            base.OnEnable();
        }
        catch
        {
            // error, do nothing except close foldouts
            showLoadDatasets = false;
            showMapReferences = false;
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
        bool guiEnabledState = GUI.enabled;// preserve GUI.enabled state 
        GUI.enabled = false;
        EditorGUILayout.PropertyField(_scriptReference);
        GUI.enabled = guiEnabledState;

        if (Section("Dataset Loader", showLoadDatasets, out showLoadDatasets))
        {
            EditorGUILayout.PropertyField(_loadingParams);

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (Btn("Load Dataset", 0, BTN_HEIGHT_BIG))
            {
                dataManager.LoadAllDataFiles();
            }
            if (Btn("Clear Datasets", BTN_CLEAR_WIDTH, BTN_HEIGHT_BIG))
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
        else
        {
            GUILayout.Space(-6);
            GUILayout.BeginHorizontal();
            if (Btn("Load Dataset", 0, BTN_HEIGHT_SMALL))
            {
                dataManager.LoadAllDataFiles();
            }
            if (Btn("Clear Datasets", BTN_CLEAR_WIDTH, BTN_HEIGHT_SMALL))
            {
                dataManager.ClearDataFiles();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
        }

        bool haveLoaded = dataManager.HaveLoadedDatasets();
        GUI.enabled = haveLoaded;
        string loadMsg = haveLoaded ? "" : " (Load Datasets First)";
        if (Section("Map Reference Data" + loadMsg, showMapReferences, out showMapReferences))
        {
            GUI.enabled = true;// can always edit data properties 
            EditorGUILayout.PropertyField(_mapReferenceParams);
            GUI.enabled = haveLoaded;

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (Btn("Populate Map References", 0, BTN_HEIGHT_BIG))
            {
                dataManager.LoadMapReferences();
            }
            if (Btn("Clear MapRefs", BTN_CLEAR_WIDTH, BTN_HEIGHT_BIG))
            {
                dataManager.ClearMapReferences();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (!DataManager._useMapDataCollecterAsMapRefs)
            {
                EditorGUILayout.PropertyField(_mapCities);
                EditorGUILayout.PropertyField(_mapCountries);
                EditorGUILayout.PropertyField(_mapContinents);
            }

            EndSection();
        }
        else
        {
            GUILayout.Space(-6);
            GUILayout.BeginHorizontal();
            if (Btn("Populate Map References", 0, BTN_HEIGHT_SMALL))
            {
                dataManager.LoadMapReferences();
            }
            if (Btn("Clear MapRefs", BTN_CLEAR_WIDTH, BTN_HEIGHT_SMALL))
            {
                dataManager.ClearMapReferences();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
        }

        haveLoaded = dataManager.HaveLoadedMapReferences();
        GUI.enabled = haveLoaded;
        loadMsg = haveLoaded ? "" : " (Load Map Refs First)";
        if (Section("Internal Reference Data" + loadMsg, showInternalRefs, out showInternalRefs))
        {
            GUI.enabled = true;// can always edit data properties 
            EditorGUILayout.PropertyField(_internalReferenceParams);
            GUI.enabled = haveLoaded;

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (Btn("Load Internal References", 0, BTN_HEIGHT_BIG))
            {
                dataManager.LoadInternalReferences();
            }
            if (Btn("Clear InRefs", BTN_CLEAR_WIDTH, BTN_HEIGHT_BIG))
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

            EditorGUILayout.PropertyField(_referenceCities);
            EditorGUILayout.PropertyField(_referenceCountries);
            EditorGUILayout.PropertyField(_referenceContinents);

            EndSection();
        }
        else
        {
            GUILayout.Space(-6);
            GUILayout.BeginHorizontal();
            if (Btn("Load Internal References", 0, BTN_HEIGHT_SMALL))
            {
                dataManager.LoadInternalReferences();
            }
            if (Btn("Clear InRefs", BTN_CLEAR_WIDTH, BTN_HEIGHT_SMALL))
            {
                dataManager.ClearInternalReferences();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
        }

        haveLoaded = dataManager.HaveLoadedInternalReferences();
        GUI.enabled = haveLoaded;
        loadMsg = haveLoaded ? "" : " (Load Internal Refs First)";
        if (Section("Export Data" + loadMsg, showExportData, out showExportData))
        {
            GUI.enabled = true;// can always edit data properties 
            EditorGUILayout.PropertyField(_exportSourceParams);
            GUI.enabled = haveLoaded;

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (Btn("Export All Data", 0, BTN_HEIGHT_BIG))
            {
                dataManager.ExportData();
            }
            if (Btn("Clear ExpData", BTN_CLEAR_WIDTH, BTN_HEIGHT_BIG))
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
        else
        {
            GUILayout.Space(-6);
            GUILayout.BeginHorizontal();
            if (Btn("Export All Data", 0, BTN_HEIGHT_SMALL))
            {
                dataManager.ExportData();
            }
            if (Btn("Clear ExpData", BTN_CLEAR_WIDTH, BTN_HEIGHT_SMALL))
            {
                dataManager.ClearExportData();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
        }

        GUI.enabled = guiEnabledState;

        serializedObject.ApplyModifiedProperties();

        DrawPropertiesExcluding(serializedObject, new string[] {
                "m_Script", "mapData", "loadingParams", "mapReferenceParams",
                "cityDatasets", "countryDatasets", "continentDatasets",
                "referenceSourceFiles", "exportSourceParams",
                "referenceCities", "referenceCountries", "referenceContinents",
                "mapCities","mapCountries","mapContinents", });
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
    private void EndSection() { GUILayout.Space(25); EditorGUI.indentLevel--; }
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