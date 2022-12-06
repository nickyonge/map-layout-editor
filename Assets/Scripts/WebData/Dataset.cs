using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class Dataset
{
    public enum DataFormat { CSV, JSON, XML, XLS, XLSX, ERROR }

    public enum DataScope { City = 0, Country = 1, Continent = 2, Other = 3 }

    /// <summary> name of this dataset </summary>
    public string fileName;

    public DataFormat format = DataFormat.CSV;

    public DataScope scope;

    public TextAsset dataFile;

    public string actualName;

#if UNITY_EDITOR
    [UnityEngine.Multiline]
#endif
    public string description;

    public string[] indicators;

    public SampleData[] sampleData;

    public int entries;

    public string containingFolder;

    private bool _errorGenerated = false;


    [Serializable]
    public struct SampleData
    {
        public string indicator;
        public string value;
        public SampleData(string indicator, string value)
        {
            this.indicator = indicator;
            this.value = value;
        }
    }

    public void LoadData(StreamReader streamReader)
    {
        // get indicators 
        int currentLine = DataManager.instance.loadingParams.GetInitialDataLine(this);

        char delimiter = fileName.ToLower().IndexOf("semicolon") > 0 ? ';' : ',';

        string[] preDataText = new string[currentLine + 1];
        List<string[]> preDataLinesList = new();
        int columns = -1;
        for (int i = 0; i < preDataText.Length; i++)
        {
            preDataText[i] = streamReader.ReadLine();
            // ensure we didn't overrun the file 
            if (preDataText[i] == null)
            {
                Debug.LogError("ERROR: readline returned null, presumably we've already exceeded the dataset height. \n" +
                    $"FileName: {fileName}, PreDataLines: {currentLine}, ErrorLine: {i}", dataFile);
                return;
            }
            string[] newData = preDataText[i].Split(delimiter);
            preDataLinesList.Add(newData);
            if (i == 0 || columns == -1)
            {
                columns = newData.Length;
            }
            else if (newData.Length != columns)
            {
                Debug.LogError($"ERROR: Column count mismatch on line {i} in file {fileName}\n" +
                    $"Col:{columns},LineCount:{newData.Length}, investigate", dataFile);
                return;
            }
        }
        // 2D array index corner references (L = length) 
        // 0/0=top-left, L/0=bottom-left, 0/L=top-right, L/L=bottom-right 
        indicators = new string[columns];
        if (currentLine <= 0)
        {
            // single line of info, read indicators directly 
            currentLine = 1;
            indicators = preDataLinesList[0];
        }
        else
        {
            string[][] preData = preDataLinesList.ToArray();
            for (int i = 0; i < columns; i++)
            {
                indicators[i] = "";
                for (int j = preData.Length - 1; j >= 0; j--)
                {
                    if (!string.IsNullOrWhiteSpace(preData[j][i]))
                    {
                        indicators[i] = preData[j][i];
                        break;
                    }
                }
            }
        }
        // ensure that indicators are properly formatted
        for (int i = 0; i < indicators.Length; i++)
        {
            // ensure indicator doesn't start with invalid char 
            while (
                indicators[i].StartsWith('_') || indicators[i].StartsWith('-') || indicators[i].StartsWith('[') ||
                indicators[i].StartsWith('{') || indicators[i].StartsWith('(') || indicators[i].StartsWith('"') ||
                indicators[i].StartsWith("'") || indicators[i].StartsWith('.') || indicators[i].StartsWith(';') ||
                indicators[i].StartsWith(' '))
            { indicators[i] = indicators[i].Substring(1); }
            // ensure indicator doesn't end with invalid char 
            while (
                indicators[i].EndsWith('_') || indicators[i].EndsWith('-') || indicators[i].EndsWith(']') ||
                indicators[i].EndsWith('}') || indicators[i].EndsWith(')') || indicators[i].EndsWith('"') ||
                indicators[i].EndsWith("'") || indicators[i].EndsWith('.') || indicators[i].EndsWith(';') ||
                indicators[i].EndsWith(' '))
            { indicators[i] = indicators[i].Substring(0, indicators[i].Length - 1); }
        }

        // get indicator line 
        // remove invalid indicators 
        List<string> listIndicators = new();
        // generate sample data 
        sampleData = new SampleData[indicators.Length];
        int index = 0;
        foreach (string s in indicators)
        {
            sampleData[index] = new SampleData(s, null);
            index++;
        }
        // reference to data that's already been loaded 
        bool[] loadedData = new bool[sampleData.Length];

        int failsafe = Mathf.Max(entries, 0) + 1;// ensure we don't get stuck 
        while (failsafe > 0)
        {
            failsafe--;
            if (failsafe < 0)
            {
                Debug.LogWarning("WARNING: exceeded entries while reading dataset: " +
                    $"{fileName}, this should be impossible, investigate data. Safely escaping while loop.", dataFile);
                break;
            }
            string dataLine = streamReader.ReadLine();
            currentLine++;
            if (dataLine == null)
            {
                // hit the end of the document 
                break;
            }
            if (string.IsNullOrWhiteSpace(dataLine))
            {
                // empty line 
                continue;
            }
            // we've got a line! parse it 
            string[] data = ParseLine(dataLine, delimiter);
            // iterate through loaded data to apply parsed info 
            for (int i = 0; i < loadedData.Length; i++)
            {
                if (!loadedData[i])
                {
                    // data is not yet loaded, so check if we can load it! 
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(data[i]))
                        {
                            // yup, data found :)
                            sampleData[i].value = data[i];
                            loadedData[i] = true;
                            // check if all data is fulfilled 
                            if (!Array.Exists(loadedData, element => !element))
                            {
                                // all data loaded! 
                                break;
                            }
                        }
                    }
                    catch
                    {
                        Debug.LogError($"ERROR: Failed to read line {currentLine}: {dataLine}\n" +
                            $"Columns (data.Length): {data.Length}, loadedData.Length: {loadedData.Length}, current index: {i}", dataFile);
                    }
                }
            }
        }
        // presumably done loading data, check if all data is true 
        if (Array.Exists(loadedData, element => !element))
        {
            // some data is still false, check for soft field indicators 
            // get false field indicators 
            bool b = fileName.IndexOf("ump_rateedu") >= 0;
            List<Tuple<string, int>> falseFieldIndicators = new();
            for (int i = 0; i < loadedData.Length; i++)
            {
                if (!loadedData[i])
                {
                    if (b)
                    {
                        Debug.Log("INDICATOR FOUND BAD " + i + ":" + indicators[i]);
                    }
                    falseFieldIndicators.Add(new Tuple<string, int>(indicators[i], i));
                }
            }
            // got false indicators, check if they're listed 
            List<int> removeIndicators = new();
            if (b) Debug.Log("FFI TUPS LENGTH: " + falseFieldIndicators.Count);
            foreach (Tuple<string, int> indicator in falseFieldIndicators)
            {
                if (b) Debug.Log(1);
                if (DataManager.instance.loadingParams.softIgnoreIndicators.Contains(indicator.Item1))
                {
                    if (b) Debug.Log(2 + ", Item1: " + indicator.Item1 + ", Item2: " + indicator.Item2);
                    // yup! indicator is false field, slate it to be removed 
                    removeIndicators.Add(indicator.Item2);
                }
            }
            // check if any indicators are slated to be removed 
            if (b) Debug.Log("REMOVE COUNT: " + removeIndicators.Count);
            if (removeIndicators.Count > 0)
            {
                if (b) Debug.Log("COUNT0: " + removeIndicators[0]);
                // iterate through and remove necessary indicators from: indicators, loadedData, sampleData
                // create temp arrays to hold new data 
                string[] newIndicators = new string[indicators.Length - removeIndicators.Count];
                bool[] newLoadedData = new bool[loadedData.Length - removeIndicators.Count];
                SampleData[] newSampleData = new SampleData[sampleData.Length - removeIndicators.Count];
                index = 0;
                // iterate thru old indicators 
                for (int i = 0; i < indicators.Length; i++)
                {
                    // check for indicator to be removed, and if so, skip 
                    if (removeIndicators.Contains(i))
                    {
                        removeIndicators.Remove(i);
                        if (b) Debug.Log("SKIPPING: " + indicators[i] + "," + i);
                        continue;
                    }
                    if (b) Debug.Log("ADDING: " + indicators[i] + "," + i);
                    newIndicators[index] = indicators[i];
                    newSampleData[index] = sampleData[i];
                    newLoadedData[index] = loadedData[i];
                    index++;
                }
                // update arrays 
                indicators = newIndicators;
                loadedData = newLoadedData;
                sampleData = newSampleData;
                newSampleData = new SampleData[0];// GC failsafe for struct 
            }
            // done, check again for non-soft false indicators 
            if (Array.Exists(loadedData, element => !element))
            {
                string s = "Index : LoadedData : Indicator";
                for (int i = 0; i < indicators.Length; i++)
                {
                    s += "\n" + i.ToString() + " : " + loadedData[i] + " : " + indicators[i];
                }
                Debug.LogWarning($"WARNING: some data is still false in the dataset {fileName}" +
                    $"\nFurther details in this warning message \n{s}", dataFile);
            }
        }
    }

    public string[] ParseLine(string input, char delimiter = ',')
    {
        switch (format)
        {
            case DataFormat.CSV:
                return input.Split(delimiter);
            case DataFormat.JSON:
            case DataFormat.XML:
            case DataFormat.XLS:
            case DataFormat.XLSX:
#pragma warning disable CS0162
                if (DataManager.DEBUG_UNIMPLEMENTED_FORMATS)
                {
                    if (!_errorGenerated)
                    {
                        Debug.LogWarning($"WARNING: unimplemented DataFormat {format}, " +
                            $"can't parse line data for dataset {fileName}, returning '{delimiter}' split", dataFile);
                        _errorGenerated = true;
                    }
                }
                return input.Split(delimiter);
#pragma warning restore CS0162
            default:
                if (!_errorGenerated)
                {
                    Debug.LogError($"ERROR: invalid DataFormat {format}, " +
                        $"can't parse line data for dataset {fileName}, returning '{delimiter}' split", dataFile);
                    _errorGenerated = true;
                }
                return input.Split(delimiter);
        }
    }

    public string filePath;
}