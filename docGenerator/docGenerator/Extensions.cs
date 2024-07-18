using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace docGenerator;

public static class Extensions
{
    public static HtmlNode? NextSiblingWhere(this HtmlNode node, Predicate<HtmlNode> predicate)
    {
        var currentNode = node;
        while (currentNode.NextSibling != null)
        {
            if (predicate(currentNode))
                return currentNode;
            currentNode = currentNode.NextSibling;
        }

        return null;
    }

    static readonly Regex trimmer = new Regex(@"\s\s+");
    public static string TrimAll(this string str)
    {
        str = str.Trim().Replace("\n", " ");
        return trimmer.Replace(str, " ");
    }

    public static string RemoveChar(this string str, params char[] characters)
    {
        string result = str;
        foreach (var chr in characters)
        {
            result = result.Replace(chr.ToString(), string.Empty);
        }

        return result;
    }
    
    public static string FirstLetterToLower(this string source)
    {
        if (source == "GUI")
            return "gui";
        
        if (string.IsNullOrEmpty(source))
            return string.Empty;
        // convert to char array of the string
        char[] letters = source.ToCharArray();
        // upper case the first char
        letters[0] = char.ToLower(letters[0]);
        // return the array made of the new char array
        return new string(letters);
    }

    public static StringBuilder AppendSpace(this StringBuilder builder, string? str = null)
    {
        return builder.Append(str).Append(' ');
    }
    
    public static StringBuilder AppendSpace(this StringBuilder builder, char chr)
    {
        return builder.Append(chr).Append(' ');
    }
}