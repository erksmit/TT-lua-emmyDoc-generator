

using System.Text.Json;
using docGenerator;
using HtmlAgilityPack;

HtmlParser parser = new();
var headers = await parser.GetModuleHeaders();

List<Module> modules = new();
foreach (var header in headers)
{
    var module = await parser.GetModule(header);
    modules.Add(module);
}
string json = JsonSerializer.Serialize(modules);
Console.WriteLine(json);