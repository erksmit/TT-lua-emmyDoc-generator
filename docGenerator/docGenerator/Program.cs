

using docGenerator;
using HtmlAgilityPack;

HtmlParser parser = new();
var headers = await parser.GetModuleHeaders();

foreach (var header in headers)
{
    var module = await parser.GetModule(header);
}
