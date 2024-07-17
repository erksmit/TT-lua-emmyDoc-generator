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
}