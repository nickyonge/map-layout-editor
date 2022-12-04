using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataManager : DataDownloader
{
    public static DataManager instance;

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
                file.ToLower().EndsWith("json") ||
                file.ToLower().EndsWith("xml")
                ).ToList();

        for (int i = 0; i < files.Count; i++)
        {
            files[i] = files[i].Substring(files[i].LastIndexOf("\\") + 1);
            files[i] = i.ToString() + " " + files[i];
        }

        Debug.Log(mapData.world.name);
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
