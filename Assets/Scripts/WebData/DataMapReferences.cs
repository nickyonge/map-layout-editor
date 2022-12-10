using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataMapReferences : MonoBehaviour
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


    public void LoadMapReferences()
    {
        // load all map references from mapData
        // Dictionary<Continent, Dictionary<Country, City[]>> AllCitiesByCountryByContinent

        // ensure data manager is up to date 
        dataManager.mapData.CollectData();

        if (DataManager._useMapDataCollecterAsMapRefs)
        {
            return;
        }

        List<DataStructs.MapReference> mapCities = new();
        List<DataStructs.MapReference> mapCountries = new();
        List<DataStructs.MapReference> mapContinents = new();
        // iterate through continents, countries, cities
        foreach (KeyValuePair<MapDataCollector.Continent,
            Dictionary<MapDataCollector.Country, MapDataCollector.City[]>>
            continent in dataManager.mapData.AllCitiesByCountryByContinent)
        {
            // get continent
            mapContinents.Add(new DataStructs.MapReference(continent.Key));
            // iterate through countries, cities 
            foreach (KeyValuePair<MapDataCollector.Country, MapDataCollector.City[]>
                country in continent.Value)
            {
                // get country
                mapCountries.Add(new DataStructs.MapReference(country.Key));
                // iterate through cities 
                foreach (MapDataCollector.City city in country.Value)
                {
                    // get city 
                    mapCities.Add(new DataStructs.MapReference(city));
                }
            }
        }
        dataManager.mapCities = mapCities.ToArray();
        dataManager.mapCountries = mapCountries.ToArray();
        dataManager.mapContinents = mapContinents.ToArray();
    }


    public void ClearMapReferences()
    {
        Initialize();
        dataManager.ClearMapReferences();
    }


}
