using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataStructs
{

    [Serializable]
    public struct MapReference
    {
        public string basicName;
        public string simpleName;
        public string[] Names { get { return new string[] { basicName, simpleName }; } }

        public MapReference(MapDataCollector.SerializedMapData mapData)
        {
            basicName = mapData.name;
            simpleName = basicName.SimplifyString();
        }
        public MapReference(string name) {
            basicName = name;
            simpleName = basicName.SimplifyString();
        }
    }

    [Serializable]
    public struct InternalReference_City
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
    public struct InternalReference_Country
    {
        [Header("Export Fields")]
        public string asciiName;
        public string isoA2;
        public string isoA3;
        public int m49;
        public string m49String
        {
            get { return m49.ToString("000"); }
        }
        public string continentCode;

        [Header("Internal Reference Fields")]
        public string baseName;
        public string simplifiedName;
        public string[] alternateNames;

    }

    [Serializable]
    public struct InternalReference_Continent
    {

        [Header("Export Fields")]
        public string name;
        public string code;
        [Header("Internal Reference Fields")]
        public string[] alternateNames;
    }

}