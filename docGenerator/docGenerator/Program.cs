

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
Directory.CreateDirectory("result");
File.WriteAllText("result/theoDocs.lua", docs);
File.WriteAllText("result/settings.json", """
                                   {
                                       "Lua.diagnostics.globals": [
                                           "Translation",
                                           "script"
                                       ],
                                       "Lua.diagnostics.disable": [
                                           "duplicate-set-field"
                                       ],
                                       "Lua.workspace.library": [
                                           ".vscode/theoDocs.lua"
                                       ],
                                       "files.associations": {
                                           "*.json": "jsonc"
                                       }
                                   }
                                   """);