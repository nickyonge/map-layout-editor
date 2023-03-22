using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataExporter : MonoBehaviour
{

    private DataManager dataManager;

    private bool _initialized = false;


    private void Start()
    {
        Initialize();
    }
    public void Initialize()
    {
        if (_initialized) { return; }
        _initialized = true;
        if (!TryGetComponent(out dataManager))
        {
            Debug.LogError("ERROR: No DataManager found on DataExporter's gameObject", gameObject);
        }
    }


    public void ExportDataByScope(DataScope scope)
    {
        switch (scope)
        {
            case DataScope.City:
                ExportCitiesData();
                break;
            case DataScope.Country:
                ExportCountriesData();
                break;
            case DataScope.Continent:
                ExportContinentsData();
                break;
            case DataScope.Other:
                Debug.LogWarning("WARNING: OTHER is invalid scope for " +
                    "ExportDataByScope", gameObject);
                break;
            default:
                Debug.LogWarning($"WARNING: Invalid scope: {scope}, for " +
                    "ExportDataByScope", gameObject);
                break;
        }
    }
    private void ExportCitiesData()
    {
        Initialize();
    }
    private void ExportCountriesData()
    {
        Initialize();
    }
    private void ExportContinentsData()
    {
        Initialize();
    }




    public void ClearDataExport() {
        Initialize();
        dataManager.ClearExportData();
    }




}
