using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataRegionReference : MonoBehaviour
{
    private DataManager dataManager;

    private bool _initialized = false;


    private void Start() {

    }
    public void Initialize() {
        if (_initialized) { return; }
        _initialized = true;
        if (!TryGetComponent(out dataManager)) {
            Debug.LogError("ERROR: No DataManager found on DataRegionRef's gameObject", gameObject);
        }
    }

    public void LoadCities() {
        Initialize();
    }
    public void LoadCountries() {
        Initialize();
    }
    public void LoadContinents() {
        Initialize();
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
    public struct RegionContinent {

        [Header("Export Fields")]
        public string name;
        public string code;
        [Header("Internal Reference Fields")]
        public string[] alternateNames;
    }


}
