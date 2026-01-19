

using System.Text.Json;
using docGenerator;

HtmlParser parser = new();
var headers = await parser.GetModuleHeaders();

List<Module> modules = new();
foreach (var header in headers)
{
    var module = await parser.GetModule(header);
    modules.Add(module);
}

CatsDocConverter converter = new();
string docs = converter.GetDocs(modules);
Directory.CreateDirectory("../../../../../docs");
File.WriteAllText("../../../../../docs/theoDocs.lua", docs);
File.WriteAllText("../../../../../docs/settings.json", File.ReadAllText("settings.json"));
File.WriteAllText("../../../../../docs/patches.lua", File.ReadAllText("patches.lua"));