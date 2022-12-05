using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    public string[] indicators;
    public SampleData[] sampleData;

    public int entries;

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
        int currentLine = 0;
        string unformattedIndicatorLine = streamReader.ReadLine();
        string[] rawIndicators = ParseLine(unformattedIndicatorLine);
        // remove invalid indicators 
        List<string> listIndicators = new();
        int[] cleanIndices = new int[rawIndicators.Length];
        for (int i = 0; i < rawIndicators.Length; i++)
        {
            cleanIndices[i] = -1;
            // generate warning for empty IDs 
            bool isBlank = false;
            if (string.IsNullOrWhiteSpace(rawIndicators[i]))
            {
                isBlank = true;
                Debug.LogWarning($"WARNING: Column {i} in {fileName} is blank, " +
                    "consider naming this indicator");
            }
            // add unique values 
            if (isBlank || !listIndicators.Contains(rawIndicators[i]))
            {
                cleanIndices[i] = listIndicators.Count;
                listIndicators.Add(rawIndicators[i]);
            }
        }
        if (listIndicators.Count == 0)
        {
            Debug.LogError($"ERROR: After cleaning, NO valid indicators in {format} dataset: "
                + $"{fileName}, investigate or ignore this dataset. Raw Indicator line:\n{unformattedIndicatorLine}");
            return;
        }
        indicators = listIndicators.ToArray();
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
        for (int i = 0; i < loadedData.Length; i++)
        {
            // use clean index reference to ignore unnecessary raw columns 
            loadedData[i] = !Array.Exists(cleanIndices, element => element == i);
        }

        int failsafe = Mathf.Max(entries, 0) + 1;// ensure we don't get stuck 
        while (failsafe > 0)
        {
            failsafe--;
            if (failsafe < 0)
            {
                Debug.LogWarning("WARNING: exceeded entries while reading dataset: " +
                    $"{fileName}, this should be impossible, investigate data. Safely escaping while loop.");
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
            string[] data = ParseLine(dataLine);
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
                            int cleanIndex = cleanIndices[i];
                            sampleData[cleanIndex].value = data[i];
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
            // some data is still false :( 
            Debug.LogWarning($"WARNING: some data is still false in the dataset {fileName}");
            string s = "Index : CleanIndex : LoadedData : RawIndicator\n\n";
            for (int i = 0; i < rawIndicators.Length; i++)
            {
                s += "\n" + i.ToString() + " : " + cleanIndices[i] + " : " + loadedData[i] + " : " + rawIndicators[i];
            }
        }
    }

    public string[] ParseLine(string input)
    {
        switch (format)
        {
            case DataFormat.CSV:
                return input.Split(',');
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
                            $"can't parse line data for dataset {fileName}, returning ',' split");
                        _errorGenerated = true;
                    }
                }
                return input.Split(',');
#pragma warning restore CS0162
            default:
                if (!_errorGenerated)
                {
                    Debug.LogError($"ERROR: invalid DataFormat {format}, " +
                        $"can't parse line data for dataset {fileName}, returning ',' split");
                    _errorGenerated = true;
                }
                return input.Split(',');
        }
    }

    public string filePath;
}