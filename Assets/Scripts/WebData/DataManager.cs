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


    public Dataset[] cityDatasets;
    public Dataset[] countryDatasets;



    private MapDataCollector mapData;

    private void Start()
    {

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

        List<Dataset> cityDatasets = new();
        List<Dataset> countryDatasets = new();

        foreach (string file in files)
        {
            bool isCity = file.IndexOf("CityData") >= 0;
            if (!isCity)
            {
                if (file.IndexOf("CountryData") < 0)
                {
                    // improperly organized data, skip to next item 
                    Debug.LogWarning("WARNING: found data file, not organized as either " +
                        $"city nor country. Consider organizing. File: {file}", gameObject);
                    continue;
                }
            }

            // generate streamreader, ensure file exists 
            StreamReader streamReader = null;
            try
            {
                streamReader = new StreamReader(file);
            }
            catch (IOException e)
            {
                // IO Exception
                Debug.LogError("ERROR: IOException. If Excel is open, try closing it! " +
                    "(Or any app that might be using the data files)");
                throw e;
            }
            if (streamReader.Peek() < 0)
            {
                Debug.LogWarning("WARNING: was unable to generate StreamReader from file " +
                    $"{file}, skipping", gameObject);
                continue;
            }

            // count line entries (designed for CSV, need to modify for other types)
            int entries = -1;// start at -1 for the indicators line 
            while (streamReader.ReadLine() != null) {
                entries++;
                if (entries > 999999) {
                    Debug.LogError("ERROR: dataset too large! Over a million entries, yikes");
                }
            }
            streamReader.Close();


            int separator = Mathf.Max(file.LastIndexOf('\\'), file.LastIndexOf('/'));
            string fileNameWithExtension = file.Substring(separator + 1);
            int extensionIndex = fileNameWithExtension.LastIndexOf('.');
            string type = fileNameWithExtension.Substring(extensionIndex + 1).ToLower();
            string fileName = fileNameWithExtension.Substring(0, extensionIndex);
            string containingFolder = file.Substring(0, separator);
            separator = Mathf.Max(containingFolder.LastIndexOf('\\'),
                containingFolder.LastIndexOf('/'));
            containingFolder = containingFolder.Substring(separator + 1);
            string filePathDirectoryOnly = file.Substring(0, file.IndexOf(fileNameWithExtension) - 1);
            int resourcesIndex = filePathDirectoryOnly.IndexOf("Resources");
            string resourcesPath = filePathDirectoryOnly.Substring(resourcesIndex + 10);

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

            TextAsset dataFile = Resources.Load(Path.Combine(resourcesPath, fileName)) as TextAsset;


            Dataset dataset = new Dataset
            {
                fileName = string.Join(' ', containingFolder, ':', fileName),
                format = format,
                filePath = file,
                dataFile = dataFile,
                entries = entries,
            };
            
            // regenerate streamreader to load data 
            streamReader = new StreamReader(file);
            dataset.LoadData(streamReader);

            // assign datasets 
            if (isCity)
                cityDatasets.Add(dataset);
            else
                countryDatasets.Add(dataset);
        }

        this.cityDatasets = cityDatasets.ToArray();
        this.countryDatasets = countryDatasets.ToArray();
    }


    public void GenerateNewEntries()
    {
        GenerateNewCountryEntries();
        GenerateNewCityEntries();
    }
    public void GenerateNewCountryEntries()
    {
        Initialize();
        Debug.Log("Generating countries, " + mapData.AllCountries.Length);
    }
    public void GenerateNewCityEntries()
    {
        Initialize();
        Debug.Log("Generating cities, " + mapData.AllCities.Length);
    }



}
