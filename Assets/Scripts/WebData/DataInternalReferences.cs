using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataInternalReferences : MonoBehaviour
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
            Debug.LogError("ERROR: No DataManager found on DataRegionRef's gameObject", gameObject);
        }
    }


    public void LoadInternalReferences(DataScope scope)
    {
        switch (scope)
        {
            case DataScope.City:
                LoadCities();
                break;
            case DataScope.Country:
                LoadCountries();
                break;
            case DataScope.Continent:
                LoadContinents();
                break;
            case DataScope.Other:
                Debug.LogWarning("WARNING: OTHER is invalid scope for " +
                    "LoadInternalReferences");
                break;
            default:
                Debug.LogWarning($"WARNING: Invalid scope: {scope}, for " +
                    "LoadInternalReferences");
                break;
        }
    }
    private void LoadCities()
    {
        Initialize();
        // get data source
        Dataset d = dataManager.GetDataset(
            dataManager.referenceSourceFiles.sourceDataCity,
            DataScope.City);
        if (d == null)
        {
            Debug.LogError("ERROR: Could not find source data file for City, " +
                "ensure it's in dataManager.internalReferenceParams, returning", gameObject);
            return;
        }
        LoadInternalReferenceFromDatasets(DataScope.City, d);
    }
    private void LoadCountries()
    {
        Initialize();
        Dataset d = dataManager.GetDataset(
            dataManager.referenceSourceFiles.sourceDataCountry,
            DataScope.Country);
        if (d == null)
        {
            Debug.LogError("ERROR: Could not find source data file for Country, " +
                "ensure it's in dataManager.internalReferenceParams, returning", gameObject);
            return;
        }
        Dataset dAlt = dataManager.GetDataset(
            dataManager.referenceSourceFiles.sourceDataCountryAliases,
            DataScope.Country);
        if (dAlt == null)
        {
            Debug.LogError("ERROR: Could not find source data file for Country (AltNames), " +
                "ensure it's in dataManager.internalReferenceParams, returning", gameObject);
            return;
        }
        LoadInternalReferenceFromDatasets(DataScope.Country, d, dAlt);
    }
    private void LoadContinents()
    {
        Initialize();
        Dataset d = dataManager.GetDataset(
            dataManager.referenceSourceFiles.sourceDataContinent,
            DataScope.Continent);
        if (d == null)
        {
            Debug.LogError("ERROR: Could not find source data file for Continent, " +
                "ensure it's in dataManager.internalReferenceParams, returning", gameObject);
            return;
        }
        LoadInternalReferenceFromDatasets(DataScope.Continent, d);
    }


    private void LoadInternalReferenceFromDatasets(DataScope scope,
        Dataset dataset, Dataset alternateNames = null)
    {

        
    }


    public void ClearInternalReferences() {
        Initialize();
        dataManager.ClearInternalReferences();
    }


}
