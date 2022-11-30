using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Dataset
{
    /// <summary> name of this dataset </summary>
    public string name;
    #if UNITY_EDITOR
    [UnityEngine.Multiline]
    #endif
    public string description;
    
    public string[] identifiers;
}