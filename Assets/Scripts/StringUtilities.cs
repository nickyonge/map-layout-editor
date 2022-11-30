using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

/// <summary> Helpful methods for processing strings. Uses Regex </summary>
public static class StringUtilities
{

    /// <summary> Regex for removing whitespace from a string,
    /// including tabs, newlines/returns, etc </summary>
    private static readonly Regex regexRemoveWhitespaceFull = new(@"\s+");
    /// <summary> Regex for removing whitespace from a string,
    /// excluding tabs, newlines/returns, etc </summary>
    private static readonly Regex regexRemoveWhitespaceSpaceOnly = new("[ ]");
    /// <summary> Regex for removing all non-alphanumeric from a string </summary>
    private static readonly Regex regexAlphaNum = new("[^a-zA-Z0-9]");
    /// <summary> Regex for removing all non-alphanumeric from a string, 
    /// except for spaces, underscores, and dashes </summary>
    private static readonly Regex regexAlphaNumWithSpaceUnderscoreDash = new("[^a-zA-Z0-9 _-]");


    /// <summary> Removes accents from the given input string, and optionally trims it </summary>
    /// <param name="text"> String to remove accents from </param>
    /// <param name="trim"> Also trim the text? Default true </param>
    /// <returns> Input string, without accents </returns>
    public static string RemoveAccents(this string text, bool trim = true)
    {
        // check for trim 
        if (trim)
        {
            text = text.Trim();
        }
        // check if already normalized before processing text 
        if (text.IsNormalized(NormalizationForm.FormD))
        {
            return text;
        }
        // prep string builder, normalize text 
        StringBuilder stringBuilder = new();
        char[] chars = text.Normalize(NormalizationForm.FormD).ToCharArray();
        // iterate through chars, append non-spacing marks (ensure accent removal)
        foreach (char c in chars)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        return stringBuilder.ToString();
    }

    /// <summary> Removes all whitespace from the given string  </summary>
    /// <param name="text"> String to process </param>
    /// <param name="removeAllWhitespaceTypes"> Remove all types, including tabs, newlines, etc? 
    /// If false, only removes space characters. Default true </param>
    /// <returns> String without whitespace </returns>
    public static string RemoveWhitespace(this string text, bool removeAllWhitespaceTypes = true)
    {
        return removeAllWhitespaceTypes ?
         regexRemoveWhitespaceFull.Replace(text, "") :
         regexRemoveWhitespaceSpaceOnly.Replace(text, "");
    }

    /// <summary> Remove all non-alphanumeric characters (a-z, A-Z, 0-9) from the given string. 
    /// Optionally also remove spaces/dashes/underscores </summary>
    /// <param name="text"> String to process </param>
    /// <param name="removeSpacesDashesUnderscores"> Should spaces, dashes, and underscores also be removed? Default true </param>
    /// <returns> String without non-alphanumeric characters </returns>
    public static string RemoveNonAlphaNumeric(this string text, bool removeSpacesDashesUnderscores = true)
    {
        return removeSpacesDashesUnderscores ?
            regexAlphaNum.Replace(text, "") :
            regexAlphaNumWithSpaceUnderscoreDash.Replace(text, "");
    }

    /// <summary>
    /// Simplifies string, removes all accents, non-alphanumeric characters, and whitespace
    /// </summary>
    /// <param name="text"> String to simplify </param>
    /// <returns> Simplified string (no accents, no whitespace, lowercase) </returns>
    public static string SimplifyString(this string text)
    {
        return text.RemoveAccents(false).RemoveNonAlphaNumeric().ToLower();

    }
}