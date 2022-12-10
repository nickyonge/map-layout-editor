using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary> Format types for data to read (note: as of this writing, only CSV is supported) </summary>
public enum DataFormat { CSV, JSON, XML, XLS, XLSX, ERROR }

/// <summary> Scope, regional, that a given dataset represents </summary>
public enum DataScope { City = 0, Country = 1, Continent = 2, Other = 3 }

[RequireComponent(typeof(DataInternalReferences), typeof(DataExporter))]
public class DataManager : DataDownloader
{
    public static DataManager instance;

    private const bool CHECK_DATASETS_FAST = false;
    public const bool DEBUG_UNIMPLEMENTED_FORMATS = false;
    public const bool SKIP_UNIMPLEMENTED_FORMATS = true;

    public DatasetLoadingParams loadingParams;
    public MapReferenceParams mapReferenceParams;
    public InternalReferenceParams referenceSourceFiles;
    public ExportDataParams exportSourceParams;

    // LOADED DATASETS 
    public Dataset[] cityDatasets;
    public Dataset[] countryDatasets;
    public Dataset[] continentDatasets;

    // MAP REFERENCES 
    public DataStructs.MapReference[] mapCities;// found in in-app map
    public DataStructs.MapReference[] mapCountries;
    public DataStructs.MapReference[] mapContinents;

    // INTERNAL REFERENCES 
    public DataStructs.InternalReference_City[] referenceCities;// loaded from datasets 
    public DataStructs.InternalReference_Country[] referenceCountries;
    public DataStructs.InternalReference_Continent[] referenceContinents;

    // EXPORT DATA 


    // local script references 
    public MapDataCollector mapData;
    private DataMapReferences dataMapRefs;
    private DataInternalReferences dataInternalRefs;
    private DataExporter dataExporter;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        // ensure only one  instance exists 
        if (instance != null)
        {
            if (instance != this)
            {
                if (Application.isPlaying)
                {
                    Destroy(instance);
                }
                else
                {
                    DestroyImmediate(instance);
                }
            }
            return;
        }
        instance = this;

        if (mapData == null)
        {
            mapData = GetComponentInChildren<MapDataCollector>();
            if (mapData == null)
            {
                Debug.LogError("ERROR: mapData is null and could " +
                    "not be found in datamanager children", gameObject);
            }
        }

        if (dataMapRefs == null)
        {
            if (!TryGetComponent(out dataMapRefs))
            {
                dataMapRefs = gameObject.AddComponent<DataMapReferences>();
            }
        }
        if (dataInternalRefs == null)
        {
            if (!TryGetComponent(out dataInternalRefs))
            {
                dataInternalRefs = gameObject.AddComponent<DataInternalReferences>();
            }
        }
        if (dataExporter == null)
        {
            if (!TryGetComponent(out dataExporter))
            {
                dataExporter = gameObject.AddComponent<DataExporter>();
            }
        }
    }

    public void LoadAllDataFiles()
    {
        Initialize();

        if (loadingParams == null) { loadingParams = new(); }

        string path = Path.Combine(Application.dataPath, "Data");

        List<string> files = Directory
            // .GetFiles(path, "*.*", SearchOption.AllDirectories)
            .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(file =>
                file.ToLower().EndsWith("csv") ||
                file.ToLower().EndsWith("xls") ||
                file.ToLower().EndsWith("xlsx") ||
                file.ToLower().EndsWith("json") ||
                file.ToLower().EndsWith("xml")
                ).ToList();

        // first, quick check if any file is currently open 
        foreach (string file in files)
        {
            try
            {
                StreamReader testReader = new StreamReader(file);
            }
            catch (IOException e)
            {
                // IO Exception
                string n = Path.GetFileName(file);
                if (n == null) { n = "[NULL, see file path below]"; }
                Debug.LogError($"File in use: <b>{n}</b>. " +
                    "If Excel is open, try closing it! (Or any app that might be using this file) " +
                    $"Exception to follow. \nFilepath: {file}");
                throw e;
            }
        }

        // prep dataset containers 
        List<Dataset> cityDatasets = new();
        List<Dataset> countryDatasets = new();
        List<Dataset> continentDatasets = new();

        // iterate through all files 
        foreach (string file in files)
        {
            DataScope scope = DataScope.City;
            if (file.IndexOf("CityData") >= 0)
            {
                scope = DataScope.City;
            }
            else if (file.IndexOf("CountryData") >= 0)
            {
                scope = DataScope.Country;
            }
            else if (file.IndexOf("ContinentData") >= 0)
            {
                scope = DataScope.Continent;
            }
            else
            {
                scope = DataScope.Other;
                Debug.LogWarning("WARNING: invalid data subdirectory, cannot determine scope. " +
                    $"Consider organizing. File: {file}", gameObject);
                continue;
            }

            // get all necessary text info, filenames, extensions, etc 
            int separator = Mathf.Max(0, file.LastIndexOf('\\'), file.LastIndexOf('/'));
            string fileNameWithExtension = file.Substring(separator + 1);
            int extensionIndex = fileNameWithExtension.LastIndexOf('.');
            string fileName = fileNameWithExtension.Substring(0, extensionIndex);

            // check if filename should be skipped 
            if (loadingParams.skipDatasets.ContainsAnyByIndex(fileName))
            {
                // skip this filename! 
                continue;
            }

            // get containing folders 
            string[] containingFolders = GetContainingFolders(file);
            // check if containing folders should be ignored 
            if ((containingFolders.Length == 1 &&
                    loadingParams.skipDatasets.ContainsAnyByIndex(containingFolders[0])) ||
                    loadingParams.skipContainerFolders.ContainsAnyByIndex(containingFolders))
            {
                // yup! skip this folder 
                continue;
            }

            // generate streamreader, ensure file exists 
            StreamReader streamReader = new StreamReader(file);
            if (streamReader.Peek() < 0)
            {
                Debug.LogWarning("WARNING: was unable to generate StreamReader from file " +
                    $"{file}, skipping\nConsider removing or excluding the {fileName} dataset", gameObject);
                continue;
            }

            // determine filetype format 
            string type = fileNameWithExtension.Substring(extensionIndex + 1).ToLower();
            DataFormat format = DataFormat.CSV;
#pragma warning disable CS0162
            switch (type)
            {
                case "csv":
                    format = DataFormat.CSV;
                    break;
                case "xls":
                    format = DataFormat.XLS;
                    if (DEBUG_UNIMPLEMENTED_FORMATS) Debug.LogWarning("WARNING: TEST XLS FOR FUNCTION");
                    if (SKIP_UNIMPLEMENTED_FORMATS) continue;
                    break;
                case "xlsx":
                    format = DataFormat.XLSX;
                    if (DEBUG_UNIMPLEMENTED_FORMATS) Debug.LogWarning("WARNING: TEST XLSX FOR FUNCTION");
                    if (SKIP_UNIMPLEMENTED_FORMATS) continue;
                    break;
                case "json":
                    format = DataFormat.JSON;
                    if (DEBUG_UNIMPLEMENTED_FORMATS) Debug.LogWarning("WARNING: TEST JSON FOR FUNCTION");
                    if (SKIP_UNIMPLEMENTED_FORMATS) continue;
                    break;
                case "xml":
                    format = DataFormat.XML;
                    if (DEBUG_UNIMPLEMENTED_FORMATS) Debug.LogWarning("WARNING: TEST XML FOR FUNCTION");
                    if (SKIP_UNIMPLEMENTED_FORMATS) continue;
                    break;
                default:
                    Debug.LogError($"ERROR: Invalid Dataset format, format: {type}, " +
                        $"filepath: {file}, cannot parse file", gameObject);
                    format = DataFormat.ERROR;
                    break;
            }
#pragma warning restore CS0162

            // define containing folder text
            string containingFolder = containingFolders[0];
            string containingFolderPathTitle = string.Join(" / ", containingFolders);

            // directory and resource loading refs 
            string filePathDirectoryOnly = file.Substring(0, file.IndexOf(fileNameWithExtension) - 1);
            int resourcesIndex = filePathDirectoryOnly.IndexOf("Resources");
            string resourcesPath = filePathDirectoryOnly.Substring(resourcesIndex + 10);

            // use streamreader to count line entries (designed for CSV, need to modify for other types)
            int entries = -1;// start at -1 for the indicators line 
            while (streamReader.ReadLine() != null)
            {
                entries++;
                if (entries > 999999)
                {
                    Debug.LogError("ERROR: dataset too large! Over a million entries, yikes");
                }
            }
            streamReader.Close();

            // load data file textasset 
            TextAsset dataFile = Resources.Load(Path.Combine(resourcesPath, fileName)) as TextAsset;

            // create the dataset 
            Dataset dataset = new Dataset
            {
                fileName = string.Join(' ', containingFolderPathTitle, ':', fileName),
                scope = scope,
                format = format,
                filePath = file,
                dataFile = dataFile,
                entries = entries,
                containingFolder = containingFolder,
            };
            // regenerate streamreader to load data 
            streamReader = new StreamReader(file);
            dataset.LoadData(streamReader);

            // assign datasets 
            switch (scope)
            {
                case DataScope.City:
                    cityDatasets.Add(dataset);
                    break;
                case DataScope.Country:
                    countryDatasets.Add(dataset);
                    break;
                case DataScope.Continent:
                    continentDatasets.Add(dataset);
                    break;
            }
        }

        // assign local datasets 
        this.cityDatasets = cityDatasets.ToArray();
        this.countryDatasets = countryDatasets.ToArray();
        this.continentDatasets = continentDatasets.ToArray();
    }
    public void ClearDataFiles()
    {
        cityDatasets = new Dataset[0];
        countryDatasets = new Dataset[0];
        continentDatasets = new Dataset[0];
    }


    private string[] GetContainingFolders(string filePath)
    {
        int separator = Mathf.Max(filePath.LastIndexOf('\\'), filePath.LastIndexOf('/'));
        // if separator is right at start, simply return path 
        switch (separator)
        {
            case -1:
                // no separator, return filepath
                return new string[] { filePath };
            case 0:
                // only one separator right at start 
                return new string[] { filePath.Substring(1) };
        }
        List<string> containingFolders = new();
        string origPath = filePath;
        int failsafe = filePath.Length + 1;
        while (failsafe > 0)
        {
            failsafe--;
            if (failsafe == 0)
            {
                Debug.LogError("ERROR: Failsafe hit zero getting parent folders for path " +
                    $"{origPath}, this should be impossible investigate", gameObject);
                break;
            }
            string containingFolder = filePath.Substring(0, separator);
            separator = Mathf.Max(0, containingFolder.LastIndexOf('\\'),
                containingFolder.LastIndexOf('/'));
            containingFolder = containingFolder.Substring(separator + 1).Trim();
            // check if containing folder is valid
            switch (containingFolder)
            {
                case "CityData":
                case "CountryData":
                case "ContinentData":
                    // done, hop out of the city/country data folders 
                    failsafe = 0;
                    break;
                default:
                    filePath = filePath.Substring(0, separator);
                    containingFolders.Insert(0, containingFolder);
                    break;
            }
        }
        return containingFolders.ToArray();
    }




    public void LoadInternalReferences()
    {
        LoadInternalContinentReferences();
        LoadInternalCountryReferences();
        LoadInternalCityReferences();
    }
    public void LoadInternalContinentReferences()
    {
        LoadInternalReferencesByScope(DataScope.Continent);
    }
    public void LoadInternalCountryReferences()
    {
        LoadInternalReferencesByScope(DataScope.Country);
    }
    public void LoadInternalCityReferences()
    {
        LoadInternalReferencesByScope(DataScope.City);
    }

    private void LoadInternalReferencesByScope(DataScope scope)
    {
        Initialize();
        dataInternalRefs.LoadInternalReferences(scope);
    }
    public void ClearInternalReferences()
    {
        Initialize();
        referenceCities = new DataStructs.InternalReference_City[0];
        referenceCountries = new DataStructs.InternalReference_Country[0];
        referenceContinents = new DataStructs.InternalReference_Continent[0];
    }




    public void LoadMapReferences()
    {
        Initialize();
        dataMapRefs.LoadMapReferences();
    }
    public void ClearMapReferences()
    {
        Initialize();
        mapCities = new DataStructs.MapReference[0];
        mapCountries = new DataStructs.MapReference[0];
        mapContinents = new DataStructs.MapReference[0];
        if (_useMapDataCollecterAsMapRefs)
        {

        }
    }






    public void ExportData()
    {
        ExportCitiesData();
        ExportCountriesData();
        ExportContinentsData();
    }

    public void ExportCitiesData()
    {
        ExportDataByScope(DataScope.Continent);
    }
    public void ExportCountriesData()
    {
        ExportDataByScope(DataScope.Country);
    }
    public void ExportContinentsData()
    {
        ExportDataByScope(DataScope.City);
    }

    private void ExportDataByScope(DataScope scope)
    {
        Initialize();
        dataExporter.ExportDataByScope(scope);
    }
    public void ClearExportData()
    {
        Initialize();
    }







    [Serializable]
    public class DatasetLoadingParams
    {

        [Space(5)]
        public string[] skipDatasets;
        public string[] skipContainerFolders;

        /// <summary>
        /// these indicators will be dropped from tables if no fields are found for them 
        /// </summary>
        [Space(5)]
        public string[] softIgnoreIndicators;

        [Space(5)]
        public CustomDatasetProperties[] customProperties;

        [Serializable]
        public struct CustomDatasetProperties
        {
            public string dataset;
            public int dataLineCount;
        }

        public int GetInitialDataLine(Dataset dataset)
        {
            return GetInitialDataLine(dataset.fileName, dataset.containingFolder);
        }
        public int GetInitialDataLine(string dataset, string containingFolder)
        {
            dataset = dataset.ToLower();
            containingFolder = containingFolder.ToLower();
            if (customProperties != null && customProperties.Length > 0)
            {
                foreach (CustomDatasetProperties cdp in customProperties)
                {
                    if (cdp.dataLineCount > 0 &&
                        (cdp.dataset.ToLower().StartsWith(dataset) ||
                        cdp.dataset.ToLower().StartsWith(containingFolder)))
                    {
                        return cdp.dataLineCount - 1;
                    }
                }
            }
            return 0;
        }
    }

    [Serializable]
    public class InternalReferenceParams
    {

        [Space(5)]
        public TextAsset sourceDataCity;
        public TextAsset sourceDataCountry;
        public TextAsset sourceDataCountryAliases;
        public TextAsset sourceDataContinent;

        public TextAsset GetSourceDataByScope(DataScope scope, bool altNames = false)
        {
            switch (scope)
            {
                case DataScope.City:
                    return sourceDataCity;
                case DataScope.Country:
                    return sourceDataCountry;
                case DataScope.Continent:
                    return sourceDataContinent;
                case DataScope.Other:
                    Debug.LogWarning("WARNING: OTHER is invalid scope for " +
                        "GetSourceDataByScope, returning null");
                    return null;
                default:
                    Debug.LogWarning($"WARNING: Invalid scope: {scope}, for " +
                        "GetSourceDataByScope, returning null");
                    return null;
            }
        }
    }

    [Serializable]
    public class MapReferenceParams
    {
    }

    [Serializable]
    public class ExportDataParams
    {

    }

    public Dataset GetDataset(string name)
    {
        // search country, then city, then continent
        Dataset d = GetDataset(name, DataScope.Country);
        if (d != null) { return d; }
        d = GetDataset(name, DataScope.City);
        if (d != null) { return d; }
        return GetDataset(name, DataScope.Continent);
    }
    public Dataset GetDataset(TextAsset dataFile)
    {
        // search country, then city, then continent
        Dataset d = GetDataset(dataFile, DataScope.Country);
        if (d != null) { return d; }
        d = GetDataset(dataFile, DataScope.City);
        if (d != null) { return d; }
        return GetDataset(dataFile, DataScope.Continent);
    }
    public Dataset GetDataset(string name, DataScope scope)
    {
        Dataset[] dataset = GetDatasetsByScope(scope);
        foreach (Dataset d in dataset)
        {
            if (d.DoesMatch(name, CHECK_DATASETS_FAST))
            {
                return d;
            }
        }
        return null;
    }
    public Dataset GetDataset(TextAsset dataFile, DataScope scope)
    {
        Dataset[] dataset = GetDatasetsByScope(scope);
        foreach (Dataset d in dataset)
        {
            if (d.DoesMatch(dataFile))
            {
                return d;
            }
        }
        return null;
    }

    /// <summary> returns the dataset array associated with the given scope </summary>
    public Dataset[] GetDatasetsByScope(DataScope scope)
    {
        switch (scope)
        {
            case DataScope.City:
                return cityDatasets;
            case DataScope.Country:
                return countryDatasets;
            case DataScope.Continent:
                return continentDatasets;
            case DataScope.Other:
                Debug.LogWarning("WARNING: OTHER is an invalid option " +
                    "for get dataset by scope, returning null", gameObject);
                break;
            default:
                Debug.LogError($"ERROR: invalid DataScope {scope}, " +
                    "can't get dataset by scope, returning null", gameObject);
                break;
        }
        return null;
    }


    public bool HaveLoadedDatasets()
    {
        return
            cityDatasets.Length > 0 &&
            countryDatasets.Length > 0 &&
            continentDatasets.Length > 0;
    }
    public static bool _useMapDataCollecterAsMapRefs = true;
    public bool HaveLoadedMapReferences()
    {
        // check if using map data collection as map refs 
        // if so, ensure that the proper country codes have been established 
        if (!HaveLoadedDatasets()) { return false; }
        if (_useMapDataCollecterAsMapRefs)
        {
                Debug.Log(1);
            if (!mapData.IsDataCollected(true))
            {
                return false;
            }
            Debug.Log(2);
            if (mapData.world.continents.Length == 0 ||
                !mapData.world.continents[0].CheckValid() ||
                !mapData.world.continents[mapData.world.continents.Length - 1].CheckValid())
            {
                return false;
            }
            Debug.Log(3);
            if (mapData.world.continents[0].countries.Length == 0 ||
                !mapData.world.continents[0].countries[0].CheckValid() ||
                !mapData.world.continents[0].countries
                    [mapData.world.continents[0].countries.Length - 1].CheckValid())
            {
                return false;
            }
            Debug.Log(4);
            return true;
        }
        return mapCities.Length > 0 &&
            mapCountries.Length > 0 &&
            mapContinents.Length > 0;
    }
    public bool HaveLoadedInternalReferences()
    {
        return HaveLoadedMapReferences() &&
            referenceCities.Length > 0 &&
            referenceCountries.Length > 0 &&
            referenceContinents.Length > 0;
    }

}
