using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MapDataCollector : MonoBehaviour
{
    public static MapDataCollector instance;


    public World world;


    [Header("Setup")]
    [SerializeField] private bool _collectDataOnInit = true;


    private void Start()
    {
        Initialize();
    }
    private void Initialize()
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
    }
    [Serializable]
    public class Continent : SerializedMapData
    {
        public Continent(MapData mapData) : base(mapData) { }
        public Country[] countries;
        protected override void GeneratedPopulation(SerializedMapData[] childData) { countries = Array.ConvertAll(childData, i => (Country)i); }
        protected override SerializedMapData CreateSerializedMapData(MapData mapData) { return new Country(mapData); }
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
