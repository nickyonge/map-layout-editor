using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayUtils
{
    /// <summary> Check if the given array contains any instance of the given value </summary>
    /// <param name="array"> Array to inspect </param>
    /// <param name="value"> Value to look for </param>
    /// <returns> True if the given array contains any instance of the given value </returns>
    public static bool Contains<T>(this T[] array, T value)
    {
        // check if generic elements exist within array 
        return array != null && array.Length > 0 && Array.Exists(array, element =>
            EqualityComparer<T>.Default.Equals(element, value));
    }
    public static bool ContainsAnyByIndex(this string[] stringArray, string value)
    {
        return Array.Exists(stringArray, element => value.IndexOf(element) >= 0) ||
            Array.Exists(stringArray, element => element.IndexOf(value) >= 0);
    }
    public static bool ContainsAnyByIndex(this string[] stringArray, string[] values)
    {
        foreach (string value in values)
        {
            if (Array.Exists(stringArray, element => value.IndexOf(element) >= 0) ||
                Array.Exists(stringArray, element => element.IndexOf(value) >= 0))
                return true;
        }
        return false;
    }
}