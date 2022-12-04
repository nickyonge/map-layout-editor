using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Dataset
{
    public enum DataFormat { CSV, JSON, XML, XLS, XLSX, ERROR }

    /// <summary> name of this dataset </summary>
    public string fileName;

    public DataFormat format = DataFormat.CSV;

    public TextAsset dataFile;

    public string actualName;

#if UNITY_EDITOR
    [UnityEngine.Multiline]
#endif
    public string description;
    
    public string[] identifiers;

    public string filePath;
}