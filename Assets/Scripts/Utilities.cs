using System.Text;
using System.Globalization;

public static class Utilities
{

    /// <summary>
    /// Removes accents from the given input string 
    /// </summary>
    /// <param name="text"> String to convert </param>
    /// <returns> Input string, without accents </returns>
    public static string RemoveAccents(this string text)
    {
        StringBuilder stringBuilder = new StringBuilder();
        char[] chars = text.Normalize(NormalizationForm.FormD).ToCharArray();
        foreach (char c in chars) {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) {
                stringBuilder.Append(c);
            }
        }
        return stringBuilder.ToString();
    }
}