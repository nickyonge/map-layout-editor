using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class MapDataCollector : MonoBehaviour
{
    public static MapDataCollector instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject[] objs = SceneManager.GetActiveScene().GetRootGameObjects()
                    .Where(obj => obj.transform.parent == null).ToArray();

                for (int i = 0; i < objs.Length; i++)
                {
                    if (objs[i].TryGetComponent(out MapDataCollector mdc))
                    {
                        mdc.Initialize();
                        if (mdc != null && mdc == _instance)
                        {
                            return _instance;
                        }
                    }
                }
                if (_instance == null)
                {
                    Debug.LogError("ERROR: could not find any MapDataCollector in scene, ensure that one exists, returning null");
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }
    /// <summary>
    /// Internal ref only, use <see cref="instance">"instance"</see>, without the underscore
    /// </summary>
    private static MapDataCollector _instance;


    public World world;


    [Header("Setup")]
    [SerializeField] private bool _collectDataOnInit = true;


    private void Start()
    {
        Initialize();
    }
    internal void Initialize()
    {
        // ensure only one  instance exists 
        if (_instance != null)
        {
            if (_instance != this)
            {
                if (Application.isPlaying)
                {
                    Destroy(_instance);
                }
                else
                {
                    DestroyImmediate(_instance);
                }
            }
            return;
        }
        instance = this;

        if (_collectDataOnInit)
        {
            CollectData();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
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
    }

    /// <summary>
    /// Iterates through all child GameObjects of this script, identifying and 
    /// sorting through continents, then countries, then cities 
    /// </summary>
    public void CollectData()
    {
        Initialize();

        if (!TryGetComponent(out MD_World worldData))
        {
            worldData = gameObject.AddComponent<MD_World>();
        }

        world = new World(worldData);

    }

    public Continent[] AllContinents
    {
        get { return world.continents; }

    }
    public Country[] AllCountries
    {
        get
        {
            List<Country> countries = new();
            foreach (Continent continent in AllContinents)
            {
                countries.AddRange(continent.countries);
            }
            return countries.ToArray();
        }
    }
    public City[] AllCities
    {
        get
        {
            List<City> cities = new();
            foreach (Country country in AllCountries)
            {
                cities.AddRange(country.cities);
            }
            return cities.ToArray();
        }
    }
    public Dictionary<Continent, Country[]> AllCountriesByContinent
    {
        get
        {
            Dictionary<Continent, Country[]> allCountriesByContinent = new();
            foreach (Continent continent in AllContinents)
            {
                allCountriesByContinent.Add(continent, continent.countries);
            }
            return allCountriesByContinent;
        }
    }
    public Dictionary<Country, City[]> AllCitiesByCountry
    {
        get
        {
            Dictionary<Country, City[]> allCitiesByCountry = new();
            foreach (Country country in AllCountries)
            {
                allCitiesByCountry.Add(country, country.cities);
            }
            return allCitiesByCountry;
        }
    }
    public Dictionary<Continent, Dictionary<Country, City[]>> AllCitiesByCountryByContinent
    {
        get
        {
            Dictionary<Continent, Dictionary<Country, City[]>> allCitiesByCountryByContinent = new();
            foreach (Continent continent in AllContinents)
            {
                Dictionary<Country, City[]> allCitiesByCountry = new();
                foreach (Country country in continent.countries)
                {
                    allCitiesByCountry.Add(country, country.cities);
                }
                allCitiesByCountryByContinent.Add(continent, allCitiesByCountry);
            }
            return allCitiesByCountryByContinent;
        }
    }



    [Serializable]
    public class SerializedMapData
    {
        public string name;
        public MapData mapData;
        public SerializedMapData(MapData mapData)
        {
            this.mapData = mapData;
            name = mapData.name;
            Populate();
        }
        public void Populate()
        {
            // check to see if there are any children to populate 
            if (mapData.transform.childCount == 0)
            {
                return;
            }
            // create dynamic list of all child mapdata content 
            List<MapData> childDataList = new();
            foreach (Transform child in mapData.transform)
            {
                if (child.TryGetComponent(out MapData childData))
                {
                    childDataList.Add(childData);
                }
            }
            // convert list to array, and create serialized childData from array data
            MapData[] childDataArray = childDataList.ToArray();
            SerializedMapData[] newChildData = new SerializedMapData[childDataArray.Length];
            for (int i = 0; i < newChildData.Length; i++)
            {
                newChildData[i] = CreateSerializedMapData(childDataArray[i]);
            }
            GeneratedPopulation(newChildData);
        }
        protected virtual SerializedMapData CreateSerializedMapData(MapData mapData) { return null; }
        protected virtual void GeneratedPopulation(SerializedMapData[] childData) { }
    }

    [Serializable]
    public class World : SerializedMapData
    {
        public World(MapData mapData) : base(mapData) { if (name == mapData.name || string.IsNullOrEmpty(name)) { name = "World"; } }
        public Continent[] continents;
        protected override void GeneratedPopulation(SerializedMapData[] childData) { continents = Array.ConvertAll(childData, i => (Continent)i); }
        protected override SerializedMapData CreateSerializedMapData(MapData mapData) { return new Continent(mapData); }
        public Country[] countries
        {
            get
            {
                List<Country> countries = new();
                foreach (Continent continent in continents)
                {
                    countries.AddRange(continent.countries);
                }
                return countries.ToArray();
            }
        }
        public City[] cities
        {
            get
            {
                List<City> cities = new();
                foreach (Country country in countries)
                {
                    cities.AddRange(country.cities);
                }
                return cities.ToArray();
            }
        }
    }
    [Serializable]
    public class Continent : SerializedMapData
    {
        public Continent(MapData mapData) : base(mapData) { }
        public Country[] countries;
        protected override void GeneratedPopulation(SerializedMapData[] childData) { countries = Array.ConvertAll(childData, i => (Country)i); }
        protected override SerializedMapData CreateSerializedMapData(MapData mapData) { return new Country(mapData); }
        public City[] cities
        {
            get
            {
                List<City> cities = new();
                foreach (Country country in countries)
                {
                    cities.AddRange(country.cities);
                }
                return cities.ToArray();
            }
        }
    }
    [Serializable]
    public class Country : SerializedMapData
    {
        public Country(MapData mapData) : base(mapData) { }
        public City[] cities;
        protected override void GeneratedPopulation(SerializedMapData[] childData) { cities = Array.ConvertAll(childData, i => (City)i); }
        protected override SerializedMapData CreateSerializedMapData(MapData mapData) { return new City(mapData); }
    }
    [Serializable]
    public class City : SerializedMapData
    {
        public City(MapData mapData) : base(mapData) { }
    }
}
