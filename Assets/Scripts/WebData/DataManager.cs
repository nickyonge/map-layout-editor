using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataManager : DataDownloader
{
    public static DataManager instance;

    public const bool DEBUG_UNIMPLEMENTED_FORMATS = false;
    public const bool SKIP_UNIMPLEMENTED_FORMATS = true;

    public DatasetLoadingParams loadingParams;

    public Dataset[] cityDatasets;
    public Dataset[] countryDatasets;
    public Dataset[] continentDatasets;



    private MapDataCollector mapData;

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
                Debug.LogError("ERROR: mapData is null and could not be found in datamanager children", gameObject);
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
            Dataset.DataScope scope = Dataset.DataScope.City;
            if (file.IndexOf("CityData") >= 0)
            {
                scope = Dataset.DataScope.City;
            }
            else if (file.IndexOf("CountryData") >= 0)
            {
                scope = Dataset.DataScope.Country;
            }
            else if (file.IndexOf("ContinentData") >= 0)
            {
                scope = Dataset.DataScope.Continent;
            }
            else
            {
                scope = Dataset.DataScope.Other;
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
            Dataset.DataFormat format = Dataset.DataFormat.CSV;
#pragma warning disable CS0162
            switch (type)
            {
                case "csv":
                    format = Dataset.DataFormat.CSV;
                    break;
                case "xls":
                    format = Dataset.DataFormat.XLS;
                    if (DEBUG_UNIMPLEMENTED_FORMATS) Debug.LogWarning("WARNING: TEST XLS FOR FUNCTION");
                    if (SKIP_UNIMPLEMENTED_FORMATS) continue;
                    break;
                case "xlsx":
                    format = Dataset.DataFormat.XLSX;
                    if (DEBUG_UNIMPLEMENTED_FORMATS) Debug.LogWarning("WARNING: TEST XLSX FOR FUNCTION");
                    if (SKIP_UNIMPLEMENTED_FORMATS) continue;
                    break;
                case "json":
                    format = Dataset.DataFormat.JSON;
                    if (DEBUG_UNIMPLEMENTED_FORMATS) Debug.LogWarning("WARNING: TEST JSON FOR FUNCTION");
                    if (SKIP_UNIMPLEMENTED_FORMATS) continue;
                    break;
                case "xml":
                    format = Dataset.DataFormat.XML;
                    if (DEBUG_UNIMPLEMENTED_FORMATS) Debug.LogWarning("WARNING: TEST XML FOR FUNCTION");
                    if (SKIP_UNIMPLEMENTED_FORMATS) continue;
                    break;
                default:
                    Debug.LogError($"ERROR: Invalid Dataset format, format: {type}, " +
                        $"filepath: {file}, cannot parse file", gameObject);
                    format = Dataset.DataFormat.ERROR;
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
                case Dataset.DataScope.City:
                    cityDatasets.Add(dataset);
                    break;
                case Dataset.DataScope.Country:
                    countryDatasets.Add(dataset);
                    break;
                case Dataset.DataScope.Continent:
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

    public void GenerateNewEntries()
    {
        GenerateNewContinentEntries();
        GenerateNewCountryEntries();
        GenerateNewCityEntries();
    }

    public void GenerateNewContinentEntries()
    {
        GenerateEntries(Dataset.DataScope.Continent);
    }
    public void GenerateNewCountryEntries()
    {
        GenerateEntries(Dataset.DataScope.Country);
    }
    public void GenerateNewCityEntries()
    {
        GenerateEntries(Dataset.DataScope.City);
    }

    private void GenerateEntries(Dataset.DataScope scope)
    {
        Initialize();
    }


    public void ClearDataEntries()
    {
        Initialize();
    }


    [Serializable]
    public class DatasetLoadingParams
    {

        [Space(5)]
        public TextAsset sourceDataCity;
        public TextAsset sourceDataCountry;
        public TextAsset sourceDataContinent;

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

}
