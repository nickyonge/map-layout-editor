using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataRegionReference : MonoBehaviour
{

    public RegionCity[] refCities;
    public RegionCountry[] refCountries;
    public RegionContinent[] refContinents;



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
            dataManager.exportSourceParams.sourceDataCity,
            DataScope.City);
        if (d == null)
        {
            Debug.LogError("ERROR: Could not find source data file for City, " +
                "ensure it's in dataManager.exportSourceParams, returning", gameObject);
            return;
        }
        LoadInternalReferenceFromDatasets(DataScope.City, d);
    }
    private void LoadCountries()
    {
        Initialize();
        Dataset d = dataManager.GetDataset(
            dataManager.exportSourceParams.sourceDataCountry,
            DataScope.Country);
        if (d == null)
        {
            Debug.LogError("ERROR: Could not find source data file for Country, " +
                "ensure it's in dataManager.exportSourceParams, returning", gameObject);
            return;
        }
        Dataset dAlt = dataManager.GetDataset(
            dataManager.exportSourceParams.sourceDataCountryAliases,
            DataScope.Country);
        if (dAlt == null)
        {
            Debug.LogError("ERROR: Could not find source data file for Country (AltNames), " +
                "ensure it's in dataManager.exportSourceParams, returning", gameObject);
            return;
        }
        LoadInternalReferenceFromDatasets(DataScope.Country, d, dAlt);
    }
    private void LoadContinents()
    {
        Initialize();
        Dataset d = dataManager.GetDataset(
            dataManager.exportSourceParams.sourceDataContinent,
            DataScope.Continent);
        if (d == null)
        {
            Debug.LogError("ERROR: Could not find source data file for Continent, " +
                "ensure it's in dataManager.exportSourceParams, returning", gameObject);
            return;
        }
        LoadInternalReferenceFromDatasets(DataScope.Continent, d);
    }


    private void LoadInternalReferenceFromDatasets(DataScope scope,
        Dataset dataset, Dataset alternateNames = null)
    {
        Debug.Log("LOAD THE INTERNAL REGION REFERENCES");
    }




    public void ClearInternalReferences() {
        refCities = new RegionCity[0];
        refCountries = new RegionCountry[0];
        refContinents = new RegionContinent[0];
    }



    [Serializable]
    public struct RegionCity
    {
        ///<summary> ASCII name (no special chars) for the country </summary>
        [Header("Export Fields")]
        public string asciiName;
        ///<summary> 2char country code </summary>
        public string countryIso2;
        public string continentCode;

        [Header("Internal Refernce Fields")]
        public string properName;
        public string simplifiedName;
        public string countryFullName;
        public string[] alternateNames;
    }

    [Serializable]
    public struct RegionCountry
    {
        [Header("Export Fields")]
        public string asciiName;
        public string isoA2;
        public string isoA3;
        public int isoN3;
        public string isoN3String
        {
            get { return isoN3.ToString("000"); }
        }
        public string continentCode;

        [Header("Internal Reference Fields")]
        public string baseName;
        public string simplifiedName;
        public string[] alternateNames;

    }

    [Serializable]
    public struct RegionContinent
    {

        [Header("Export Fields")]
        public string name;
        public string code;
        [Header("Internal Reference Fields")]
        public string[] alternateNames;
    }


}
