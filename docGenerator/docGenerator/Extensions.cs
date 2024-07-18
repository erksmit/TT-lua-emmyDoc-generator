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

    public static string TrimAll(this string str)
    {
        return str.Trim().Replace("\n", " ").Replace("  ", " ");
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
}