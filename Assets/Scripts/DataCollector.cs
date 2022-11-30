using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataCollector : MonoBehaviour
{
    public static DataCollector instance;

    [Header("Data Tree")]
    public DataTree data;


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
                    Destroy(instance.gameObject);
                }
                else
                {
                    DestroyImmediate(instance.gameObject);
                }
            }
            return;
        }
        instance = this;

        if (_collectDataOnInit)
        {
            Initialize();
        }
    }

    /// <summary>
    /// Iterates through all child GameObjects of this script, identifying and 
    /// sorting through continents, then countries, then cities 
    /// </summary>
    public void CollectData()
    {

    }

    /// <summary> Nested data of all continents, countries, and cities
    /// derived from the in-engine created map </summary>
    [Serializable]
    public class DataTree
    {
        public Continent[] continents;

        public class Continent
        {
            public Continent(MD_Continent mapData) {
                this.mapData = mapData;
            }
            public string name;
            public MD_Continent mapData;
            public Country[] countries;
        }
        public class Country
        {
            public string name;
            public MD_Country mapData;
            public City[] cities;
        }
        public class City
        {
            public string name;
            public MD_City mapData;
        }
    }

}
