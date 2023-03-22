using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataUtils
{

    public const bool WARN_CSV_QUOTE_SUS_VALUES = true;
    private const bool WARN_CSV_QUOTE_MID_CELL_QUOTES = true;

    /// <summary> value that's used to temporarily replace delimiters while parsing CSVs </summary>
    private const string REPLACE_DELIM = "[|DELIM|]";

    private static bool _errorGenerated = false;

    /// <summary> </summary>
    /// <param name="input"> text line to parse </param>
    /// <param name="fileName"> name of the file that's being parse </param>
    /// <param name="delimiter"> character to split text array </param>
    /// <param name="format"> filetype form </param>
    /// <param name="dataFile"> TextAsset associated with this line </param>
    /// <param name="currentLine"> current line being read </param>
    /// <param name="columns"> number of columns in this dataset </param>
    /// <returns> </returns>
    public static string[] ParseLine(
        string input,
        string fileName,
        char delimiter,
        DataFormat format,
        TextAsset dataFile,
        int currentLine,
        int columns = -1
        )
    {
        switch (format)
        {
            case DataFormat.CSV:
                // determine if there are any quoted values, which may include the delimiter
                int index = input.IndexOf('"');
                if (index >= 0)
                {
                    bool multiQuoteCellProcessed = false;
                    int failsafe = input.Length + 1;
                    string rawInput = input;
                    while (index >= 0 && failsafe > 0)
                    {
                        failsafe--;

                        int closing = input.IndexOf('"', index + 1);
                        if (closing < 0)
                        {
                            // no closing quote, issue warning if needed 
                            if (WARN_CSV_QUOTE_SUS_VALUES)
                            {
                                Debug.LogWarning($"WARNING: found a start quote but no closing quote in {fileName}, " +
                                    $"at index {index}, \nwhile parsing line {currentLine} string <b>[{rawInput}]</b>, investigate", dataFile);
                            }
                            break;
                        }
                        // determine if the full value is quoted 
                        bool validInitialQuote = index == 0 || input[index - 1] == delimiter;
                        bool validClosingQuote = false;
                        if (validInitialQuote)
                        {
                            validClosingQuote = closing == input.Length - 1 || input[closing + 1] == delimiter;
                            if (!validClosingQuote)
                            {
                                // it's feasible that the quoted value has multiple quotations within it, 
                                // search for next quote plus delimiter 
                                closing = input.IndexOf(new string(new char[] { '"', delimiter }), index + 1);
                                validClosingQuote = closing != -1;
                                if (!validClosingQuote)
                                {
                                    // still invalid, check if the very last field ends with a quote 
                                    closing = input.LastIndexOf('"', index + 1);
                                    validClosingQuote = closing != -1 && closing == input.Length - 1;
                                }
                                if (validClosingQuote)
                                {
                                    multiQuoteCellProcessed = true;
                                }
                            }
                        }
                        // confirm errors for beginning/end of quote 
                        if (!validInitialQuote || !validClosingQuote)
                        {
                            // only a partial amount of the value has a quote
                            if (WARN_CSV_QUOTE_MID_CELL_QUOTES)
                            {
                                Debug.LogWarning($"WARNING: found a value in {fileName} that is not immediatly " +
                                    (validInitialQuote ? "followed" : validClosingQuote ? "preceded" : "followed by OR proceeded") +
                                    $" by delimiter [{delimiter}], \nstart index {index}, " +
                                    $"closing index {closing}, line {currentLine}, string: <b>[{rawInput}]</b>, investigate", dataFile);
                            }
                            break;
                        }
                        // substring the value and check if it contains a delimiter
                        string value = input.Substring(index + 1, closing - (index + 1));
                        if (value.IndexOf(delimiter) == -1)
                        {
                            // nope! no delimiter found, update index and carry on
                            if (closing + 1 >= input.Length)
                            {
                                // done
                                break;
                            }
                            index = input.IndexOf('"', closing + 1);
                            continue;
                        }
                        string before = index == 0 ? "" : input.Substring(0, index);
                        string after = closing >= input.Length - 1 ? "" : input.Substring(closing + 1);
                        // we've got a full value quoted, check for and replace delimiter 
                        int failsafe2 = value.Length + 1;
                        while (value.IndexOf(delimiter) >= 0 && failsafe2 > 0)
                        {
                            failsafe2--;
                            value = value.Replace(delimiter.ToString(), REPLACE_DELIM);
                        }
                        input = string.Concat(before, value, after);
                        index = input.IndexOf('"', before.Length + value.Length);
                    }
                    // done! split apart the input based on the delimiter 
                    string[] split = input.Split(delimiter);
                    // ensure column count still matches if multi-quote cell processing occurred 
                    if (columns != -1 && split.Length != columns)
                    {
                        if (multiQuoteCellProcessed)
                        {
                            Debug.LogWarning("WARNING: column count mismatch after multi-quote-cell " +
                                $"line processed, delimiter: [{delimiter}], line cols: {split.Length}, actual cols: <b>{columns}</b>,\n" +
                                $"Line: [{rawInput}]\nPost-processed line: [{input}]", dataFile);
                        }
                        else
                        {
                            Debug.LogWarning("WARNING: column count mismatch DESPITE NO multi-quote-cell " +
                                $"line processed, delimiter: [{delimiter}], line cols: {split.Length}, actual cols: <b>{columns}</b>,\n" +
                                $"Line: [{rawInput}]\nPost-processed line: [{input}]", dataFile);
                        }
                    }
                    // iterate thru split and replace delimited values 
                    for (int i = 0; i < split.Length; i++)
                    {
                        int failsafe3 = split[i].Length;
                        while (split[i].IndexOf(REPLACE_DELIM) >= 0)
                        {
                            split[i] = split[i].Replace(REPLACE_DELIM, delimiter.ToString());
                        }
                    }
                    // done! 
                    return split;
                }
                // no quotes found, assume correct delimiters 
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

    /// <summary>
    /// parse array of data into individual arrays (handle multi-value cells)
    /// </summary>
    /// <param name="inputArray"> array to convert </param>
    /// <param name="outputArray"> outputs null if false (no multi-value cells found, no convert if) </param>
    /// <param name="delimiter"> character used to separate cell values </param>
    /// <param name="convertEvenIfFalse"> if true, will always return true and output array,
    /// even if no multi-value cells are found. </param>
    /// <returns> true if a multivalue array is found, and outputs data to output array.
    /// if convertEvenIfFalse is true, this will always return true </returns>
    public static bool ParseDataArrays(
        string[] inputArray, out string[][] outputArray,
        char delimiter = ',', bool convertEvenIfFalse = false)
    {
        // check if data has any multi-element values 
        if (!convertEvenIfFalse)
        {
            bool exit = true;
            foreach (string cell in inputArray)
            {
                if (IsStringMultiValueCell(cell, delimiter))
                {
                    exit = false;
                    break;
                }
            }
            if (exit)
            {
                outputArray = null;
                return false;
            }
        }
        outputArray = new string[inputArray.Length][];
        for (int i = 0; i < inputArray.Length; i++)
        {
            if (IsStringMultiValueCell(inputArray[i], delimiter))
            {
                // yep, multi-value array 
                // substring quotes from front/back, split along delimiter 
                outputArray[i] = inputArray[i].Substring(0, inputArray[i].Length - 2).Split(delimiter);
            }
            else
            {
                // nope, just use input array as the value 
                outputArray[i] = new string[] { inputArray[i] };
            }
        }
        return true;
    }
    private static bool IsStringMultiValueCell(string input, char delimiter)
    {
        return (input.Length > 2 && input[0] == '"' &&
            input[input.Length - 1] == '"' &&
            input.IndexOf(delimiter) > 0 &&
            input.LastIndexOf(delimiter) < input.Length - 1);
    }

}